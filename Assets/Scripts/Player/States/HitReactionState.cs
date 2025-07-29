using UnityEngine;

namespace UndeadSurvivalGame.Player.States
{
    public class HitReactionState : PlayerState
    {
        public HitReactionState(
            Player player,
            StateMachine<PlayerState> stateMachine,
            AnimationManager animationManager,
            string animationName,
            PlayerWeaponManager weaponManager
        ) : base(
            player,
            stateMachine,
            animationManager,
            animationName,
            weaponManager
        )
        { }

        public override void Enter()
        {
            base.Enter();
            Debug.Log($"[{player.name}] HitReactionState.Enter(): Entering Hit Reaction state.");
            animationManager.TriggerKnockback();
        }

        public void OnHit()
        {
            Debug.Log($"[{player.name}] HitReactionState.OnHit(): Player has been hit.");

            if (player.stateMachine.currentState is HitReactionState)
            {
                Debug.Log($"[{player.name}] HitReactionState.OnHit(): Already in Hit Reaction state, ignoring.");
                return;
            }

            player.stateMachine.SetState(player.hitReaction);
        }

        public void OnKnockbackFinished()
        {
            Debug.Log($"[{player.name}] HitReactionState.OnKnockbackFinished(): Knockback animation finished.");

            // Transition back to idle or appropriate state after knockback
            if (player.stateMachine.currentState is HitReactionState)
            {
                player.stateMachine.SetState(player.idle);
            }
            else
            {
                Debug.LogWarning($"[{player.name}] HitReactionState.OnKnockbackFinished(): Not in HitReactionState, cannot transition.");
            }
        }
    }
}
