using UnityEngine;

namespace EnemyStates
{
    public class IdleState : EnemyState
    {
        public IdleState(
            Enemy enemy,
            StateMachine<EnemyState> stateMachine,
            AnimationManager animationManager,
            string animationName
        ) : base(
            enemy,
            stateMachine,
            animationManager,
            animationName
        ) { }

        public override void Enter()
        {
            base.Enter();
            
            // Set animation parameters for idle state
            animationManager.SetAlertState(false);
            animationManager.SetIsAggro(false);
            animationManager.SetIsAttacking(false);
            animationManager.SetIsInAttackRange(false);
            animationManager.SetIsTurning(false);
            
            // Explicitly set idle state to true
            animationManager.SetIsIdle(true);
            
            // Set movement speed to 0 for idle
            enemy.SetSpeed(0f);
            
            Debug.Log($"[{enemy.name}] IdleState.Enter(): Set to idle animations and stopped movement");
        }

        public override void Exit(EnemyState nextState)
        {
            base.Exit(nextState);
            
            // Turn off idle state when leaving
            animationManager.SetIsIdle(false);
            
            Debug.Log($"[{enemy.name}] IdleState.Exit(): Turned off idle animation");
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            
            // Skip automatic transitions if debug mode is enabled
            if (enemy.DebugModeEnabled)
            {
                return;
            }
            
            // Logic for idle state, e.g., checking for player proximity
            if (enemy.IsPlayerInAlertRange())
            {
                stateMachine.SetState(enemy.Alert);
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
            // Physics-related logic for idle state
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            // Cleanup or final adjustments for the idle state
        }
    }
}