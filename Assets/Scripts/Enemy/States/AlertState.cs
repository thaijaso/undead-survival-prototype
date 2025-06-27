using UnityEngine;

public class AlertState : EnemyState
{
    private float alertTimer = 0f;
    private float alertDuration = 5f; // Duration for which the enemy remains alert

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

            // Turn towards the player
            float angle = GetAngleToPlayer();
            animationManager.SetTurnAngle(angle);

            // Rotate the enemy
            RotateTowardsPlayer(angle);
        }
        else
        {
            // If player is out of range, transition to idle state
            stateMachine.SetState(enemy.Idle);
        }
    }

    private float GetAngleToPlayer()
    {
        Vector3 directionToPlayer = enemy.GetPlayerTransform().position - enemy.transform.position;
        return Vector3.SignedAngle(enemy.transform.forward, directionToPlayer, Vector3.up);
    }

    private void RotateTowardsPlayer(float angle)
    {
        Quaternion targetRotation = Quaternion.LookRotation(enemy.GetPlayerTransform().position - enemy.transform.position, Vector3.up);

        if (Mathf.Abs(angle) > enemy.template.turnTheshold)
        {
            float step = enemy.template.rotationSpeed * Time.deltaTime;
            enemy.transform.rotation = Quaternion.RotateTowards(enemy.transform.rotation, targetRotation, step);
        }
    }
}
