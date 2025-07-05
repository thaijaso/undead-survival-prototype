using UnityEngine;
using PlayerStates;

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

        animationManager.SetIsAiming(true);
        player.PlayerIKController.SetIKTargetWeight(1f);

        SetupCrosshair();

        player.PlayerCameraController.SetCameraSwayAmount(weaponManager.CurrentWeaponData.weaponSway);
        player.PlayerCameraController.EnableCameraSway();

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

        if (weaponScript != null && weaponScript.aimTransform != null)
        {
            player.PlayerIKController.SetAimTransform(weaponScript.aimTransform);
        }
        else
        {
            Debug.LogWarning($"[{player.name}] AimState.SetupWeapon(): AimTransform not set on weapon prefab!");
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

            player.CrosshairController.DisableCrosshair();
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

        animationManager.BlendLayerWeight(1, 1f);

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
