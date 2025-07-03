using UnityEngine;
using PlayerStates;
using Sirenix.OdinInspector;

/// <summary>
/// Handles all debug functionality for the Player class
/// Provides inspector buttons, logging, and debug state management
/// </summary>
public class PlayerDebugger : MonoBehaviour
{
    [Header("Debug References")]
    [Required]
    [SerializeField] private Player player;
    
    [Header("Debug Information")]
    [ShowInInspector, ReadOnly]
    [ShowIf("@player != null && player.stateMachine != null")]
    public string CurrentState => GetCurrentStateName();

    [ShowInInspector, ReadOnly]
    [ShowIf("@player != null && player.PlayerCharacterController != null")]
    public bool IsGrounded => player?.PlayerCharacterController?.CharacterController.isGrounded ?? false;

    [ShowInInspector, ReadOnly]
    [ShowIf("@player != null && player.PlayerCharacterController != null")]
    public Vector3 CurrentVelocity => player?.PlayerCharacterController?.CharacterController.velocity ?? Vector3.zero;

    [ShowInInspector, ReadOnly]
    [ShowIf("@player != null && player.playerTemplate != null")]
    public float MaxHealth => player?.playerTemplate?.maxHealth ?? 0;

    [ShowInInspector, ReadOnly]
    [ShowIf("@player != null && player.playerTemplate != null")]
    public float SprintSpeed => player?.playerTemplate?.sprintSpeed ?? 0;

    [ShowInInspector, ReadOnly]
    [ShowIf("@player != null && player.playerTemplate != null")]
    public float StrafeSpeed => player?.playerTemplate?.strafeSpeed ?? 0;

    private void Awake()
    {
        // Auto-assign player if not set
        if (player == null)
        {
            player = GetComponent<Player>();
            if (player == null)
            {
                Debug.LogError($"[{name}] PlayerDebugger: No Player component found on this GameObject or assigned in inspector!");
            }
        }
    }

    // ===============================================
    // UTILITY METHODS
    // ===============================================
    
    private string GetCurrentStateName()
    {
        if (player?.stateMachine?.currentState == null) return "None";
        
        // Check which state is currently active
        if (player.stateMachine.currentState == player.idle) return "Idle";
        if (player.stateMachine.currentState == player.sprint) return "Sprint";
        if (player.stateMachine.currentState == player.strafe) return "Strafe";
        if (player.stateMachine.currentState == player.aim) return "Aim";
        if (player.stateMachine.currentState == player.shoot) return "Shoot";
        if (player.stateMachine.currentState == player.jump) return "Jump";
        
        return player.stateMachine.currentState.GetType().Name;
    }

    /// <summary>
    /// Logs comprehensive component status
    /// </summary>
    public void LogComponentStatus()
    {
        Debug.Log($"=== Player Debug Info for {player?.gameObject.name ?? name} ===");
        Debug.Log($"State Machine Initialized: {(player?.stateMachine != null ? "✓" : "✗")}");
        Debug.Log($"Current State: {GetCurrentStateName()}");
        Debug.Log($"Player Template: {(player?.playerTemplate != null ? "✓" : "✗")}");
        
        if (player?.playerTemplate != null)
        {
            Debug.Log($"  Max Health: {player.playerTemplate.maxHealth}");
            Debug.Log($"  Sprint Speed: {player.playerTemplate.sprintSpeed}");
            Debug.Log($"  Strafe Speed: {player.playerTemplate.strafeSpeed}");
        }
    }

    /// <summary>
    /// Validates all required components
    /// </summary>
    public void ValidateComponents()
    {
        Debug.Log($"=== Component Validation for {player?.gameObject.name ?? name} ===");
        Debug.Log($"PlayerInput: {(player?.PlayerInput != null ? "✓" : "✗")}");
        Debug.Log($"PlayerCharacterController: {(player?.PlayerCharacterController != null ? "✓" : "✗")}");
        Debug.Log($"PlayerCameraController: {(player?.PlayerCameraController != null ? "✓" : "✗")}");
        Debug.Log($"AnimationManager: {(player?.AnimationManager != null ? "✓" : "✗")}");
        Debug.Log($"WeaponManager: {(player?.WeaponManager != null ? "✓" : "✗")}");
        Debug.Log($"PlayerTemplate: {(player?.playerTemplate != null ? "✓" : "✗")}");
        Debug.Log($"Recoil: {(player?.Recoil != null ? "✓" : "✗")}");
        Debug.Log($"BulletHitscan: {(player?.BulletHitscan != null ? "✓" : "✗")}");
        Debug.Log($"BulletDecalManager: {(player?.BulletDecalManager != null ? "✓" : "✗")}");
    }

