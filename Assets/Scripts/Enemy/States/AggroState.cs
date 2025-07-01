using UnityEngine;
using Pathfinding;
using System.Collections;

public class AggroState : EnemyState
{
    private Coroutine turnCoroutine;

    public AggroState(
        Enemy enemy,
        StateMachine<EnemyState> stateMachine,
        AnimationManager animationManager,
        string animationName,
        Transform player
    ) : base(
        enemy,
        stateMachine,
        animationManager,
        animationName)
    {
    }

    public override void Enter()
    {
        Debug.Log($"[{enemy.name}] AggroState: Entering - IsTurning is {enemy.IsTurning}");
        base.Enter();
        Debug.Log($"[{enemy.name}] AggroState: Aggro animation should start");
        animationManager.SetIsAggro(true);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Check if player is in attack range (primary condition)
        if (enemy.IsPlayerInAttackRange())
        {
            // Transition to Attack state
            stateMachine.SetState(enemy.Attack);
            return;
        }

        float angleToPlayer = GetAngleToPlayer();
    
        if (Mathf.Abs(angleToPlayer) > 90f && !enemy.IsTurning)
        {
            Debug.Log($"[{enemy.name}] AggroState: Starting turn animation - IsTurning is currently {enemy.IsTurning}");
            PlayTurnAnimation(angleToPlayer);
        }
    }

    public void OnAggroAnimationFinished()
    {
        Debug.Log($"[{enemy.name}] AggroState: Aggro180 animation finished - enabling AI movement");

        // Reset turning flag
        enemy.IsTurning = false;

        // Enable AI movement
        enemy.AIDestinationSetter.enabled = true;
        enemy.FollowerEntity.enabled = true;
        enemy.SetAndLogSpeed(enemy.GetChaseSpeed(), "AggroState.OnAggro180Finished()");
    }

    private void PlayTurnAnimation(float angle)
    {
        Debug.Log($"[{enemy.name}] AggroState: Starting Aggro180 turn for angle {angle:F1}° - IsTurning was {enemy.IsTurning}");
        enemy.IsTurning = true; // Set shared flag
        Debug.Log($"[{enemy.name}] AggroState: Set IsTurning to {enemy.IsTurning}");

        // Stop any existing rotation coroutine before starting new one
        if (turnCoroutine != null)
        {
            enemy.StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }

        // Calculate target rotation
        Vector3 directionToPlayer = enemy.GetPlayerTransform().position - enemy.transform.position;
        directionToPlayer.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

        animationManager.SetIsTurning(true);

        // Trigger Aggro180 for any large turn
        animationManager.SetTrigger("Aggro180");

        // Use aggro-specific two-phase rotation timing
        turnCoroutine = enemy.StartCoroutine(RotateWithAggroAnimation(targetRotation));
    }

    public override void Exit(EnemyState nextState)
    {
        base.Exit(nextState);

        // Stop any rotation coroutines when exiting the state
        if (turnCoroutine != null)
        {
            enemy.StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }

        Debug.Log($"[{enemy.name}] AggroState: Exiting state");
    }

    public void OnTurnFinished()
    {
        Debug.Log($"[{enemy.name}] AggroState: Turn animation finished - resetting IsTurning to false");
        animationManager.SetIsTurning(false);
        enemy.IsTurning = false; // Reset shared flag
    }
}
