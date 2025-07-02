using UnityEngine;
using EnemyStates;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using RootMotion.Dynamics;
using Sirenix.OdinInspector;

public class Enemy : MonoBehaviour
{
    public AnimationManager AnimationManager { get; private set; }

    public HealthManager HealthManager { get; private set; }

    public AIDestinationSetter AIDestinationSetter { get; private set; }

    public FollowerEntity FollowerEntity { get; private set; }

    // Reference to PuppetMaster component (could be on this GameObject or a sibling)
    [TabGroup("Setup")]
    [SerializeField] private PuppetMaster puppetMasterReference;
    public PuppetMaster PuppetMaster { get; private set; }

    [TabGroup("Setup")]
    [Required]
    [SerializeField]
    private Transform PlayerTransform;

    [TabGroup("Setup")]
    [SerializeField]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
    private List<Transform> patrolPoints;

    public StateMachine<EnemyState> stateMachine { get; private set; }

    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    [ShowIf("@stateMachine != null")]
    public string CurrentState => stateMachine?.currentState?.GetType().Name ?? "None";

    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    [ShowIf("@FollowerEntity != null")]
    public float CurrentSpeed => FollowerEntity?.maxSpeed ?? 0f;

    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    public EnemyState Idle { get; private set; }

    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    public EnemyState Alert { get; private set; }
    
    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    public EnemyState Patrol { get; private set; }

    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    public EnemyState Aggro { get; private set; }

    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    public EnemyState Chase { get; private set; }

    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    public EnemyState Attack { get; private set; }

    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    public EnemyState Death { get; private set; }

    [TabGroup("Configuration")]
    [Required]
    [AssetsOnly]
    public EnemyTemplate template;

    public Transform GetPlayerTransform() => PlayerTransform;
    public List<Transform> GetPatrolPoints() => patrolPoints;
    public float GetAlertRange() => template.alertRange;
    public float GetPatrolSpeed() => template.patrolSpeed;
    public float GetChaseSpeed() => template.chaseSpeed;

    public float GetAggroRange() => template.aggroRange;

    public float GetAttackRange() => template.attackRange;

    
    // Track if enemy has been aggroed before
    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    public bool HasAggroed { get; private set; } = false;
    
    public void SetHasAggroed(bool value)
    {
        HasAggroed = value;
        Debug.Log($"[{name}] HasAggroed set to {value}");
    }

    // Shared turn animation state across all states
    [TabGroup("Debug")]
    [ShowInInspector, ReadOnly]
    private bool _isTurning = false;
    public bool IsTurning 
    { 
        get => _isTurning;
        set 
        {
            if (_isTurning != value)
            {
                Debug.Log($"[{name}] ENEMY IsTurning changed from {_isTurning} to {value}");
                _isTurning = value;
            }
        }
    }

    // Speed blending fields
    private Coroutine speedBlendCoroutine;
    
    // Debug mode to override automatic state transitions
    [TabGroup("Debug")]
    [ShowInInspector]
    [InfoBox("When enabled, prevents automatic state transitions based on player distance. Allows manual state control.", InfoMessageType.Info)]
    public bool DebugModeEnabled = false;

    private void Awake()
    {
        SetupAnimator();
        SetupHealthManager();
        SetupAIDestinationSetter();
        SetupPuppetMaster();
        SetupFollowerEntity();

        stateMachine = new StateMachine<EnemyState>(gameObject.name);
    }

    private void SetupAnimator()
    {
        Animator animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component is missing on the Enemy GameObject.");
        }

