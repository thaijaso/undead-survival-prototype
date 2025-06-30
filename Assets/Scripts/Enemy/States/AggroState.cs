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

        animationManager.SetIsAggro(true); // Set aggro state in animation manager
        animationManager.animator.SetInteger("AggroType", 1);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

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
    }

    private void PlayTurnAnimation(float angle)
    {
        //Debug.Log("AggroState: Playing turn animation with angle: " + angle);
        if (Mathf.Abs(angle) > 90f)
        {
            if (!hasTurned)
            {
                hasTurned = true;
                
                // Calculate target rotation
                Vector3 directionToPlayer = enemy.GetPlayerTransform().position - enemy.transform.position;
                directionToPlayer.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                
                animationManager.SetHasTurned(true);
                
                // Use aggro-specific two-phase rotation timing
                turnCoroutine = enemy.StartCoroutine(RotateWithAggroAnimation(targetRotation));
            }
        }
    }

}
