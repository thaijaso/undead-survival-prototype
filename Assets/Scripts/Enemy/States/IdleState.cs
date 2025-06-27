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
        }

        public override void Exit(EnemyState nextState)
        {
            base.Exit(nextState);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
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