        AnimationManager = new AnimationManager(animator);
    }

    private void SetupHealthManager()
    {
        HealthManager = GetComponent<HealthManager>();
        if (HealthManager == null)
        {
            Debug.LogError("HealthManager component is missing on the Enemy GameObject.");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] HealthManager initialized successfully.");
        }
    }

    private void SetupAIDestinationSetter()
    {
        AIDestinationSetter = GetComponent<AIDestinationSetter>();
        if (AIDestinationSetter == null)
        {
            Debug.LogError("AIDestinationSetter component is missing on the Enemy GameObject.");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] AIDestinationSetter initialized successfully.");
        }
    }

    private void SetupPuppetMaster()
    {
        // Use serialized reference first (best performance)
        if (puppetMasterReference != null)
        {
            PuppetMaster = puppetMasterReference;
            Debug.Log($"[{gameObject.name}] Using serialized PuppetMaster reference: {PuppetMaster.gameObject.name}");
            return;
        }

        // Fallback to automatic search if no reference is assigned
        Debug.Log($"[{gameObject.name}] No PuppetMaster reference assigned, searching automatically...");

        // Option 1: Try to find on this GameObject first (fastest)
        PuppetMaster = GetComponent<PuppetMaster>();

        if (PuppetMaster == null)
        {
            // Option 2: Look for it in the parent (moderate cost)
            PuppetMaster = GetComponentInParent<PuppetMaster>();
        }

        if (PuppetMaster == null)
        {
            // Option 3: Look for it in siblings (most expensive)
            if (transform.parent != null)
            {
                PuppetMaster = transform.parent.GetComponentInChildren<PuppetMaster>();
            }
        }

        if (PuppetMaster == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PuppetMaster component not found on this GameObject, parent, or siblings.");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] PuppetMaster found automatically on: {PuppetMaster.gameObject.name}");
        }
    }

    public void SetupFollowerEntity()
    {
        FollowerEntity = GetComponent<FollowerEntity>();
        if (FollowerEntity == null)
        {
            Debug.LogError("FollowerEntity component is missing on the Enemy GameObject.");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] FollowerEntity initialized successfully.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (template == null)
        {
            Debug.LogError("EnemyTemplate is not assigned. Please assign a template in the inspector.");
            return;
        }

        Debug.Log($"[{gameObject.name}] Enemy template loaded - patrolSpeed: {template.patrolSpeed}, chaseSpeed: {template.chaseSpeed}");

        // Initialize HealthManager with template data now that template is confirmed available
        if (HealthManager != null)
        {
            HealthManager.Initialize(template.maxHealth);
        }

        Debug.Log($"[{gameObject.name}] Initializing enemy states...");

        Idle = new IdleState(this, stateMachine, AnimationManager, "Idle");
        Debug.Log($"[{gameObject.name}] ✓ Idle state initialized");

        Alert = new AlertState(this, stateMachine, AnimationManager, "Alert");
        Debug.Log($"[{gameObject.name}] ✓ Alert state initialized");

        Patrol = new PatrolState(this, stateMachine, AnimationManager, "Locomotion (OH)");
        Debug.Log($"[{gameObject.name}] ✓ Patrol state initialized");

        Aggro = new AggroState(this, stateMachine, AnimationManager, "Aggro", PlayerTransform);
        Debug.Log($"[{gameObject.name}] ✓ Aggro state initialized");

        Chase = new ChaseState(this, stateMachine, AnimationManager, "Chase");
        Debug.Log($"[{gameObject.name}] ✓ Chase state initialized");

        Attack = new AttackState(this, stateMachine, AnimationManager, "Attack");
        Debug.Log($"[{gameObject.name}] ✓ Attack state initialized");

        Death = new DeathState(this, stateMachine, AnimationManager, "Death");
        Debug.Log($"[{gameObject.name}] ✓ Death state initialized");

        Debug.Log($"[{gameObject.name}] All states initialized. Setting initial state to Idle...");
        stateMachine.SetState(Idle);

        // Log initial speed state
        LogCurrentSpeed("Initial state after setup");
    }

    // Update is called once per frame
    private void Update()
    {
        stateMachine.LogicUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.PhysicsUpdate();
    }

    private void LateUpdate()
    {
        stateMachine.LateUpdate();
    }

    public void ProcessHit(int damage, Limb limb)
    {
        if (limb == null)
        {
            Debug.LogWarning("Limb component is null. Cannot process hit.");
            return;
        }

        limb.TakeDamage(damage);
        HealthManager.TakeDamage(damage);
        Debug.Log($"[{name}] Enemy.ProcessHit(): Took {damage} damage. Remaining health: {HealthManager.currentHealth}");

        if (HealthManager.currentHealth <= 0)
        {
            // Enter death state
            Debug.Log($"[{name}] Enemy.ProcessHit(): Has died");
            stateMachine.SetState(Death);
        }
    }

    public bool IsPlayerInAlertRange()
    {
        // If debug mode is enabled, return false to prevent automatic transitions
        if (DebugModeEnabled) return false;
        
        if (PlayerTransform == null)
        {
            Debug.LogWarning("PlayerTransform is not assigned.");
            return false;
        }

        float sqrDistanceToPlayer = (transform.position - PlayerTransform.position).sqrMagnitude;
        return sqrDistanceToPlayer <= template.alertRange * template.alertRange;
    }

    public bool IsPlayerInAggroRange()
    {
        // If debug mode is enabled, return false to prevent automatic transitions
        if (DebugModeEnabled) return false;
        
        if (PlayerTransform == null)
        {
            Debug.LogWarning("PlayerTransform is not assigned.");
            return false;
        }

        float sqrDistanceToPlayer = (transform.position - PlayerTransform.position).sqrMagnitude;
        return sqrDistanceToPlayer <= template.aggroRange * template.aggroRange;
    }

    public bool IsPlayerInAttackRange()
    {
        // If debug mode is enabled, return false to prevent automatic transitions
        if (DebugModeEnabled) return false;
        
        if (PlayerTransform == null)
        {
            Debug.LogWarning("PlayerTransform is not assigned.");
            return false;
        }

        float sqrDistanceToPlayer = (transform.position - PlayerTransform.position).sqrMagnitude;
        return sqrDistanceToPlayer <= template.attackRange * template.attackRange;
    }

    public void OnTurnFinished()
    {
        Debug.Log($"[{name}] Enemy.OnTurnFinished(): Turn animation finished. Current state: {stateMachine.currentState.GetType().Name}");
        // This method is called directly from animation events
        // Delegate to the current state if it's AlertState
        if (stateMachine.currentState == Alert && Alert is AlertState alertState)
        {
            Debug.Log($"[{name}] Enemy.OnTurnFinished(): Delegating to AlertState");
            alertState.OnTurnFinished();
        }
        else if (stateMachine.currentState == Aggro && Aggro is AggroState aggroState)
        {
            Debug.Log($"[{name}] Enemy.OnTurnFinished(): Delegating to AggroState");
            aggroState.OnTurnFinished();
        }
        else
        {
            Debug.Log($"[{name}] Enemy.OnTurnFinished(): Called but current state ({stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnAggroAnimationFinished()
    {
        Debug.Log($"[{name}] Enemy.OnAggroAnimationFinished(): Current state: " + stateMachine.currentState.GetType().Name);

        if (stateMachine.currentState == Aggro && Aggro is AggroState aggroState)
        {
            Debug.Log($"[{name}] Enemy.OnAggroAnimationFinished(): Delegating to AggroState");
            aggroState.OnAggroAnimationFinished();
        }
        else
        {
            Debug.LogWarning($"[{name}] Enemy.OnAggroAnimationFinished(): Called but current state ({stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    // Utility methods for debugging speed issues
    public void LogCurrentSpeed(string context = "")
    {
        var followerEntity = GetComponent<FollowerEntity>();
        if (followerEntity != null)
        {
            Debug.Log($"[{gameObject.name}] SPEED DEBUG{(string.IsNullOrEmpty(context) ? "" : $" ({context})")}: " +
                     $"FollowerEntity.maxSpeed = {followerEntity.maxSpeed}, " +
                     $"Current State = {stateMachine.currentState?.GetType().Name ?? "None"}, " +
                     $"PatrolSpeed = {template.patrolSpeed}, ChaseSpeed = {template.chaseSpeed}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No FollowerEntity component found for speed debugging");
        }
    }

    public void SetAndLogSpeed(float newSpeed, string source, float blendTime = 0.3f)
    {
        var followerEntity = GetComponent<FollowerEntity>();
        if (followerEntity != null)
        {
            // Stop any existing speed blend
            if (speedBlendCoroutine != null)
            {
                StopCoroutine(speedBlendCoroutine);
            }
            
            // Start new speed blend
            speedBlendCoroutine = StartCoroutine(BlendSpeed(followerEntity.maxSpeed, newSpeed, blendTime, source));
        }
    }
    
    private IEnumerator BlendSpeed(float fromSpeed, float toSpeed, float duration, string source)
    {
        var followerEntity = GetComponent<FollowerEntity>();
        float elapsed = 0f;
        
        Debug.Log($"[{gameObject.name}] SPEED BLEND START ({source}): {fromSpeed:F1} -> {toSpeed:F1} over {duration:F1}s");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            followerEntity.maxSpeed = Mathf.Lerp(fromSpeed, toSpeed, t);
            yield return null;
        }
        
        followerEntity.maxSpeed = toSpeed;
        Debug.Log($"[{gameObject.name}] SPEED BLEND COMPLETE ({source}): Final speed = {toSpeed:F1}");
        
        speedBlendCoroutine = null;
    }

    public void SetSpeed(float speed)
    {
        var followerEntity = GetComponent<FollowerEntity>();
        if (followerEntity != null)
        {
            followerEntity.maxSpeed = speed;
            Debug.Log($"[{name}] Enemy.SetSpeed(): Speed set to {speed}");
        }
        else
        {
            Debug.LogWarning($"[{name}] Enemy.SetSpeed(): No FollowerEntity component found to set speed");
        }
    }

    public void OnAttackFinished()
    {
        Debug.Log($"[{name}] Enemy.OnAttackFinished(): Current state: " + stateMachine.currentState.GetType().Name);
        // This method is called directly from animation events
        // Delegate to the current state if it's AttackState
        if (stateMachine.currentState == Attack && Attack is AttackState attackState)
        {
            attackState.OnAttackFinished();
        }
    }

    public void OnAttackLostMomentum()
    {
        Debug.Log($"[{name}] Enemy.OnAttackLostMomentum(): Current state: " + stateMachine.currentState.GetType().Name);
        // This method is called directly from animation events
        // Delegate to the current state if it's AttackState
        if (stateMachine.currentState == Attack && Attack is AttackState attackState)
        {
            attackState.OnAttackLostMomentum();
        }

        if (stateMachine.currentState == Aggro && Aggro is AggroState aggroState)
        {
            aggroState.OnAttackLostMomentum();
        }
    }

    // Debug buttons for testing in play mode
    [TabGroup("Debug")]
    [Button("Toggle Debug Mode"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void ToggleDebugMode()
    {
        DebugModeEnabled = !DebugModeEnabled;
        string status = DebugModeEnabled ? "ENABLED" : "DISABLED";
        Debug.Log($"[{name}] Debug Mode {status} - Automatic state transitions are now {(DebugModeEnabled ? "blocked" : "active")}");
        
        if (DebugModeEnabled)
        {
            // Stop the enemy from moving when debug mode is enabled
            SetSpeed(0f);
        }
    }
    
    [TabGroup("Debug")]
    [Button("Stop Movement"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void StopMovement()
    {
        SetSpeed(0f);
        Debug.Log($"[{name}] Movement stopped (speed set to 0)");
    }
    
    [TabGroup("Debug")]
    [Button("Resume Movement"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void ResumeMovement()
    {
        // Set speed based on current state
        if (stateMachine.currentState == Patrol)
        {
            SetSpeed(template.patrolSpeed);
        }
        else if (stateMachine.currentState == Alert || stateMachine.currentState == Aggro)
        {
            SetSpeed(template.chaseSpeed);
        }
        else if (stateMachine.currentState == Idle)
        {
            SetSpeed(0f); // Idle should not move
        }
        else if (stateMachine.currentState == Attack)
        {
            SetSpeed(0f); // Attack typically doesn't move
        }
        else
        {
            // Default fallback
            SetSpeed(template.patrolSpeed);
        }
        Debug.Log($"[{name}] Movement resumed for state: {stateMachine.currentState?.GetType().Name}");
    }

    [TabGroup("Debug")]
    [Button("Log Current Speed"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugLogCurrentSpeed()
    {
        LogCurrentSpeed("Manual Debug");
    }

    // State transition debug buttons - Row 1
    [TabGroup("Debug")]
    [HorizontalGroup("StateButtons1")]
    [Button("→ Idle"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetIdleState()
    {
        if (Idle != null)
        {
            stateMachine.SetState(Idle);
            Debug.Log($"[{name}] Manually set to Idle state" + (DebugModeEnabled ? "" : " (may auto-transition if player is close)"));
        }
    }

    [TabGroup("Debug")]
    [HorizontalGroup("StateButtons1")]
    [Button("→ Alert"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetAlertState()
    {
        if (Alert != null)
        {
            stateMachine.SetState(Alert);
            Debug.Log($"[{name}] Manually set to Alert state" + (DebugModeEnabled ? "" : " (may auto-transition if player distance changes)"));
        }
    }

    [TabGroup("Debug")]
    [HorizontalGroup("StateButtons1")]
    [Button("→ Patrol"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetPatrolState()
    {
        if (Patrol != null)
        {
            stateMachine.SetState(Patrol);
            Debug.Log($"[{name}] Manually set to Patrol state" + (DebugModeEnabled ? "" : " (may auto-transition if player is close)"));
        }
    }
    
    // State transition debug buttons - Row 2
    [TabGroup("Debug")]
    [HorizontalGroup("StateButtons2")]
    [Button("→ Aggro"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetAggroState()
    {
        if (Aggro != null)
        {
            stateMachine.SetState(Aggro);
            Debug.Log($"[{name}] Manually set to Aggro state" + (DebugModeEnabled ? "" : " (may auto-transition if player distance changes)"));
        }
    }

    [TabGroup("Debug")]
    [HorizontalGroup("StateButtons2")]
    [Button("→ Chase"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetChaseState()
    {
        if (Chase != null)
        {
            stateMachine.SetState(Chase);
            Debug.Log($"[{name}] Manually set to Chase state" + (DebugModeEnabled ? "" : " (may auto-transition if player distance changes)"));
        }
    }
    
    [TabGroup("Debug")]
    [HorizontalGroup("StateButtons2")]
    [Button("→ Attack"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetAttackState()
    {
        if (Attack != null)
        {
            stateMachine.SetState(Attack);
            Debug.Log($"[{name}] Manually set to Attack state" + (DebugModeEnabled ? "" : " (may auto-transition if player distance changes)"));
        }
    }
    
    [TabGroup("Debug")]
    [HorizontalGroup("StateButtons2")]
    [Button("→ Death"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetDeathState()
    {
        if (Death != null)
        {
            stateMachine.SetState(Death);
            Debug.Log($"[{name}] Manually set to Death state");
        }
    }

    [TabGroup("Debug")]
    [Button("Force Take Damage"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugTakeDamage([MinValue(1)] int damage = 10)
    {
        if (HealthManager != null)
        {
            HealthManager.TakeDamage(damage);
            Debug.Log($"[{name}] Debug: Took {damage} damage. Health: {HealthManager.currentHealth}");
        }
    }

    [TabGroup("Debug")]
    [Button("Revive Zombie"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugReviveZombie()
    {
        if (HealthManager != null)
        {
            // Restore health
            HealthManager.Initialize(template.maxHealth);
            Debug.Log($"[{name}] Debug: Restored health to {HealthManager.currentHealth}");
        }

        // Revive PuppetMaster if it's dead
        if (PuppetMaster != null && PuppetMaster.state == PuppetMaster.State.Dead)
        {
            PuppetMaster.Resurrect();
            Debug.Log($"[{name}] Debug: Resurrected PuppetMaster");
        }

        // Transition to idle state
        if (Idle != null)
        {
            stateMachine.SetState(Idle);
            Debug.Log($"[{name}] Debug: Set to Idle state after revival");
        }
    }
}
