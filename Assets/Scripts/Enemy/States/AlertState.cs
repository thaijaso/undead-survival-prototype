using System;
using System.Collections;
using UnityEngine;

public class AlertState : EnemyState
{
    public bool hasTurned = false;
    private float alertTimer = 0f;
    private float alertDuration = 5f; // Duration for which the enemy remains alert
    private Coroutine turnCoroutine;

    public AlertState(
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
        alertTimer = 0f; // Initialize alert timer
        alertDuration = enemy.template.alertDuration; // Set alert duration from template
    }

    public override void Enter()
    {
        base.Enter();
        alertTimer = 0f; // Reset the alert timer
        animationManager.SetAlertState(true); // Set alert state in animation manager
    }

    public override void Exit(EnemyState nextState)
    {
        base.Exit(nextState);
        animationManager.SetAlertState(false); // Reset alert state in animation manager
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        alertTimer += Time.deltaTime; // Increment the alert timer

        // Check if zombie is in aggro range
        if (enemy.IsPlayerInAggroRange())
        {
            // If player is in aggro range, transition to aggro state
            stateMachine.SetState(enemy.Aggro);
            return;
        }

        // Check if the alert duration has passed
        if (enemy.IsPlayerInAlertRange())
        {

            // If player is still in range, stay in alert state
            animationManager.SetAlertState(true);

            float angle = GetAngleToPlayer();
            PlayTurnAnimation(angle);
        }
        else
        {
            // If player is out of range, transition to idle state
            stateMachine.SetState(enemy.Idle);
        }
    }

    public void OnTurnFinished()
    {
        Debug.Log("AlertState: Turn animation finished.");
        hasTurned = false;
        
        // Set the animator bool to false so the turn animation can exit
        animationManager.SetHasTurned(false);
        
        // Stop the rotation coroutine if it's still running
        if (turnCoroutine != null)
        {
            enemy.StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }
    }

    private void PlayTurnAnimation(float angle)
    {
        //Debug.Log("AlertState: Playing turn animation with angle: " + angle);
        if (Math.Abs(angle) > 90f)
        {
            if (!hasTurned)
            {
                hasTurned = true;
                
                // Calculate target rotation
                Vector3 directionToPlayer = enemy.GetPlayerTransform().position - enemy.transform.position;
                directionToPlayer.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                
                if (angle > 0f)
                {
                    // Player is to the right, turn right
                    animationManager.SetTrigger("TurnRight180");
                    animationManager.SetHasTurned(true);
                }
                else
                {
                    // Player is to the left, turn left
                    animationManager.SetTrigger("TurnLeft180");
                    animationManager.SetHasTurned(true);
                }
                
                // Start the rotation coroutine to match animation timing (uses base class method)
                turnCoroutine = enemy.StartCoroutine(RotateWithAnimation(targetRotation));
            }
        }
    }
}
