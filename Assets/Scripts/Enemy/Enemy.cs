using UnityEngine;
using EnemyStates;
using System.Collections.Generic;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    public AnimationManager AnimationManager { get; private set; }

    public HealthManager HealthManager { get; private set; }

    public AIDestinationSetter AIDestinationSetter { get; private set; }

    [SerializeField]
    private Transform PlayerTransform;

    [SerializeField]
    private List<Transform> patrolPoints;

    private float alertRange = 15f;

    private float patrolSpeed = 1f;

    private float chaseSpeed = 3f;

    private float aggroRange = 10f;

    public StateMachine<EnemyState> stateMachine { get; private set; }

    public EnemyState Idle { get; private set; }

    public EnemyState Alert { get; private set; }
    public EnemyState Patrol { get; private set; }

    public EnemyState Aggro { get; private set; }

    public Transform GetPlayerTransform() => PlayerTransform;
    public List<Transform> GetPatrolPoints() => patrolPoints;
    public float GetAlertRange() => alertRange;
    public float GetPatrolSpeed() => patrolSpeed;
    public float GetChaseSpeed() => chaseSpeed;

    public float GetAggroRange() => aggroRange;

    public EnemyTemplate template;

    public int health = 100;

    private void Awake()
    {
        SetupAnimator();
        HealthManager = GetComponent<HealthManager>();
        AIDestinationSetter = GetComponent<AIDestinationSetter>();
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (template == null)
        {
            Debug.LogError("EnemyTemplate is not assigned. Please assign a template in the inspector.");
            return;
        }

        health = template.maxHealth;
        alertRange = template.alertRange;
        patrolSpeed = template.patrolSpeed;
        chaseSpeed = template.chaseSpeed;
        aggroRange = template.aggroRange;

        Debug.Log($"[{gameObject.name}] Enemy template loaded - patrolSpeed: {patrolSpeed}, chaseSpeed: {chaseSpeed}");

        Debug.Log($"[{gameObject.name}] Initializing enemy states...");

        Idle = new IdleState(this, stateMachine, AnimationManager, "Idle");
        Debug.Log($"[{gameObject.name}] ✓ Idle state initialized");
        
        Alert = new AlertState(this, stateMachine, AnimationManager, "Alert");
        Debug.Log($"[{gameObject.name}] ✓ Alert state initialized");
        
        Patrol = new PatrolState(this, stateMachine, AnimationManager, "Locomotion (OH)");
        Debug.Log($"[{gameObject.name}] ✓ Patrol state initialized");
        
        Aggro = new AggroState(this, stateMachine, AnimationManager, "Aggro", PlayerTransform);
        Debug.Log($"[{gameObject.name}] ✓ Aggro state initialized");

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

        if (limb.LimbType == LimbType.Head || limb.LimbType == LimbType.Torso)
        {
            HealthManager.TakeDamage(damage);
            Debug.Log($"{name} took {damage} damage. Remaining health: {HealthManager.currentHealth}");
        }

        if (HealthManager.currentHealth <= 0)
        {
            // Enter death state
            Debug.Log($"{name} has died.");
            //RagdollUtility.EnableRagdoll();
        }
    }

    public bool IsPlayerInAlertRange()
    {
        if (PlayerTransform == null)
        {
            Debug.LogWarning("PlayerTransform is not assigned.");
            return false;
        }

        float sqrDistanceToPlayer = (transform.position - PlayerTransform.position).sqrMagnitude;
        return sqrDistanceToPlayer <= alertRange * alertRange;
    }

    public bool IsPlayerInAggroRange()
    {
        if (PlayerTransform == null)
        {
            Debug.LogWarning("PlayerTransform is not assigned.");
            return false;
        }

        float sqrDistanceToPlayer = (transform.position - PlayerTransform.position).sqrMagnitude;
        return sqrDistanceToPlayer <= aggroRange * aggroRange;
    }

    public void OnTurnFinished()
    {
        Debug.Log("Enemy: Turn animation finished.");
        // This method is called directly from animation events
        // Delegate to the current state if it's AlertState
        if (stateMachine.currentState == Alert && Alert is AlertState alertState)
        {
            alertState.OnTurnFinished();
        }
    }

    public void OnYellFinished()
    {
        Debug.Log("Enemy: Yell animation finished.");

        if (stateMachine.currentState == Aggro && Aggro is AggroState aggroState)
        {
            aggroState.OnYellFinished();
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
                     $"PatrolSpeed = {patrolSpeed}, ChaseSpeed = {chaseSpeed}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No FollowerEntity component found for speed debugging");
        }
    }

    public void SetAndLogSpeed(float newSpeed, string source)
    {
        var followerEntity = GetComponent<FollowerEntity>();
        if (followerEntity != null)
        {
            float oldSpeed = followerEntity.maxSpeed;
            followerEntity.maxSpeed = newSpeed;
            Debug.Log($"[{gameObject.name}] SPEED CHANGE ({source}): {oldSpeed} -> {newSpeed}");
            
            // Add stack trace for debugging (can be commented out in production)
            if (Application.isEditor)
            {
                Debug.Log($"[{gameObject.name}] Speed change stack trace:\n{System.Environment.StackTrace}");
            }
        }
    }
}
