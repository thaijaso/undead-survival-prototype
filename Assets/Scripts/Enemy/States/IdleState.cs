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
            // Set IsIdle to true FIRST to ensure Attack -> Idle transition has priority
            animationManager.SetIsIdle(true);
            
            // Then clear other parameters
            animationManager.SetAlertState(false);
            animationManager.SetIsAggro(false);
            animationManager.SetIsAttacking(false);
            animationManager.SetIsInAttackRange(false);
            animationManager.SetIsTurning(false);
            
            // Clear movement parameters that might trigger Chase transitions
            animationManager.SetIsMoving(false);
            
            // Set movement speed to 0 for idle
            enemy.SetSpeed(0f);
            
            Debug.Log($"[{enemy.name}] IdleState.Enter(): Entered idle state and stopped movement");
        }

        public override void Exit(EnemyState nextState)
        {
            base.Exit(nextState);
            
            // Turn off idle state when leaving
            animationManager.SetIsIdle(false);
            
            Debug.Log($"[{enemy.name}] IdleState.Exit(): Exiting to {nextState?.GetType().Name ?? "Unknown"} state");
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