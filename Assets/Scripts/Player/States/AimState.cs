using UnityEngine;
using PlayerStates;
using RootMotion.FinalIK;

public class AimState : StrafeState
{
    protected PlayerWeaponManager weaponManager;

    public AimState(
        Player player,
        StateMachine<PlayerState> stateMachine,
        AnimationManager animationManager,
        string animationName,
        PlayerWeaponManager weaponManager
    ) : base(
        player,
        stateMachine,
        animationManager,
        animationName
    )
    {
        this.weaponManager = weaponManager;
    }

    public override void Enter()
    {
        Debug.Log($"[{player.name}] AimState.Enter(): Entering Aim state");
        base.Enter();

        // Enable AimIK via PlayerIKController
        player.PlayerIKController.EnableAimIK();

        animationManager.SetIsAiming(true);
        player.PlayerIKController.SetIKTargetWeight(1f);

        SetupCrosshair();

        player.PlayerCameraController.SetCameraSwayAmount(weaponManager.CurrentWeaponData.weaponSway);
        player.PlayerCameraController.EnableCameraSway();
        player.PlayerCameraController.SetCameraOffset();

        SetupWeapon();
    }

    private void SetupCrosshair()
    {
        float bulletSpreadHorizontal = weaponManager.CurrentWeaponData.bulletSpreadHorizontal;
        float bulletSpreadVertical = weaponManager.CurrentWeaponData.bulletSpreadVertical;

        player.CrosshairController.EnableCrosshair();
        Debug.Log($"[{player.name}] AimState.SetupCrosshair(): Expanding and contracting crosshair");
        player.CrosshairController.ExpandAndContractCrosshair(
            bulletSpreadHorizontal,
            bulletSpreadVertical,
            1f
        );
    }

    private void SetupWeapon()
    {
        GameObject weaponInstance = weaponManager.SpawnWeaponInWeaponHand();
        Weapon weaponScript = weaponInstance.GetComponent<Weapon>();

        if (weaponScript != null && weaponScript.muzzleTransform != null)
        {
            player.PlayerIKController.SetAimTransform(weaponScript.muzzleTransform);
        }
        else
        {
            Debug.LogWarning($"[{player.name}] AimState.SetupWeapon(): MuzzleTransform not set on weapon prefab!");
        }

        weaponManager.SetAimIKOffsets();
        weaponManager.SetRecoilIKSettings();
    }

    public override void Exit(PlayerState nextState)
    {
        Debug.Log($"[{player.name}] AimState.Exit(): Exiting to {nextState.GetType().Name}");
        base.Exit(nextState);

        if (nextState is IdleState || (nextState is StrafeState && nextState is not ShootState && nextState is not AimState) || nextState is SprintState)
        {
            animationManager.SetIsAiming(false);
            weaponManager.DespawnWeaponInWeaponHand(); // TODO: create WeaponIdleIKOffsets for when the weapon is not aimed

            player.PlayerIKController.SetIKTargetWeight(0f);
            player.PlayerCameraController.DisableCameraSway();
            player.PlayerCameraController.ResetCameraOffset();

            player.CrosshairController.DisableCrosshair();
            player.PlayerIKController.DisableAimIK();
        }
    }

    public override void LogicUpdate()
    {
        // Guard: Only update if this is the current state
        if (stateMachine.currentState != this)
        {
            return;
        }

        base.LogicUpdate();

        player.PlayerCameraController.ZoomIn();

        // Always update IK offsets, even in debug mode
        weaponManager.SetAimIKOffsets();

        // Prevent automatic transitions if debug mode is active
        if (PlayerDebugger.ForceAimDebugMode)
        {
            player.PlayerIKController.SetIKTargetWeight(1f); // Ensure IK weight is set every frame in debug mode
            if (player.PlayerInput.IsMoving)
            {
                player.CrosshairController.ExpandAndContractCrosshair(
                    1f,
                    weaponManager.CurrentWeaponData.bulletSpreadHorizontal,
                    weaponManager.CurrentWeaponData.bulletSpreadVertical,
                    0.1f
                );
            }
            return;
        }

        if (player.PlayerInput.IsMoving && !player.PlayerInput.IsAiming)
        {
            stateMachine.SetState(player.strafe);
            return;
        }

        if (player.PlayerInput.IsAiming && player.PlayerInput.IsAttacking && stateMachine.currentState != player.shoot)
        {
            stateMachine.SetState(player.shoot);
            return;
        }

        if (player.PlayerInput.IsMoving)
        {
            player.CrosshairController.ExpandAndContractCrosshair(
                1f,
                weaponManager.CurrentWeaponData.bulletSpreadHorizontal,
                weaponManager.CurrentWeaponData.bulletSpreadVertical,
                0.1f
            );
        }
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        player.PlayerCameraController.MoveAimTarget();

        Vector3 direction = player.PlayerInput.GetInputDirection();
        Vector3 aimTarget = player.PlayerCameraController.GetAimTarget();
        player.PlayerIKController.UpdateIKs(direction, aimTarget);
        
        // Prevent crosshair expansion if we're in ShootState
        if (stateMachine.currentState == player.shoot)
            return;

        if (player.PlayerCameraController.HasCameraAxisChanged())
        {
            float bulletSpreadHorizontal = weaponManager.CurrentWeaponData.bulletSpreadHorizontal;
            float bulletSpreadVertical = weaponManager.CurrentWeaponData.bulletSpreadVertical;
            player.CrosshairController.ExpandAndContractCrosshair(
                1f,
                bulletSpreadHorizontal,
                bulletSpreadVertical,
                0.1f
            );
        }
    }
}
