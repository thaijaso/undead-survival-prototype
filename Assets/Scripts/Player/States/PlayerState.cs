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
                Debug.Log("PlayerState.LogicUpdate(): Player menu button pressed.");
                player.PlayerMenuUIController.TogglePlayerMenu();
            }

            if (player.PlayerInput.IsInteracting && player.InteractionSensor.CurrentInteractable != null)
            {
                player.InteractionSensor.CurrentInteractable.Interact(player);
            }
        }

        public virtual void PhysicsUpdate()
        {
            // Physics logic to be implemented in derived classes
        }

        public virtual void LateUpdate()
        {
            // Late update logic to be implemented in derived classes
        }
    }
}

