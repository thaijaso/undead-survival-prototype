using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{
    public class IdleState : PlayerState
    {
        public IdleState(
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
        {}

        public override void Enter()
        {
            Debug.Log($"[{player.name}] IdleState.Enter(): Entering Idle state.");

            animationManager.SetIsIdle(true);
            animationManager.SetIsStrafing(false);
            animationManager.SetMoveParams(0f, 0f);

            player.PlayerIKController.SetIKTargetWeight(0f);
            player.ArmsLayerWeightController.SetWeight(1f);
        }

        public override void Exit(PlayerState nextState)
        {
            Debug.Log($"[{player.name}] IdleState.Exit(): Exiting to {nextState.GetType().Name}.");
            animationManager.SetIsIdle(false);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            player.PlayerCharacterController.Move(Vector3.zero, 0f);

            if (!player.PlayerInput.IsSprinting && player.PlayerInput.IsMoving)
            {
                stateMachine.SetState(player.strafe);
                return;
            }

            if (player.PlayerInput.IsSprinting && player.PlayerInput.IsMoving)
            {
                stateMachine.SetState(player.sprint);
                return;
            }

            if (player.PlayerInput.IsAiming)
            {
                stateMachine.SetState(player.aim);
                return;
            }

            if (player.PlayerInput.IsReloading && weaponManager.CanReload())
            {
                stateMachine.SetState(player.reload);
                return;
            }     
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            player.PlayerIKController.BlendAllIKWeights();
        }
    }
}
