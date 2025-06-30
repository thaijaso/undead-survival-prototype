using UnityEngine;
using Pathfinding;
using System.Collections;

public class AggroState : EnemyState
{
    private Coroutine turnCoroutine;
    private float entryTime;
    // Prevents immediate turning when entering AggroState after Alert→Aggro transition
    // Without this cooldown, zombie would often double-turn: Alert finishes 180° turn, then AggroState immediately starts another
    // This provides a safety buffer beyond the IsTurning flag to ensure clean animation transitions
    // TODO: Consider removing this if animation exit times and IsTurning flag prove sufficient after extensive testing
    private const float TURN_COOLDOWN_DURATION = 0.5f; // Half second cooldown before allowing turns

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

        entryTime = Time.time; // Record when we entered

        // Stop any existing rotation coroutines from previous states
        if (turnCoroutine != null)
        {
            enemy.StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }

        // Reset turn state
        IsTurning = false; // Reset base class flag

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

        // Only try to turn if we're not already turning AND enough time has passed since entering
        if (!IsTurning && (Time.time - entryTime) > TURN_COOLDOWN_DURATION)
        {
            // Sync turn animation with transform rotation
            PlayTurnAnimation(GetAngleToPlayer());
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
        Debug.Log($"AggroState: Checking turn with angle: {angle:F1}°");
        
        // Only turn if angle is greater than 90° AND we're not already turning
        if (Mathf.Abs(angle) > 90f)
        {
            if (!IsTurning)
            {
                Debug.Log($"AggroState: Starting Aggro180 turn for angle: {angle:F1}°");
                IsTurning = true; // Set base class flag

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
        else
        {
            Debug.Log($"AggroState: Angle {angle:F1}° is within range, no turn needed");
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
        IsTurning = false; // Reset base class flag
    }

}
