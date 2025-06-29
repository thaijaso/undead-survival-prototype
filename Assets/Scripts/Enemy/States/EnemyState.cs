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
}