using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace EnemyStates
{
    public class PatrolState : EnemyState
    {
        private int currentPatrolIndex;
        private float patrolDistanceThreshold = 0.5f; // Distance to consider as "reached" a patrol point

        public PatrolState(
            Enemy enemy,
            StateMachine<EnemyState> stateMachine,
            AnimationManager animationManager,
            string animationName
        ) : base(
            enemy,
            stateMachine,
            animationManager,
            animationName
        )
        {
        }

        public override void Enter()
        {
            base.Enter();

            // Disable LookAtIK when patrolling (not actively tracking player)
            enemy.LookAtIK.enabled = false;

            // Logic for entering patrol state, e.g., setting patrol path
            animationManager.SetIsMoving(true);
            animationManager.SetMoveParams(0f, .5f);

            SetNextPatrolPoint();

            destinationSetter.enabled = true;
            
            float patrolSpeed = enemy.GetPatrolSpeed();
            enemy.SetAndLogSpeed(patrolSpeed, "PatrolState.Enter()");
        }

        private void SetNextPatrolPoint()
        {
            if (patrolPoints.Count == 0) return;

            // Set random patrol point as the destination
            currentPatrolIndex = Random.Range(0, patrolPoints.Count);
            destinationSetter.target = patrolPoints[currentPatrolIndex];
        }

        public override void Exit(EnemyState nextState)
        {
            base.Exit(nextState);

            // Logic for exiting patrol state, e.g., stopping movement
            animationManager.SetIsMoving(false);
            animationManager.SetMoveParams(0f, 0f);

            destinationSetter.enabled = false;
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            // Logic for patrol state, e.g., moving along a path
            HandlePatrolPathing();
            HandlePlayerDetection();
        }

        private void HandlePatrolPathing()
        {
            if (IsAtPatrolPoint())
            {
                // If reached the current patrol point, set the next one
                SetNextPatrolPoint();
            }
        }

        private bool IsAtPatrolPoint()
        {
            if (destinationSetter.target == null) return false;

            float sqrDistanceToTarget = (enemy.transform.position - destinationSetter.target.position).sqrMagnitude;
            return sqrDistanceToTarget < patrolDistanceThreshold * patrolDistanceThreshold; // Adjust threshold as needed
        }

        private void HandlePlayerDetection()
        {
            // Skip automatic transitions if debug mode is enabled
            if (enemy.DebugModeEnabled)
            {
                return;
            }
            
            // Logic to detect player and transition to chase state if player is within detection range
            if (IsPlayerInDetectionRange())
            {
                // change to chase state
                stateMachine.SetState(enemy.Aggro);
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
            // Physics-related logic for patrol state, e.g., applying movement forces
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            // Cleanup or final adjustments for the patrol state
        }
    }
}
