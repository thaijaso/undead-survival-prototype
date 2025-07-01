using UnityEngine;

public class AttackState : EnemyState
{
    public AttackState(
        Enemy enemy,
        StateMachine<EnemyState> stateMachine,
        AnimationManager animationManager,
        string animationName
    ) : base(
        enemy,
        stateMachine,
        animationManager,
        animationName
    )
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"[{enemy.name}] AttackState.Enter(): Entering Attack state - attack animation should start");
    }

    public override void Exit(EnemyState nextState)
    {
        base.Exit(nextState);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!enemy.IsPlayerInAttackRange())
        {
            Debug.Log($"[{enemy.name}] AttackState.LogicUpdate(): Player out of attack range - transitioning to Aggro state");
            // Transition back to Aggro state if player is out of attack range
            animationManager.SetIsAttacking(false); // Reset attacking state in animation manager
            animationManager.SetIsInAttackRange(false); // Reset attack range in animation manager
            stateMachine.SetState(enemy.Aggro);
            return;
        }

        animationManager.SetIsAttacking(true); // Set attacking state in animation manager
        animationManager.SetIsInAttackRange(true); // Ensure attack range is set in animation manager
    }

    public void OnAttackFinished()
    {
        Debug.Log($"[{enemy.name}] AttackState.OnAttackFinished(): Attack animation finished - transitioning to Aggro state");

        // Transition back to Aggro state
        stateMachine.SetState(enemy.Aggro);
    }

    public void OnAttackLostMomentum()
    {
        Debug.Log($"[{enemy.name}] AttackState.OnAttackLostMomentum(): Attack lost momentum - transitioning to Aggro state");
        enemy.SetAndLogSpeed(0, "AttackState.OnAttackLostMomentum()", 0f);
    }
}
