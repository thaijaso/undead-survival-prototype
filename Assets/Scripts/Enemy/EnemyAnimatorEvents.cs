using UndeadSurvivalGame.Enemy;
using UndeadSurvivalGame.Enemy.States;
using UnityEngine;

public class EnemyAnimatorEvents : MonoBehaviour
{
    private Enemy enemy;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError($"[{name}] EnemyAnimatorEvents: No Enemy component found on this GameObject.");
            enabled = false; // Disable this script if no Enemy is found
        }
    }

    // ====================================================
    // ANIMATION EVENTS - Called directly by Unity Animator
    // ====================================================

    /// <summary>
    /// Called when turn animations finish (TurnLeft180, TurnRight180, Aggro180)
    /// Delegates to the appropriate state handler
    /// </summary>
    public void OnTurnFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnTurnFinished(): Turn animation finished. Current state: {enemy.stateMachine.currentState.GetType().Name}");

        // Delegate to the current state if it handles turn finishing
        if (enemy.stateMachine.currentState == enemy.Alert && enemy.Alert is AlertState alertState)
        {
            Debug.Log($"[{name}] EnemyAnimatorEvents.OnTurnFinished(): Delegating to AlertState");
            alertState.OnTurnFinished();
        }
        else if (enemy.stateMachine.currentState == enemy.Aggro && enemy.Aggro is AggroState aggroState)
        {
            Debug.Log($"[{name}] EnemyAnimatorEvents.OnTurnFinished(): Delegating to AggroState");
            aggroState.OnTurnFinished();
        }
        else
        {
            Debug.Log($"[{name}] EnemyAnimatorEvents.OnTurnFinished(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnAggroAnimStarted()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnAggroAnimStarted(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        if (enemy.stateMachine.currentState is EnemyState state)
        {
            Debug.Log($"[{name}] EnemyAnimatorEvents.OnAggroAnimStarted(): Delegating to {state.GetType().Name}");
            state.OnAggroAnimStarted();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnAggroAnimStarted(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    /// <summary>
    /// Called when aggro animation sequence finishes
    /// Transitions from Aggro to Chase state
    /// </summary>
    public void OnAggroAnimationFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnAggroAnimationFinished(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        if (enemy.stateMachine.currentState == enemy.Aggro && enemy.Aggro is AggroState aggroState)
        {
            Debug.Log($"[{name}] EnemyAnimatorEvents.OnAggroAnimationFinished(): Delegating to AggroState");
            aggroState.OnAggroAnimationFinished();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnAggroAnimationFinished(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    /// <summary>
    /// Called when attack animations finish
    /// Handles post-attack state transitions
    /// </summary>
    public void OnAttackFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnAttackFinished(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it's AttackState
        if (enemy.stateMachine.currentState == enemy.Attack && enemy.Attack is AttackState attackState)
        {
            attackState.OnAttackFinished();
        }
    }

    /// <summary>
    /// Called when attack loses momentum/force
    /// Used for physics-based attack feedback
    /// </summary>
    public void OnAttackLostMomentum()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnAttackLostMomentum(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to appropriate state handlers
        if (enemy.stateMachine.currentState == enemy.Attack && enemy.Attack is AttackState attackState)
        {
            attackState.OnAttackLostMomentum();
        }
        else if (enemy.stateMachine.currentState == enemy.Aggro && enemy.Aggro is AggroState aggroState)
        {
            aggroState.OnAttackLostMomentum();
        }
    }

    public void OnForwardKnockbackFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnForwardKnockbackFinished(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it handles hit reactions
        if (enemy.stateMachine.currentState == enemy.HitReaction && enemy.HitReaction is HitReactionState hitReactionState)
        {
            hitReactionState.OnForwardKnockbackFinished();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnTorsoKnockbackFinished(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnForwardKnockdownFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnForwardKnockdownFinished(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it handles knockdowns
        if (enemy.stateMachine.currentState == enemy.HitReaction && enemy.HitReaction is HitReactionState hitReactionState)
        {
            hitReactionState.OnForwardKnockdownFinished();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnForwardKnockdownFinished(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnFaceUpGetUp()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnFaceUpGetUp(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        if (enemy.stateMachine.currentState == enemy.HitReaction && enemy.HitReaction is HitReactionState hitReactionState)
        {
            hitReactionState.OnFaceUpGetUp();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnFaceUpGetUp(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnLegKnockdownFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnLegKnockdownFinished(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it handles leg knockdowns
        if (enemy.stateMachine.currentState == enemy.HitReaction && enemy.HitReaction is HitReactionState hitReactionState)
        {
            hitReactionState.OnLegKnockdownFinished();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnLegKnockdownFinished(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnBackKnockbackFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnBackKnockbackFinished(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it handles back knockbacks
        if (enemy.stateMachine.currentState == enemy.HitReaction && enemy.HitReaction is HitReactionState hitReactionState)
        {
            hitReactionState.OnBackKnockbackFinished();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnBackKnockbackFinished(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnBackKnockdownFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnBackKnockdownFinished(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it handles back knockdowns
        if (enemy.stateMachine.currentState == enemy.HitReaction && enemy.HitReaction is HitReactionState hitReactionState)
        {
            hitReactionState.OnBackKnockdownFinished();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnBackKnockdownFinished(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnFaceDownGetUp()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnFaceDownGetUp(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it handles face down get up
        if (enemy.stateMachine.currentState == enemy.HitReaction && enemy.HitReaction is HitReactionState hitReactionState)
        {
            hitReactionState.OnFaceDownGetUp();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnFaceDownGetUp(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnAttackImpactFrame()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnAttackImpactFrame(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it handles attack impacts
        if (enemy.stateMachine.currentState == enemy.Attack && enemy.Attack is AttackState attackState)
        {
            attackState.OnAttackImpactFrame();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnAttackImpactFrame(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    // ===============================================
    // END ANIMATION EVENTS
    // ===============================================
}
