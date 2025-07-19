using Unity.Entities.UniversalDelegates;
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

    public void OnKnockbackFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnTorsoKnockbackFinished(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it handles hit reactions
        if (enemy.stateMachine.currentState == enemy.HitReaction && enemy.HitReaction is HitReactionState hitReactionState)
        {
            hitReactionState.OnTorsoKnockbackFinished();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnTorsoKnockbackFinished(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnKnockdownFinished()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnKnockdownFinished(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        // Delegate to the current state if it handles knockdowns
        if (enemy.stateMachine.currentState == enemy.HitReaction && enemy.HitReaction is HitReactionState hitReactionState)
        {
            hitReactionState.OnKnockdownFinished();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnKnockdownFinished(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    public void OnGetUp()
    {
        Debug.Log($"[{name}] EnemyAnimatorEvents.OnGetUp(): Current state: " + enemy.stateMachine.currentState.GetType().Name);

        if (enemy.stateMachine.currentState == enemy.GetUp && enemy.GetUp is GetUpState getUpState)
        {
            getUpState.OnGetUp();
        }
        else
        {
            Debug.LogWarning($"[{name}] EnemyAnimatorEvents.OnGetUp(): Called but current state ({enemy.stateMachine.currentState.GetType().Name}) doesn't handle it");
        }
    }

    // ===============================================
    // END ANIMATION EVENTS
    // ===============================================
}
