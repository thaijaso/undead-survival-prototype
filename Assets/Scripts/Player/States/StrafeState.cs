using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{ 

    public class StrafeState : MoveState
    {
        private float strafeSpeed = 2.0f;

        public StrafeState(
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
            strafeSpeed = player.PlayerCharacterController.strafeSpeed;
        }

        public override void Enter()
        {
            base.Enter();
            Debug.Log($"[{player.name}] StrafeState.Enter(): Entering Strafe state");
            //animationManager.SetIsStrafing(true);
            animationManager.SetIsIdle(false);
        }

        public override void Exit(PlayerState nextState)
        {
            Debug.Log($"[{player.name}] StrafeState.Exit(): Exiting to {nextState.GetType().Name}");
            base.Exit(nextState);
            animationManager.SetIsStrafing(false);
        }

        public override void LogicUpdate()
        {
            // Guard: Only update if this is the current state
            if (stateMachine.currentState != this)
            {
                return;
            }

            // Prevent automatic transitions if debug mode is active
            if (IsDebugAimLockActive)
            {
                return;
            }

            base.LogicUpdate();
            HandleMovement(strafeSpeed, false);

            if (!player.PlayerInput.IsMoving && !player.PlayerInput.IsAiming && stateMachine.currentState != player.reload)
            {
                stateMachine.SetState(player.idle);
                return;
            }

            if (player.PlayerInput.IsSprinting && player.PlayerInput.IsMoving && !player.PlayerInput.IsAiming)
            {
                stateMachine.SetState(player.sprint);
                return;
            }

            if (player.PlayerInput.IsAiming
                && stateMachine.currentState != player.aim
                && stateMachine.currentState != player.shoot
                && stateMachine.currentState != player.reload)
            {
                stateMachine.SetState(player.aim);
                return;
            }

            if (player.PlayerInput.IsMoving)
            {
                animationManager.SetIsStrafing(true);
            }
            else
            {
                animationManager.SetIsStrafing(false);
            }
        }
    }
}
