using andywiecko.BurstTriangulator;
using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{
    public class PlayerState : IState<PlayerState>
    {
        protected Player player;
        protected StateMachine<PlayerState> stateMachine;
        protected AnimationManager animationManager;
        protected string animationName;
        protected PlayerWeaponManager weaponManager;

        /// <summary>
        /// Returns true if debug aim lock is active (prevents automatic state transitions for offset editing)
        /// </summary>
        protected virtual bool IsDebugAimLockActive => PlayerDebugger.ForceAimDebugMode;

        public PlayerState(
            Player player,
            StateMachine<PlayerState> stateMachine,
            AnimationManager animationManager,
            string animationName,
            PlayerWeaponManager weaponManager
        )
        {
            this.player = player;
            this.stateMachine = stateMachine;
            this.animationManager = animationManager;
            this.animationName = animationName;
            this.weaponManager = weaponManager;
        }

        public virtual void Enter()
        {
            // Logic to be implemented in derived classes
        }

        public virtual void Exit(PlayerState nextState)
        {
            animationManager.StopAnimation();
        }

        public virtual void LogicUpdate()
        {
            if (stateMachine.currentState != player.aim && stateMachine.currentState != player.shoot)
            {
                player.PlayerCameraController.ZoomOut();
            }

            if (player.PlayerInput.IsPlayerMenuPressed)
            {
                player.PlayerMenuUIController.TogglePlayerMenu();
            }

            if (player.PlayerInput.IsInteracting && player.InteractionSensor.CurrentInteractable != null)
            {
                player.InteractionSensor.CurrentInteractable.Interact(player);
            }

            if (player.WeaponManager.IsUnarmed)
            {
                player.UpperBodyLayerWeightController.SetWeight(0f);
            }
            else
            {
                player.UpperBodyLayerWeightController.SetWeight(1f);
            }

            HandleAnimatorParams();
            HandleOrbitalCollisionParams();
            HandleFadeCutoffParam();
        }

        private void HandleAnimatorParams()
        {
            animationManager.SetSprintStopGracePeriodFinished(player.PlayerInput.SprintStopGracePeriodFinished);
            animationManager.SetIsUnarmed(weaponManager.IsUnarmed);
            animationManager.SetIsPistolEquipped(weaponManager.IsPistolEquipped);
            animationManager.SetIsRevolverEquipped(weaponManager.IsRevolverEquipped);
            animationManager.SetIsLocke17Equipped(weaponManager.IsLocke17Equipped);
            HandleWallDetectedAnimatorParam();
            HandleStairDetectedAnimatorParam();
        }

        private void HandleWallDetectedAnimatorParam()
        {
            if (player.WallDetector.IsWallDetected)
            {
                animationManager.SetIsFacingWall(true);
            }
            else
            {
                animationManager.SetIsFacingWall(false);
            }
        }

        private void HandleStairDetectedAnimatorParam()
        {
            if (player.StairDetector.IsStairDetected)
            {
                animationManager.SetIsOnStairs(true);
            }
            else
            {
                animationManager.SetIsOnStairs(false);
            }
        }

        private void HandleOrbitalCollisionParams()
        {
            if (player.IsInside)
            {
                player.PlayerCameraController.SetMaxBoomTarget(1f); // TODO: define in player template
            }
            else
            {
                player.PlayerCameraController.SetMaxBoomTarget(2f); // TODO: define in player template
            }
        }
        
        private void HandleFadeCutoffParam()
        {
            bool isBlocking = player.CenterZoneOverlapCalculator.isPlayerBlockingView;
            float cutoffValue = isBlocking ? 1f : 0f; // 1 = fully cut, 0 = opaque
            player.AlphaCutoffController.SetCutoff(cutoffValue);
        }

        public virtual void PhysicsUpdate()
        {
            // Physics logic to be implemented in derived classes
        }

        public virtual void LateUpdate()
        {
            //player.PlayerIKController.UpdateLeftHand();
        }
    }
}

