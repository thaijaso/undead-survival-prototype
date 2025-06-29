using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class EnemyState : IState<EnemyState>
{
    protected Enemy enemy;

    protected StateMachine<EnemyState> stateMachine;

    protected AnimationManager animationManager;

    protected string animationName;

    private float detectionRange;

    protected List<Transform> patrolPoints;

    public AIDestinationSetter destinationSetter { get; private set; }

    public FollowerEntity followerEntity { get; private set; }

    public EnemyState(
        Enemy enemy,
        StateMachine<EnemyState> stateMachine,
        AnimationManager animationManager,
        string animationName)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
        this.animationManager = animationManager;
        this.animationName = animationName;

        detectionRange = enemy.GetAlertRange();
        patrolPoints = enemy.GetPatrolPoints();

        SetupDestinationSetter();
        SetupFollowerEntity();
    }

    private void SetupDestinationSetter()
    {
        destinationSetter = enemy.GetComponent<AIDestinationSetter>();
        if (destinationSetter == null)
        {
            Debug.LogError("AIDestinationSetter component is missing on the Enemy GameObject.");
        }
    }

    private void SetupFollowerEntity()
    {
        followerEntity = enemy.GetComponent<FollowerEntity>();
        if (followerEntity == null)
        {
            Debug.LogError("FollowerEntity component is missing on the Enemy GameObject.");
        }
    }

    public virtual void Enter() { }
    public virtual void Exit(EnemyState nextState) { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void LateUpdate() { }

    protected bool IsPlayerInDetectionRange()
    {
        if (enemy.GetPlayerTransform() == null)
        {
            Debug.LogWarning("PlayerTransform is not set on the enemy.");
            return false;
        }

        float sqrDistance = (enemy.GetPlayerTransform().position - enemy.transform.position).sqrMagnitude;
        return sqrDistance <= detectionRange * detectionRange;
    }
    
    protected float GetAngleToPlayer()
    {
        Vector3 directionToPlayer = enemy.GetPlayerTransform().position - enemy.transform.position;
        return Vector3.SignedAngle(enemy.transform.forward, directionToPlayer, Vector3.up);
    }

    // Shared rotation methods
    protected IEnumerator RotateWithAnimation(Quaternion targetRotation)
    {
        // Two-phase rotation for AlertState (default behavior)
        return RotateWithTwoPhaseAnimation(targetRotation);
    }

    protected IEnumerator RotateWithTwoPhaseAnimation(Quaternion targetRotation)
    {
        Quaternion startRotation = enemy.transform.rotation;
        
        // Phase 1: Slow rotation (first part of animation)
        float slowPhaseDuration = enemy.template.turn180Phase1Duration;
        float slowPhaseProgress = 0.2f; // How much to rotate during slow phase
        
        float elapsedTime = 0f;
        while (elapsedTime < slowPhaseDuration)
        {
            float progress = (elapsedTime / slowPhaseDuration) * slowPhaseProgress;
            enemy.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Phase 2: Fast rotation (second part of animation)
        float fastPhaseDuration = enemy.template.turn180Phase2Duration;
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
    }

    protected IEnumerator RotateWithAggroAnimation(Quaternion targetRotation)
    {
        Quaternion startRotation = enemy.transform.rotation;
        
        // Phase 1: Slow rotation (first part of Aggro180 animation)
        float slowPhaseDuration = enemy.template.aggro180Phase1Duration;
        float slowPhaseProgress = 0.5f; // How much to rotate during slow phase
        
        float elapsedTime = 0f;
        while (elapsedTime < slowPhaseDuration)
        {
            float progress = (elapsedTime / slowPhaseDuration) * slowPhaseProgress;
            enemy.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Phase 2: Fast rotation (second part of Aggro180 animation)
        float fastPhaseDuration = enemy.template.aggro180Phase2Duration;
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
    }

    protected IEnumerator RotateWithSinglePhaseAnimation(Quaternion targetRotation, float duration)
    {
        Quaternion startRotation = enemy.transform.rotation;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            
            // Use a smooth curve for the rotation
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            enemy.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothProgress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end up exactly at target rotation
        enemy.transform.rotation = targetRotation;
    }
}