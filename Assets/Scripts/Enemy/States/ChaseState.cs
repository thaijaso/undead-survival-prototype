using UnityEngine;
using Pathfinding;
using System.Collections;

// ChaseState handles the enemy's pursuit behavior after initial aggro
public class ChaseState : EnemyState
{

    public ChaseState(
        Enemy enemy,
        StateMachine<EnemyState> stateMachine,
        AnimationManager animationManager,
        string animationName
    ) : base(
        enemy,
        stateMachine,
        animationManager,
        animationName)
    {
    }

    public override void Enter()
    {
        Debug.Log($"[{enemy.name}] ChaseState.Enter(): Entering chase state");
        base.Enter();
        
        // Ensure AI movement is enabled
        enemy.AIDestinationSetter.enabled = true;
        enemy.FollowerEntity.enabled = true;
        enemy.SetAndLogSpeed(enemy.GetChaseSpeed(), "ChaseState.Enter()");
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Skip automatic transitions if debug mode is enabled
        if (enemy.DebugModeEnabled)
        {
            return;
        }

        // Check if player is in attack range (primary condition)
        if (enemy.IsPlayerInAttackRange())
        {
            // Transition to Attack state
            stateMachine.SetState(enemy.Attack);
            return;
        }
    }

    public override void Exit(EnemyState nextState)
    {
        base.Exit(nextState);
        Debug.Log($"[{enemy.name}] ChaseState.Exit(): Exiting chase state");
    }
}
