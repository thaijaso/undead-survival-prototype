using System.Collections;
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
            //player.OverlayController.PlayBloodEffect();
            animationManager.TriggerKnockback();
            player.StartCoroutine(LerpPlayerBack(.8f, .8f));
        }

        public override void Exit(PlayerState nextState)
        {
            base.Exit(nextState);
        }

        public void OnHandCollided()
        {
            Debug.Log($"[{player.name}] HitReactionState.OnHandCollided(): Player has been hit.");

            if (player.stateMachine.currentState is HitReactionState)
            {
                Debug.Log($"[{player.name}] HitReactionState.OnHandCollided(): Already in Hit Reaction state, ignoring.");
                return;
            }
        }

        // Called by animation event when knockback animation finishes 
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

        private IEnumerator LerpPlayerBack(float backwardDistance, float duration = 0.2f)
        {
            Debug.Log($"[{player.name}] HitReactionState.LerpPlayerBack(): Starting to lerp player back.");
            Vector3 startPosition = player.transform.position;
            Vector3 targetPosition = startPosition - player.transform.forward * backwardDistance;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                player.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            player.transform.position = targetPosition; // Ensure final position is set
        }
    }
}
