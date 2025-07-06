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
    [ShowIf("@player != null && (player.PlayerCharacterController != null || player.GetComponent<CharacterController>() != null)")]
    public bool IsGrounded {
        get {
            if (player?.PlayerCharacterController?.CharacterController != null)
                return player.PlayerCharacterController.CharacterController.isGrounded;
            var cc = player != null ? player.GetComponent<CharacterController>() : null;
            return cc != null && cc.isGrounded;
        }
    }

    [ShowInInspector, ReadOnly]
    [ShowIf("@player != null && (player.PlayerCharacterController != null || player.GetComponent<CharacterController>() != null)")]
    public Vector3 CurrentVelocity {
        get {
            if (player?.PlayerCharacterController?.CharacterController != null)
                return player.PlayerCharacterController.CharacterController.velocity;
            var cc = player != null ? player.GetComponent<CharacterController>() : null;
            return cc != null ? cc.velocity : Vector3.zero;
        }
    }

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

    private void Update()
    {
        if (ForceAimDebugMode && player?.stateMachine != null && player.aim != null)
        {
            if (player.stateMachine.currentState != player.aim)
            {
                player.stateMachine.SetState(player.aim);
                Debug.Log($"[{player.name}] PlayerDebugger: Re-forced Aim state (Debug Mode ON).");
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
        Debug.Log($"State Machine Initialized: {(player?.stateMachine != null ? "✓" : "✗")}.");
        Debug.Log($"Current State: {GetCurrentStateName()}");
        Debug.Log($"Player Template: {(player?.playerTemplate != null ? "✓" : "✗")}.");
        
        if (player?.playerTemplate != null)
        {
            Debug.Log($"Max Health: {player.playerTemplate.maxHealth}.");
            Debug.Log($"Sprint Speed: {player.playerTemplate.sprintSpeed}.");
            Debug.Log($"Strafe Speed: {player.playerTemplate.strafeSpeed}.");
        }
    }

    /// <summary>
    /// Validates all required components
    /// </summary>
    public void ValidateComponents()
    {
        Debug.Log($"=== Component Validation for {player?.gameObject.name ?? name} ===");
        Debug.Log($"PlayerInput: {(player?.PlayerInput != null ? "✓" : "✗")}");
        Debug.Log($"PlayerCharacterController: {(player?.PlayerCharacterController != null ? "✓" : "✗")}.");
        Debug.Log($"PlayerCameraController: {(player?.PlayerCameraController != null ? "✓" : "✗")}.");
        Debug.Log($"AnimationManager: {(player?.AnimationManager != null ? "✓" : "✗")}.");
        Debug.Log($"WeaponManager: {(player?.WeaponManager != null ? "✓" : "✗")}.");
        Debug.Log($"PlayerTemplate: {(player?.playerTemplate != null ? "✓" : "✗")}.");
        Debug.Log($"Recoil: {(player?.Recoil != null ? "✓" : "✗")}.");
        Debug.Log($"BulletHitscan: {(player?.BulletHitscan != null ? "✓" : "✗")}.");
        Debug.Log($"BulletDecalManager: {(player?.BulletDecalManager != null ? "✓" : "✗")}.");
    }

    /// <summary>
    /// Logs current movement and physics state
    /// </summary>
    public void LogMovementState()
    {
        if (player?.PlayerCharacterController == null)
        {
            Debug.LogWarning($"[{player?.name ?? name}] No PlayerCharacterController found for movement debugging.");
            return;
        }

        Debug.Log($"=== Movement Debug for {player.name} ===");
        Debug.Log($"Is Grounded: {player.PlayerCharacterController.CharacterController.isGrounded}.");
        Debug.Log($"Velocity: {player.PlayerCharacterController.CharacterController.velocity}.");
        Debug.Log($"Speed: {player.PlayerCharacterController.CharacterController.velocity.magnitude:F2}.");
        Debug.Log($"Current State: {GetCurrentStateName()}.");
    }

    /// <summary>
    /// Logs current weapon and combat state
    /// </summary>
    public void LogWeaponState()
    {
        if (player?.WeaponManager == null)
        {
            Debug.LogWarning($"[{player?.name ?? name}] No WeaponManager found for weapon debugging.");
            return;
        }

        Debug.Log($"=== Weapon Debug for {player.name} ===");
        Debug.Log($"WeaponManager Active: {player.WeaponManager != null}");
        Debug.Log($"Current State: {GetCurrentStateName()}");
        Debug.Log($"Crosshair Controller: {(player.CrosshairController != null ? "✓" : "✗")}.");
    }

    /// <summary>
    /// Logs current input state and input system status
    /// </summary>
    public void LogInputState()
    {
        if (player?.PlayerInput == null)
        {
            Debug.LogWarning($"[{player?.name ?? name}] No PlayerInput found for input debugging.");
            return;
        }

        Debug.Log($"=== Input Debug for {player.name} ===");
        Debug.Log($"PlayerInput Active: {player.PlayerInput != null}");
        Debug.Log($"Current State: {GetCurrentStateName()}");
        Debug.Log($"Input System Responsive: {(player.PlayerInput.enabled ? "✓" : "✗")}.");
    }

    /// <summary>
    /// Resets player to a safe position (useful when player gets stuck)
    /// </summary>
    public void ResetPlayerPosition()
    {
        if (player?.transform == null)
        {
            Debug.LogWarning($"[{player?.name ?? name}] No player transform found for position reset.");
            return;
        }

        Vector3 resetPosition = new Vector3(0f, 2f, 0f); // Safe spawn position
        player.transform.position = resetPosition;
        
        // Stop any velocity if CharacterController exists
        if (player.PlayerCharacterController?.CharacterController != null)
        {
            // CharacterController velocity is read-only, but moving to a position resets it
            player.PlayerCharacterController.CharacterController.enabled = false;
            player.PlayerCharacterController.CharacterController.enabled = true;
        }

        Debug.Log($"[{player.name}] PlayerDebugger.ResetPlayerPosition(): Player position reset to {resetPosition}.");
    }

    /// <summary>
    /// Logs detailed physics state for debugging movement issues
    /// </summary>
    public void LogPhysicsState()
    {
        if (player?.PlayerCharacterController == null)
        {
            Debug.LogWarning($"[{player?.name ?? name}] No PlayerCharacterController found for physics debugging");
            return;
        }

        Debug.Log($"=== Physics Debug for {player.name} ===");
        Debug.Log($"Is Grounded: {player.PlayerCharacterController.CharacterController.isGrounded}.");
        Debug.Log($"Velocity: {player.PlayerCharacterController.CharacterController.velocity}.");
        Debug.Log($"Speed: {player.PlayerCharacterController.CharacterController.velocity.magnitude:F2} units/sec.");
        Debug.Log($"Position: {player.transform.position}.");
        Debug.Log($"Template Gravity: {player.playerTemplate?.gravity ?? 0f} m/s².");
        Debug.Log($"Current State: {GetCurrentStateName()}.");
    }

    /// <summary>
    /// Logs all template data for debugging configuration issues
    /// </summary>
    public void LogTemplateData()
    {
        if (player?.playerTemplate == null)
        {
            Debug.LogWarning($"[{player?.name ?? name}] No PlayerTemplate found for template debugging.");
            return;
        }

        Debug.Log($"=== Template Debug for {player.name} ===");
        Debug.Log($"Template Name: {player.playerTemplate.name}");
        Debug.Log($"Max Health: {player.playerTemplate.maxHealth} HP");
        Debug.Log($"Strafe Speed: {player.playerTemplate.strafeSpeed} units/sec");
        Debug.Log($"Sprint Speed: {player.playerTemplate.sprintSpeed} units/sec");
        Debug.Log($"Gravity: {player.playerTemplate.gravity} m/s²");
        
        // Validate template consistency
        if (player.playerTemplate.sprintSpeed <= player.playerTemplate.strafeSpeed)
            Debug.LogWarning($"[{player.name}] ⚠️ Template issue: Sprint speed ({player.playerTemplate.sprintSpeed}) should be faster than strafe speed ({player.playerTemplate.strafeSpeed})!");
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

    // Player-specific debug buttons
    [HorizontalGroup("PlayerDebugButtons1")]
    [Button("Log Input State"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugLogInputState() => LogInputState();

    [HorizontalGroup("PlayerDebugButtons1")]
    [Button("Reset Position"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugResetPosition() => ResetPlayerPosition();

    [HorizontalGroup("PlayerDebugButtons2")]
    [Button("Test Physics"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugTestPhysics() => LogPhysicsState();

    [HorizontalGroup("PlayerDebugButtons2")]
    [Button("Log Template Data"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugLogTemplateData() => LogTemplateData();

    // Health debug buttons
    [Button("Force Take Damage"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugTakeDamage([MinValue(1)] int damage = 10) => ForceTakeDamage(damage);

    [Button("Restore Full Health"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugRestoreHealth() => RestoreFullHealth();

    // Debug flag to lock AimState
    public static bool ForceAimDebugMode = false;

    [Button("Force Aim State"), EnableIf("@UnityEngine.Application.isPlaying")]
    private void DebugForceAimState()
    {
        DebugExitAimDebugMode(); // Always exit first to reset state
        if (player?.stateMachine != null && player.aim != null)
        {
            ForceAimDebugMode = true;
            player.stateMachine.SetState(player.aim);
            Debug.Log($"[{player.name}] PlayerDebugger: Forced Aim state (Debug Mode ON).");
        }
        else
        {
            Debug.LogWarning($"[{player?.name ?? name}] PlayerDebugger: Cannot force Aim state (missing stateMachine or aim state).");
        }
    }

    [Button("Exit Aim Debug Mode"), EnableIf("@UnityEngine.Application.isPlaying && PlayerDebugger.ForceAimDebugMode")]
    private void DebugExitAimDebugMode()
    {
        ForceAimDebugMode = false;
        Debug.Log($"[{player?.name ?? name}] PlayerDebugger: Aim Debug Mode OFF.");
    }

    // Debug flag to disable IK for weapon positioning
    public static bool DebugDisableIK = false;

    [Button("Disable IK (Debug)", ButtonSizes.Large), EnableIf("@UnityEngine.Application.isPlaying && !PlayerDebugger.DebugDisableIK")]
    private void DebugDisableIKButton()
    {
        DebugDisableIK = true;
        Debug.Log($"[{player?.name ?? name}] PlayerDebugger: IK Disabled for weapon positioning.");
    }

    [Button("Enable IK (Debug)", ButtonSizes.Large), EnableIf("@UnityEngine.Application.isPlaying && PlayerDebugger.DebugDisableIK")]
    private void DebugEnableIKButton()
    {
        DebugDisableIK = false;
        Debug.Log($"[{player?.name ?? name}] PlayerDebugger: IK Enabled.");
    }

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
            Debug.Log($"[{player.name}] PlayerDebugger.ForceTakeDamage(): Took {damage} damage. Health: {player.HealthManager.currentHealth}.");
        }
        else
        {
            Debug.LogWarning($"[{player?.name ?? name}] PlayerDebugger.ForceTakeDamage(): No HealthManager found for damage testing.");
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
            Debug.Log($"[{player.name}] PlayerDebugger.RestoreFullHealth(): Health restored to {player.HealthManager.currentHealth}.");
        }
        else
        {
            Debug.LogWarning($"[{player?.name ?? name}] PlayerDebugger.RestoreFullHealth(): No HealthManager found for health restoration.");
        }
    }
}