    /// <summary>
    /// Logs current movement and physics state
    /// </summary>
    public void LogMovementState()
    {
        if (player?.PlayerCharacterController == null)
        {
            Debug.LogWarning($"[{player?.name ?? name}] No PlayerCharacterController found for movement debugging");
            return;
        }

        Debug.Log($"=== Movement Debug for {player.name} ===");
        Debug.Log($"Is Grounded: {player.PlayerCharacterController.CharacterController.isGrounded}");
        Debug.Log($"Velocity: {player.PlayerCharacterController.CharacterController.velocity}");
        Debug.Log($"Speed: {player.PlayerCharacterController.CharacterController.velocity.magnitude:F2}");
        Debug.Log($"Current State: {GetCurrentStateName()}");
    }

    /// <summary>
    /// Logs current weapon and combat state
    /// </summary>
    public void LogWeaponState()
    {
        if (player?.WeaponManager == null)
        {
            Debug.LogWarning($"[{player?.name ?? name}] No WeaponManager found for weapon debugging");
            return;
        }

        Debug.Log($"=== Weapon Debug for {player.name} ===");
        Debug.Log($"WeaponManager Active: {player.WeaponManager != null}");
        Debug.Log($"Current State: {GetCurrentStateName()}");
        Debug.Log($"Crosshair Controller: {(player.CrosshairController != null ? "✓" : "✗")}");
    }

    // ===============================================
    // INSPECTOR DEBUG BUTTONS
    // ===============================================

    [Button("Log Component Status"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugLogComponentStatus() => LogComponentStatus();

    [Button("Validate All Components"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugValidateComponents() => ValidateComponents();

    [Button("Log Movement State"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugLogMovementState() => LogMovementState();

    [Button("Log Weapon State"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugLogWeaponState() => LogWeaponState();

    // State transition debug buttons - Row 1
    [HorizontalGroup("StateButtons1")]
    [Button("→ Idle"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void ForceIdleState() => player?.stateMachine?.SetState(player.idle);

    [HorizontalGroup("StateButtons1")]
    [Button("→ Sprint"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void ForceSprintState() => player?.stateMachine?.SetState(player.sprint);

    [HorizontalGroup("StateButtons1")]
    [Button("→ Strafe"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void ForceStrafeState() => player?.stateMachine?.SetState(player.strafe);

    // State transition debug buttons - Row 2
    [HorizontalGroup("StateButtons2")]
    [Button("→ Aim"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void ForceAimState() => player?.stateMachine?.SetState(player.aim);

    [HorizontalGroup("StateButtons2")]
    [Button("→ Shoot"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void ForceShootState() => player?.stateMachine?.SetState(player.shoot);

    [HorizontalGroup("StateButtons2")]
    [Button("→ Jump"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void ForceJumpState() => player?.stateMachine?.SetState(player.jump);

    // Health debug buttons
    [Button("Force Take Damage"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugTakeDamage([MinValue(1)] int damage = 10) => ForceTakeDamage(damage);

    [Button("Restore Full Health"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugRestoreHealth() => RestoreFullHealth();

    // ===============================================
    // HEALTH DEBUG METHODS
    // ===============================================

    /// <summary>
    /// Forces the player to take damage for testing
    /// </summary>
    public void ForceTakeDamage(int damage = 10)
    {
        if (player?.HealthManager != null)
        {
            player.HealthManager.TakeDamage(damage);
            Debug.Log($"[{player.name}] PlayerDebugger.ForceTakeDamage(): Took {damage} damage. Health: {player.HealthManager.currentHealth}");
        }
        else
        {
            Debug.LogWarning($"[{player?.name ?? name}] PlayerDebugger.ForceTakeDamage(): No HealthManager found for damage testing");
        }
    }

    /// <summary>
    /// Restores player to full health
    /// </summary>
    public void RestoreFullHealth()
    {
        if (player?.HealthManager != null)
        {
            player.HealthManager.ResetHealth();
            Debug.Log($"[{player.name}] PlayerDebugger.RestoreFullHealth(): Health restored to {player.HealthManager.currentHealth}");
        }
        else
        {
            Debug.LogWarning($"[{player?.name ?? name}] PlayerDebugger.RestoreFullHealth(): No HealthManager found for health restoration");
        }
    }
}
