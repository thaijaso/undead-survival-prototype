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
        Debug.Log($"AggroState.Enter() - IsTurning is {enemy.IsTurning} at start");
        base.Enter();
        Debug.Log($"AggroState.Enter() - IsTurning is {enemy.IsTurning} after base.Enter()");
        
        Debug.Log("AggroState: Entering Aggro state - yell animation should start");
        animationManager.SetIsAggro(true);
        Debug.Log($"AggroState.Enter() - IsTurning is {enemy.IsTurning} at end");
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
        Debug.Log($"AggroState.LogicUpdate: angleToPlayer={angleToPlayer:F1}°, IsTurning={enemy.IsTurning}");
        if (Mathf.Abs(angleToPlayer) > 90f && !enemy.IsTurning)
        {
            Debug.Log($"AggroState: About to call PlayTurnAnimation - IsTurning is currently {enemy.IsTurning}");
            PlayTurnAnimation(angleToPlayer);
        }
    }

    public void OnYellFinished()
    {
        Debug.Log("AggroState: Yell animation finished - enabling AI movement");

        // Set the final rotation after the 180° turn animation completes
        //enemy.transform.rotation = targetRotationAfterAggro;

        // Enable AI movement
        enemy.AIDestinationSetter.enabled = true;
        enemy.FollowerEntity.enabled = true;
        enemy.SetAndLogSpeed(enemy.GetChaseSpeed(), "AggroState.OnYellFinished()");
    }

    private void PlayTurnAnimation(float angle)
    {
        Debug.Log($"AggroState: Starting Aggro180 turn for angle: {angle:F1}° - IsTurning was {enemy.IsTurning}");
        enemy.IsTurning = true; // Set shared flag
        Debug.Log($"AggroState: Set IsTurning to {enemy.IsTurning}");

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

        Debug.Log("AggroState: Exiting Aggro state");
    }

    public void OnTurnFinished()
    {
        Debug.Log("AggroState: OnTurnFinished() called - resetting IsTurning to false");
        animationManager.SetIsTurning(false);
        enemy.IsTurning = false; // Reset shared flag
    }
}
