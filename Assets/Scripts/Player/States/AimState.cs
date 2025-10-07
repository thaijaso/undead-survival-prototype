using UndeadSurvivalGame.Gameplay;
using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{
    public class AimState : StrafeState
    {
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
            animationName,
            weaponManager
        )
        {

        }

        public override void Enter()
        {
            Debug.Log($"[{player.name}] AimState.Enter(): Entering Aim state");
            animationManager.SetIsAiming(true);
            //player.PlayerIKController.EnableIK();
            player.PlayerIKController.SetAimIkWeight(1f);
            player.PlayerIKController.SetFBBIKWeight(1f);
            player.PlayerIKController.SetHeadLookAtWeight(1f);
            player.AimPoseLayerWeightController.SetWeight(0f);
            SetupCrosshair();
            SetupCamera();
            SetupWeapon();

            player.AimPoseLayerWeightController.SetWeight(0f);
            player.AimPitchLayerWeightController.SetWeight(1f);
        }

        private void SetupCamera()
        {
            player.PlayerCameraController.SetCameraSwayAmount(weaponManager.CurrentWeaponConfig.weaponSway);
            player.PlayerCameraController.EnableCameraSway();
            player.PlayerCameraController.SetCameraOffset();
        }

        private void SetupCrosshair()
        {
            player.CrosshairController.EnableCrosshair();
            
            float bulletSpreadHorizontal = weaponManager.CurrentWeaponConfig.bulletSpreadHorizontal;
            float bulletSpreadVertical = weaponManager.CurrentWeaponConfig.bulletSpreadVertical;

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
            if (weaponManager == null)
            {
                Debug.LogWarning($"[{player.name}] AimState.SetupWeapon(): WeaponManager is null!");
                return;
            }

            SetupWeaponScriptIKAndGrip(weaponManager.CurrentWeaponScript);
            weaponManager.SetAimIKOffsets();
            weaponManager.SetRecoilIKSettings();
        }

        private void SetupWeaponScriptIKAndGrip(Weapon weaponScript)
        {
            if (weaponScript != null)
            {
                // Set muzzle transform for aiming
                if (weaponScript.muzzleTransform != null)
                {
                    player.PlayerIKController.SetAimTransform(weaponScript.muzzleTransform);
                }
                else
                {
                    Debug.LogWarning($"[{player.name}] AimState.SetupWeapon(): MuzzleTransform not set on weapon prefab!");
                }

                // Set left hand grip source
                if (weaponScript.leftHandGripSource != null)
                {
                    Debug.Log($"[{player.name}] AimState.SetupWeapon(): Setting left hand grip source to {weaponScript.leftHandGripSource.name}");
                    player.PlayerIKController.SetLeftHandGripSource(weaponScript.leftHandGripSource);
                }
                else
                {
                    Debug.LogWarning($"[{player.name}] AimState.SetupWeapon(): Left hand grip source not set on weapon prefab!");
                }
            }
            else
            {
                Debug.LogWarning($"[{player.name}] AimState.SetupWeapon(): Weapon script not found on weapon instance!");
            }
        }

        public override void Exit(PlayerState nextState)
        {
            Debug.Log($"[{player.name}] AimState.Exit(): Exiting to {nextState.GetType().Name}");
            base.Exit(nextState);

            if (ShouldResetAimState(nextState))
            {
                animationManager.SetIsAiming(false);
                player.PlayerIKController.DisableIK();
                player.AimPoseLayerWeightController.SetWeight(1f);
                player.AimPitchLayerWeightController.SetWeight(0f);
                player.PlayerCameraController.DisableCameraSway();
                player.PlayerCameraController.ResetCameraOffset();
                player.CrosshairController.DisableCrosshair();
            }
        }

        private bool ShouldResetAimState(PlayerState nextState)
        {
            return
                nextState is IdleState
                || (
                    nextState is StrafeState
                    && nextState is not ShootState
                    && nextState is not AimState
                )
                || nextState is SprintState
                || nextState is HitReactionState;
        }

        public override void LogicUpdate()
        {
            if (stateMachine.currentState != this)
                return;

            base.LogicUpdate();

            player.PlayerCameraController.ZoomIn();
            weaponManager.SetAimIKOffsets();

            if (HandleDebugMode())
                return;

            if (HandleStateTransitions())
                return;

            UpdateCrosshair();
            UpdateStrafeAndIdle();
            SetAimPitch();
        }

        private void SetAimPitch()
        {
            float aimPitch = player.PlayerCameraController.GetCameraPitch();
            animationManager.SetAimPitch(aimPitch);
        }  

        private bool HandleDebugMode()
        {
            if (PlayerDebugger.ForceAimDebugMode)
            {
                player.PlayerIKController.SetAimIkWeight(1f);
                player.PlayerIKController.SetFBBIKWeight(1f);
                player.PlayerIKController.SetHeadLookAtWeight(1f);
                if (player.PlayerInput.IsMoving)
                {
                    player.CrosshairController.ExpandAndContractCrosshair(
                        1f,
                        weaponManager.CurrentWeaponConfig.bulletSpreadHorizontal,
                        weaponManager.CurrentWeaponConfig.bulletSpreadVertical,
                        0.1f
                    );
                }
                return true;
            }
            return false;
        }

        private bool HandleStateTransitions()
        {
            if (player.PlayerInput.IsMoving && !player.PlayerInput.IsAiming)
            {
                stateMachine.SetState(player.strafe);
                return true;
            }

            if (CanShoot())
            {
                player.PlayerInput.ConsumeAttackBuffer();
                stateMachine.SetState(player.shoot);
                return true;
            }

            if (player.PlayerInput.IsReloading && weaponManager.CanReload())
            {
                stateMachine.SetState(player.reload);
                return true;
            }

            return false;
        }

        private bool CanShoot()
        {
            return player.PlayerInput.IsAiming &&
            player.PlayerInput.AttackBuffered &&
            stateMachine.currentState != player.shoot &&
            !animationManager.Animator.GetCurrentAnimatorStateInfo(animationManager.UpperBodyLayerIndex).IsTag("Shoot");
        }

        private void UpdateCrosshair()
        {
            if (player.PlayerInput.IsMoving)
            {
                player.CrosshairController.ExpandAndContractCrosshair(
                    1f,
                    weaponManager.CurrentWeaponConfig.bulletSpreadHorizontal,
                    weaponManager.CurrentWeaponConfig.bulletSpreadVertical,
                    0.1f
                );
            }
        }

    private void UpdateStrafeAndIdle()
    {
        if (player.PlayerInput.IsMoving)
        {
            animationManager.SetIsStrafing(true);
        }
        else
        {
            animationManager.SetIsStrafing(false);
            animationManager.SetIsIdle(true);
        }
    }

        public override void LateUpdate()
        {
            base.LateUpdate();

            player.PlayerCameraController.MoveAimIKTarget();
            player.PlayerCameraController.MoveBulletHitTarget();

            Vector3 direction = player.PlayerInput.GetInputDirection();
            Vector3 aimTarget = player.PlayerCameraController.GetAimTarget();

            // 1. Solve all IKs (AimIK, FBBIK, RecoilIK, etc.)
            player.PlayerIKController.UpdateIKs(direction, aimTarget);

            // Prevent crosshair expansion if we're in ShootState
            if (stateMachine.currentState == player.shoot)
                return;
        }
    }
}

