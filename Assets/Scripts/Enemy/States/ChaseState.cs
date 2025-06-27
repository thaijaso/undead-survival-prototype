using Pathfinding;
using UnityEngine;

public class ChaseState : EnemyState
{
    private Transform playerTransform;

    private float chaseSpeed;

    public ChaseState(
        Enemy enemy,
        StateMachine<EnemyState> stateMachine,
        AnimationManager animationManager,
        string animationName,
        Transform playerTransform
    ) : base(
        enemy,
        stateMachine,
        animationManager,
        animationName
    )
    {
        chaseSpeed = enemy.GetChaseSpeed();
        this.playerTransform = playerTransform;
    }

    public override void Enter()
    {
        base.Enter();
        // Logic for entering chase state, e.g., setting the player as the target
        animationManager.SetIsMoving(true);
        animationManager.SetMoveParams(1f, 1f);
        destinationSetter.target = playerTransform;
        destinationSetter.enabled = true;
        followerEntity.maxSpeed = chaseSpeed;
    }

    public override void Exit(EnemyState nextState)
    {
        base.Exit(nextState);
        // Logic for exiting chase state, e.g., stopping movement
        animationManager.SetIsMoving(false);
        animationManager.SetMoveParams(0f, 0f);
        destinationSetter.enabled = false;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        // Logic for chase state, e.g., following the player
        destinationSetter.target = playerTransform;
    }
}