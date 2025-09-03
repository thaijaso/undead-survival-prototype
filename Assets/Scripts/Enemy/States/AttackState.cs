using UndeadSurvivalGame.PlayerSystems;
using UnityEngine;

namespace UndeadSurvivalGame.EnemySystems
{
    public class AttackState : EnemyState
    {
        public AttackState(
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
        {}

        public override void Enter()
        {
            base.Enter();
            Debug.Log($"[{enemy.name}] AttackState.Enter(): Entering Attack state - attack animation should start");

            // Enable LookAtIK for player tracking during attack
            enemy.LookAtIK.enabled = true;
        }

        public override void Exit(EnemyState nextState)
        {
            base.Exit(nextState);

            // Clean up attack animation states
            animationManager.SetIsAttacking(false);
            animationManager.SetIsInAttackRange(false);
            Debug.Log($"[{enemy.name}] AttackState.Exit(): Cleaned up attack animations");
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            // Skip automatic transitions if debug mode is enabled
            if (enemy.DebugModeEnabled)
            {
                // Still set animation states appropriately
                animationManager.SetIsAttacking(true);
                animationManager.SetIsInAttackRange(true);
                return;
            }

            animationManager.SetIsAttacking(true); // Set attacking state in animation manager
            animationManager.SetIsInAttackRange(true); // Ensure attack range is set in animation manager
        }

        public void OnAttackFinished()
        {
            Debug.Log($"[{enemy.name}] AttackState.OnAttackFinished(): Attack animation finished");

            // Don't auto-transition if debug mode is enabled
            if (enemy.DebugModeEnabled)
            {
                Debug.Log($"[{enemy.name}] AttackState.OnAttackFinished(): Debug mode enabled - not auto-transitioning");
                return;
            }

            if (enemy.IsPlayerInAttackRange())
            {
                Debug.Log($"[{enemy.name}] AttackState.OnAttackFinished(): Player still in attack range - staying in Attack state");
                // Player is still in attack range, stay in Attack state
                animationManager.SetIsAttacking(true);
                animationManager.SetIsInAttackRange(true);
            }
            else
            {
                Debug.Log($"[{enemy.name}] AttackState.OnAttackFinished(): Player out of attack range - transitioning to Chase state");
                // Player is out of attack range, transition to Chase state
                animationManager.SetIsAttacking(false); // Reset attacking state in animation manager
                animationManager.SetIsInAttackRange(false); // Reset attack range in animation manager
                stateMachine.SetState(enemy.Chase);
            }
        }

        public void OnAttackLostMomentum()
        {
            Debug.Log($"[{enemy.name}] AttackState.OnAttackLostMomentum(): Attack lost momentum - transitioning to Aggro state");
            enemy.SetAndLogSpeed(0, "AttackState.OnAttackLostMomentum()", 0f);
        }

        public void OnAttackImpactFrame()
        {
            Debug.Log($"[{enemy.name}] AttackState.OnAttackImpact(): Attack impact frame reached - checking player hit");

            // Check if the player is hit
            if (enemy.IsPlayerInAttackRange())
            {
                Debug.Log($"[{enemy.name}] AttackState.OnAttackImpact(): Player is in attack range - dealing damage");
                Player player = enemy.PlayerTransform.GetComponent<Player>();
                var hitReaction = player.hitReaction as PlayerSystems.HitReactionState;
                if (hitReaction != null
                    && player.stateMachine.currentState is not PlayerSystems.HitReactionState
                    && player.stateMachine.currentState is not PlayerSystems.DeathState)
                {
                    // Trigger hit reaction and process damage
                    Debug.Log($"[{enemy.name}] AttackState.OnAttackImpact(): Player hit reaction should be triggered");
                    player.ProcessHit(enemy.enemyTemplate.damage);
                    player.stateMachine.SetState(player.hitReaction);
                }
            }
            else
            {
                Debug.Log($"[{enemy.name}] AttackState.OnAttackImpact(): Player is out of attack range - no damage dealt");
            }
        }
    }
}
