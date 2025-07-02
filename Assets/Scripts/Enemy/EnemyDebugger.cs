using UnityEngine;
using EnemyStates;
using System.Collections;
using Pathfinding;
using RootMotion.Dynamics;
using Sirenix.OdinInspector;

/// <summary>
/// Handles all debug functionality for the Enemy class
/// Provides inspector buttons, logging, and debug state management
/// </summary>
public class EnemyDebugger : MonoBehaviour
{
    [Header("Debug References")]
    [Required]
    [SerializeField] private Enemy enemy;
    
    [Header("Debug Settings")]
    [ShowInInspector]
    [InfoBox("When enabled, prevents automatic state transitions based on player distance. Allows manual state control.", InfoMessageType.Info)]
    public bool DebugModeEnabled = false;
    
    [Header("Debug Information")]
    [ShowInInspector, ReadOnly]
    [ShowIf("@enemy != null && enemy.stateMachine != null")]
    public string CurrentState => enemy?.stateMachine?.currentState?.GetType().Name ?? "None";

    [ShowInInspector, ReadOnly]
    [ShowIf("@enemy != null && enemy.FollowerEntity != null")]
    public float CurrentSpeed => enemy?.FollowerEntity?.maxSpeed ?? 0f;

    [ShowInInspector, ReadOnly]
    [ShowIf("@enemy != null")]
    public bool HasAggroed => enemy?.HasAggroed ?? false;

    [ShowInInspector, ReadOnly]
    [ShowIf("@enemy != null")]
    public bool IsTurning => enemy?.IsTurning ?? false;

