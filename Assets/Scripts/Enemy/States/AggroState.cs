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
        Debug.Log($"[{enemy.name}] AggroState.Enter(): Entering - IsTurning is {enemy.IsTurning}");
        base.Enter();
        Debug.Log($"[{enemy.name}] AggroState.Enter(): Aggro animation should start");
        animationManager.SetIsAggro(true);
        
        // Mark that this enemy has been aggroed
        enemy.SetHasAggroed(true);
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // In debug mode, stay in aggro state but handle turning
        if (enemy.DebugModeEnabled)
        {
            CheckAndHandleTurning();
            return;
        }

        // Normal logic: aggro state should transition to chase after animation finishes
        // The actual transition happens in OnAggroAnimationFinished()
        CheckAndHandleTurning();
    }

    private void CheckAndHandleTurning()
    {
        float angleToPlayer = GetAngleToPlayer();
        if (Mathf.Abs(angleToPlayer) > 90f && !enemy.IsTurning)
        {
            Debug.Log($"[{enemy.name}] AggroState.CheckAndHandleTurning(): Starting turn animation - IsTurning is currently {enemy.IsTurning}");
            PlayTurnAnimation(angleToPlayer);
        }
    }

    public void OnAggroAnimationFinished()
    {
        Debug.Log($"[{enemy.name}] AggroState.OnAggroAnimationFinished(): Aggro animation finished - transitioning to Chase state");
        animationManager.SetHasAgroAnimationFinished(true);
        animationManager.SetIsTurning(false);

        // Reset turning flag
        enemy.IsTurning = false;


        // Transition to chase state
        stateMachine.SetState(enemy.Chase);
    }

    private void PlayTurnAnimation(float angle)
    {
        Debug.Log($"[{enemy.name}] AggroState.PlayTurnAnimation(): Starting Aggro180 turn for angle {angle:F1}° - IsTurning was {enemy.IsTurning}");
        enemy.IsTurning = true; // Set shared flag
        Debug.Log($"[{enemy.name}] AggroState.PlayTurnAnimation(): Set IsTurning to {enemy.IsTurning}");

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

        // Clean up animation state
        if (nextState != enemy.Attack)
        {
            animationManager.SetIsAggro(false);
        }

        // Stop any rotation coroutines when exiting the state
        if (turnCoroutine != null)
        {
            enemy.StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }

        Debug.Log($"[{enemy.name}] AggroState.Exit(): Exiting state");
    }

    public void OnTurnFinished()
    {
        Debug.Log($"[{enemy.name}] AggroState.OnTurnFinished(): Turn animation finished - resetting IsTurning to false");
        animationManager.SetIsTurning(false);
        enemy.IsTurning = false; // Reset shared flag
    }
    
    public void OnAttackLostMomentum()
    {
        Debug.Log($"[{enemy.name}] AggroState.OnAttackLostMomentum(): Attack lost momentum - resetting speed to 0");
        enemy.SetAndLogSpeed(0, "AggroState.OnAttackLostMomentum()", 0f);
    }
}
