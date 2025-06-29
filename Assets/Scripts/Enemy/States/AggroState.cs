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

        // Sync turn animaion with transform rotation
        PlayTurnAnimation(GetAngleToPlayer());
    }

    public void OnYellFinished()
    {
        Debug.Log("AggroState: Yell animation finished - enabling AI movement");

        // Set the final rotation after the 180° turn animation completes
        //enemy.transform.rotation = targetRotationAfterAggro;

        // Enable AI movement
        //enemy.AIDestinationSetter.enabled = true;
    }

        private void PlayTurnAnimation(float angle)
    {
        //Debug.Log("AlertState: Playing turn animation with angle: " + angle);
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
                
                // Start the rotation coroutine to match animation timing
                turnCoroutine = enemy.StartCoroutine(RotateWithAnimation(targetRotation));
            }
        }
    }
    
    private IEnumerator RotateWithAnimation(Quaternion targetRotation)
    {
        Quaternion startRotation = enemy.transform.rotation;
        
        // Phase 1: Slow rotation (first part of animation)
        // Adjust these values to match your animation timing
        float slowPhaseDuration = enemy.template.aggro180Phase1Duration; // Duration of slow rotation phase
        float slowPhaseProgress = 0.5f; // How much to rotate during slow phase (30%)
        
        float elapsedTime = 0f;
        while (elapsedTime < slowPhaseDuration)
        {
            float progress = (elapsedTime / slowPhaseDuration) * slowPhaseProgress;
            enemy.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Phase 2: Fast rotation (second part of animation)
        float fastPhaseDuration = enemy.template.aggro180Phase2Duration; // Duration of fast rotation phase
        elapsedTime = 0f;
        Quaternion midRotation = enemy.transform.rotation;
        
        while (elapsedTime < fastPhaseDuration)
        {
            float progress = elapsedTime / fastPhaseDuration;
            enemy.transform.rotation = Quaternion.Slerp(midRotation, targetRotation, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end up exactly at target rotation
        enemy.transform.rotation = targetRotation;
        turnCoroutine = null;
    }

}