    private void Awake()
    {
        // Auto-assign enemy if not set
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogError($"[{name}] EnemyDebugger: No Enemy component found on this GameObject or assigned in inspector!");
            }
        }
    }

    private void Start()
    {
        // Sync debug mode with enemy
        if (enemy != null)
        {
            enemy.DebugModeEnabled = DebugModeEnabled;
        }
    }

    private void OnValidate()
    {
        // Sync debug mode when changed in inspector
        if (enemy != null && Application.isPlaying)
        {
            enemy.DebugModeEnabled = DebugModeEnabled;
        }
    }

    // ===============================================
    // SPEED DEBUG METHODS
    // ===============================================
    
    /// <summary>
    /// Logs current speed information with context
    /// </summary>
    public void LogCurrentSpeed(string context = "")
    {
        if (enemy?.FollowerEntity != null)
        {
            Debug.Log($"[{enemy.name}] SPEED DEBUG{(string.IsNullOrEmpty(context) ? "" : $" ({context})")}: " +
                     $"FollowerEntity.maxSpeed = {enemy.FollowerEntity.maxSpeed}, " +
                     $"Current State = {enemy.stateMachine?.currentState?.GetType().Name ?? "None"}, " +
                     $"PatrolSpeed = {enemy.template?.patrolSpeed ?? 0f}, " +
                     $"ChaseSpeed = {enemy.template?.chaseSpeed ?? 0f}");
        }
        else
        {
            Debug.LogWarning($"[{enemy?.name ?? name}] No FollowerEntity component found for speed debugging");
        }
    }

    /// <summary>
    /// Sets speed with smooth blending and logging
    /// </summary>
    public void SetAndLogSpeed(float newSpeed, string source, float blendTime = 0.3f)
    {
        if (enemy?.FollowerEntity != null)
        {
            enemy.SetAndLogSpeed(newSpeed, source, blendTime);
        }
    }

    // ===============================================
    // MOVEMENT DEBUG METHODS
    // ===============================================

    /// <summary>
    /// Stops enemy movement immediately
    /// </summary>
    public void StopMovement()
    {
        if (enemy != null)
        {
            enemy.SetSpeed(0f);
            Debug.Log($"[{enemy.name}] Debug: Movement stopped (speed set to 0)");
        }
    }

    /// <summary>
    /// Resumes movement based on current state
    /// </summary>
    public void ResumeMovement()
    {
        if (enemy?.template == null) return;

        // Set speed based on current state
        if (enemy.stateMachine.currentState == enemy.Patrol)
        {
            enemy.SetSpeed(enemy.template.patrolSpeed);
        }
        else if (enemy.stateMachine.currentState == enemy.Alert || enemy.stateMachine.currentState == enemy.Aggro)
        {
            enemy.SetSpeed(enemy.template.chaseSpeed);
        }
        else if (enemy.stateMachine.currentState == enemy.Idle)
        {
            enemy.SetSpeed(0f); // Idle should not move
        }
        else if (enemy.stateMachine.currentState == enemy.Attack)
        {
            enemy.SetSpeed(0f); // Attack typically doesn't move
        }
        else
        {
            // Default fallback
            enemy.SetSpeed(enemy.template.patrolSpeed);
        }
        Debug.Log($"[{enemy.name}] Debug: Movement resumed for state: {enemy.stateMachine.currentState?.GetType().Name}");
    }

    // ===============================================
    // STATE TRANSITION DEBUG METHODS
    // ===============================================

    /// <summary>
    /// Transitions to Idle state
    /// </summary>
    public void SetIdleState()
    {
        if (enemy?.Idle != null)
        {
            enemy.stateMachine.SetState(enemy.Idle);
            Debug.Log($"[{enemy.name}] Debug: Manually set to Idle state" + (DebugModeEnabled ? "" : " (may auto-transition if player is close)"));
        }
    }

    /// <summary>
    /// Transitions to Alert state
    /// </summary>
    public void SetAlertState()
    {
        if (enemy?.Alert != null)
        {
            enemy.stateMachine.SetState(enemy.Alert);
            Debug.Log($"[{enemy.name}] Debug: Manually set to Alert state" + (DebugModeEnabled ? "" : " (may auto-transition if player distance changes)"));
        }
    }

    /// <summary>
    /// Transitions to Patrol state
    /// </summary>
    public void SetPatrolState()
    {
        if (enemy?.Patrol != null)
        {
            enemy.stateMachine.SetState(enemy.Patrol);
            Debug.Log($"[{enemy.name}] Debug: Manually set to Patrol state" + (DebugModeEnabled ? "" : " (may auto-transition if player is close)"));
        }
    }

    /// <summary>
    /// Transitions to Aggro state
    /// </summary>
    public void SetAggroState()
    {
        if (enemy?.Aggro != null)
        {
            enemy.stateMachine.SetState(enemy.Aggro);
            Debug.Log($"[{enemy.name}] Debug: Manually set to Aggro state" + (DebugModeEnabled ? "" : " (may auto-transition if player distance changes)"));
        }
    }

    /// <summary>
    /// Transitions to Chase state
    /// </summary>
    public void SetChaseState()
    {
        if (enemy?.Chase != null)
        {
            enemy.stateMachine.SetState(enemy.Chase);
            Debug.Log($"[{enemy.name}] Debug: Manually set to Chase state" + (DebugModeEnabled ? "" : " (may auto-transition if player distance changes)"));
        }
    }

    /// <summary>
    /// Transitions to Attack state
    /// </summary>
    public void SetAttackState()
    {
        if (enemy?.Attack != null)
        {
            enemy.stateMachine.SetState(enemy.Attack);
            Debug.Log($"[{enemy.name}] Debug: Manually set to Attack state" + (DebugModeEnabled ? "" : " (may auto-transition if player distance changes)"));
        }
    }

    /// <summary>
    /// Transitions to Death state
    /// </summary>
    public void SetDeathState()
    {
        if (enemy?.Death != null)
        {
            enemy.stateMachine.SetState(enemy.Death);
            Debug.Log($"[{enemy.name}] Debug: Manually set to Death state");
        }
    }

    // ===============================================
    // HEALTH DEBUG METHODS
    // ===============================================

    /// <summary>
    /// Forces the enemy to take damage
    /// </summary>
    public void ForceTakeDamage(int damage = 10)
    {
        if (enemy?.HealthManager != null)
        {
            enemy.HealthManager.TakeDamage(damage);
            Debug.Log($"[{enemy.name}] Debug: Took {damage} damage. Health: {enemy.HealthManager.currentHealth}");
        }
    }

    /// <summary>
    /// Revives the zombie to full health
    /// </summary>
    public void ReviveZombie()
    {
        if (enemy == null) return;

        // Restore health
        if (enemy.HealthManager != null && enemy.template != null)
        {
            enemy.HealthManager.Initialize(enemy.template.maxHealth);
            Debug.Log($"[{enemy.name}] Debug: Restored health to {enemy.HealthManager.currentHealth}");
        }

        // Revive PuppetMaster if it's dead
        if (enemy.PuppetMaster != null && enemy.PuppetMaster.state == PuppetMaster.State.Dead)
        {
            enemy.PuppetMaster.Resurrect();
            Debug.Log($"[{enemy.name}] Debug: Resurrected PuppetMaster");
        }

        // Transition to idle state
        if (enemy.Idle != null)
        {
            enemy.stateMachine.SetState(enemy.Idle);
            Debug.Log($"[{enemy.name}] Debug: Set to Idle state after revival");
        }
    }

    // ===============================================
    // RESET DEBUG METHODS
    // ===============================================

    /// <summary>
    /// Resets enemy to a clean idle state
    /// </summary>
    public void ResetToCleanIdle()
    {
        if (enemy == null) return;

        Debug.Log($"[{enemy.name}] === RESETTING TO CLEAN IDLE ===");
        
        // 1. Stop all movement immediately
        enemy.SetSpeed(0f);
        Debug.Log($"[{enemy.name}] Reset: Speed set to 0");
        
        // 2. Clear all turning states
        enemy.IsTurning = false;
        enemy.AnimationManager?.SetIsTurning(false);
        Debug.Log($"[{enemy.name}] Reset: IsTurning cleared");
        
        // 3. Stop any active turn coroutines in current state
        if (enemy.stateMachine.currentState == enemy.Alert && enemy.Alert is AlertState alertState)
        {
            // Force finish any turn animation in AlertState
            alertState.OnTurnFinished();
            Debug.Log($"[{enemy.name}] Reset: AlertState turn finished");
        }
        else if (enemy.stateMachine.currentState == enemy.Aggro && enemy.Aggro is AggroState aggroState)
        {
            // Force finish any turn animation in AggroState  
            aggroState.OnTurnFinished();
            Debug.Log($"[{enemy.name}] Reset: AggroState turn finished");
        }
        
        // 4. Reset rotation to forward-facing
        enemy.transform.rotation = Quaternion.identity;
        Debug.Log($"[{enemy.name}] Reset: Rotation reset to identity");
        
        // 5. Clear any animation triggers that might be stuck
        enemy.AnimationManager?.ResetTrigger("TurnRight180");
        enemy.AnimationManager?.ResetTrigger("TurnLeft180");
        enemy.AnimationManager?.ResetTrigger("Aggro180");
        Debug.Log($"[{enemy.name}] Reset: Animation triggers cleared");
        
        // 6. Reset alert state in animation manager
        enemy.AnimationManager?.SetAlertState(false);
        Debug.Log($"[{enemy.name}] Reset: Alert state cleared");
        
        // 7. Clear HasAggroAnimFinished bool
        enemy.AnimationManager?.SetHasAgroAnimationFinished(false);
        Debug.Log($"[{enemy.name}] Reset: HasAggroAnimFinished cleared");
        
        // 8. Force transition to Idle state
        if (enemy.Idle != null)
        {
            enemy.stateMachine.SetState(enemy.Idle);
            Debug.Log($"[{enemy.name}] Reset: Transitioned to Idle state");
        }
        
        // 9. Enable debug mode to prevent automatic transitions
        DebugModeEnabled = true;
        enemy.DebugModeEnabled = true;
        Debug.Log($"[{enemy.name}] Reset: Debug mode enabled to prevent auto-transitions");
        
        Debug.Log($"[{enemy.name}] === RESET TO CLEAN IDLE COMPLETE ===");
    }

    /// <summary>
    /// Toggles debug mode on/off
    /// </summary>
    public void ToggleDebugMode()
    {
        DebugModeEnabled = !DebugModeEnabled;
        if (enemy != null)
        {
            enemy.DebugModeEnabled = DebugModeEnabled;
        }
        
        string status = DebugModeEnabled ? "ENABLED" : "DISABLED";
        Debug.Log($"[{enemy?.name ?? name}] Debug Mode {status} - Automatic state transitions are now {(DebugModeEnabled ? "blocked" : "active")}");
        
        if (DebugModeEnabled && enemy != null)
        {
            // Stop the enemy from moving when debug mode is enabled
            enemy.SetSpeed(0f);
        }
    }

    // ===============================================
    // INSPECTOR DEBUG BUTTONS
    // ===============================================

    [Button("Toggle Debug Mode"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugToggleDebugMode() => ToggleDebugMode();
    
    [Button("Stop Movement"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugStopMovement() => StopMovement();
    
    [Button("Resume Movement"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugResumeMovement() => ResumeMovement();

    [Button("Log Current Speed"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugLogCurrentSpeed() => LogCurrentSpeed("Manual Debug");

    // State transition debug buttons - Row 1
    [HorizontalGroup("StateButtons1")]
    [Button("→ Idle"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetIdleState() => SetIdleState();

    [HorizontalGroup("StateButtons1")]
    [Button("→ Alert"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetAlertState() => SetAlertState();

    [HorizontalGroup("StateButtons1")]
    [Button("→ Patrol"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetPatrolState() => SetPatrolState();
    
    // State transition debug buttons - Row 2
    [HorizontalGroup("StateButtons2")]
    [Button("→ Aggro"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetAggroState() => SetAggroState();

    [HorizontalGroup("StateButtons2")]
    [Button("→ Chase"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetChaseState() => SetChaseState();
    
    [HorizontalGroup("StateButtons2")]
    [Button("→ Attack"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetAttackState() => SetAttackState();
    
    [HorizontalGroup("StateButtons2")]
    [Button("→ Death"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugSetDeathState() => SetDeathState();

    [Button("Force Take Damage"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugTakeDamage([MinValue(1)] int damage = 10) => ForceTakeDamage(damage);

    [Button("Revive Zombie"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugReviveZombie() => ReviveZombie();

    [Button("🔄 Reset to Clean Idle"), EnableIf("@UnityEngine.Application.isPlaying")]
    [InfoBox("Resets rotation, clears all turning states, stops all coroutines, and transitions to clean idle state.", InfoMessageType.Info)]
    private void DebugResetToCleanIdle() => ResetToCleanIdle();
}
