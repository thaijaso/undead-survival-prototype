using UnityEngine;
using Pathfinding;
using System.Collections;

public class AggroState : EnemyState
{
    private Coroutine turnCoroutine;
    public bool hasTurned = false;

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
        base.Enter();
        Debug.Log("AggroState: Entering Aggro state - yell animation should start");

        // Stop any existing rotation coroutines from previous states
        if (turnCoroutine != null)
        {
            enemy.StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }

        // Reset turn state
        hasTurned = false;

        animationManager.SetIsAggro(true); // Set aggro state in animation manager
        enemy.SetAndLogSpeed(0, "AggroState.Enter()");
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

        // Sync turn animation with transform rotation
        PlayTurnAnimation(GetAngleToPlayer());
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
        //Debug.Log("AggroState: Playing turn animation with angle: " + angle);
        if (Mathf.Abs(angle) > 90f)
        {
            if (!hasTurned)
            {
                hasTurned = true;

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

                if (angle < 0f)
                {
                    animationManager.animator.SetTrigger("Aggro180");
                }

                // Use aggro-specific two-phase rotation timing
                turnCoroutine = enemy.StartCoroutine(RotateWithAggroAnimation(targetRotation));
            }
        }
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
        animationManager.SetIsTurning(false);
    }

}
