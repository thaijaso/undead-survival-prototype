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
        Debug.Log("AttackState: Entering Attack state - attack animation should start");

        animationManager.SetIsInAttackRange(true); // Set the attack animation
        enemy.FollowerEntity.maxSpeed = 0f; // Stop movement during attack
    }
}
