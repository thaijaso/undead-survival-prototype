using System;
using System.Collections;
using UnityEngine;

public class AlertState : EnemyState
{
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
        enemy.LookAtIK.enabled = true;
    }

    public override void Exit(EnemyState nextState)
    {
        base.Exit(nextState);
        animationManager.SetAlertState(false); // Reset alert state in animation manager
        
        // Only disable LookAtIK if transitioning to a non-tracking state
        if (nextState == enemy.Idle || nextState == enemy.Patrol || nextState == enemy.Death)
        {
            enemy.LookAtIK.enabled = false;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        alertTimer += Time.deltaTime; // Increment the alert timer

        // Skip all automatic transitions if debug mode is enabled
        if (enemy.DebugModeEnabled)
        {
            // Still handle turning animation if player is in range
            if (enemy.IsPlayerInAlertRange())
            {
                float angle = GetAngleToPlayer();
                PlayTurnAnimation(angle);
            }
            return; // Exit early to prevent any state transitions
        }

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
        Debug.Log($"[{enemy.name}] AlertState.OnTurnFinished(): Turn animation finished");
        
        // Only reset IsTurning if we're still in AlertState
        // If we've transitioned to another state, let that state manage IsTurning
        if (stateMachine.currentState == this)
        {
            Debug.Log($"[{enemy.name}] AlertState.OnTurnFinished(): Still in AlertState, resetting IsTurning to false");
            enemy.IsTurning = false;
            Debug.Log($"[{enemy.name}] AlertState.OnTurnFinished(): IsTurning = {enemy.IsTurning}");
            
            // Set the animator bool to false so the turn animation can exit
            animationManager.SetIsTurning(false);
        }
        else
        {
            Debug.Log($"[{enemy.name}] AlertState.OnTurnFinished(): Called but current state is {stateMachine.currentState.GetType().Name}, not AlertState - not resetting IsTurning");
        }
        
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
            if (!enemy.IsTurning)
            {
                enemy.IsTurning = true;
                Debug.Log($"[{enemy.name}] AlertState.PlayTurnAnimation(): Starting Alert180 turn for angle {angle:F1}° - IsTurning was {enemy.IsTurning}");
                
                // Calculate target rotation
                Vector3 directionToPlayer = enemy.GetPlayerTransform().position - enemy.transform.position;
                directionToPlayer.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                
                if (angle > 0f)
                {
                    // Player is to the right, turn right
                    animationManager.SetTrigger("TurnRight");
                    animationManager.SetIsTurning(true);
                }
                else
                {
                    // Player is to the left, turn left
                    animationManager.SetTrigger("TurnLeft");
                    animationManager.SetIsTurning(true);
                }
                
                // Start the rotation coroutine to match animation timing (uses base class method)
                turnCoroutine = enemy.StartCoroutine(RotateWithAnimation(targetRotation));
            }
        }
    }
}
