// ===============================
// HitReactionState Guard Flags Summary
//
// | Reaction Type         | Trigger Flag                   | Finished Flag                  | Active Flag         |
// |----------------------|-------------------------------|-------------------------------|---------------------|
// | Forward Knockback     | ShouldTriggerForwardKnockback  | IsForwardKnockbackFinished     |                     |
// | Forward Knockdown     | ShouldTriggerForwardKnockdown  | IsForwardKnockdownFinished     | IsKnockdownActive   |
// | Back Knockback        | ShouldTriggerBackKnockback     | IsBackKnockbackFinished        |                     |
// | Back Knockdown        | ShouldTriggerBackKnockdown     | IsBackKnockdownFinished        | IsKnockdownActive   |
// | Leg Knockdown         | ShouldTriggerLegKnockdown      | IsLegKnockdownFinished         |                     |
// ===============================

using UnityEngine;

namespace UndeadSurvivalGame.Enemy
{
    public class HitReactionState : EnemyState
    {
        public Limb HitLimb { get; private set; }

        public int hitCount { get; private set; } = 0;

        private bool ShouldTriggerForwardKnockback = false;
        private bool IsForwardKnockbackFinished = false;

        private bool ShouldTriggerForwardKnockdown = false;
        private bool IsForwardKnockdownFinished = false;

        private bool IsKnockdownActive = false;

        private bool ShouldTriggerBackKnockback = false;
        private bool IsBackKnockbackFinished = false;

        private bool ShouldTriggerBackKnockdown = false;
        private bool IsBackKnockdownFinished = false;

        private bool ShouldTriggerLegKnockdown = false;
        private bool IsLegKnockdownFinished = false;

        private bool IsReactionInProgress = false;

        public HitReactionState(
            UndeadSurvivalGame.Enemy.Enemy enemy,
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
            enemy.SetAndLogSpeed(0f, "HitReactionState.Enter(): Speed set to 0 during hit reaction");
            enemy.SetIsAggroed(true);
            enemy.SetHasAggroed(true);
            animationManager.SetIsAggro(true);
            animationManager.SetHasAggroed(true);
            IsReactionInProgress = true;
        }

        public override void Exit(EnemyState nextState)
        {
            Debug.Log($"[{enemy.name}] HitReactionState.Exit(): Exiting to {nextState?.GetType().Name}");
            base.Exit(nextState);
            hitCount = 0;
            ShouldTriggerForwardKnockback = false;
            IsForwardKnockbackFinished = false;
            ShouldTriggerForwardKnockdown = false;
            IsForwardKnockdownFinished = false;
            IsKnockdownActive = false;
            ShouldTriggerLegKnockdown = false;
            IsLegKnockdownFinished = false;
            ShouldTriggerBackKnockback = false;
            IsBackKnockbackFinished = false;
            ShouldTriggerBackKnockdown = false;
            IsBackKnockdownFinished = false;
            IsReactionInProgress = false;
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            UpdateHitReactionState();
        }

        public void SetHitLimb(Limb limb)
        {
            HitLimb = limb;
            Debug.Log($"[{enemy.name}] HitReactionState.SetHitLimb(): Hit limb set to {HitLimb}");
        }

        public void UpdateHitReactionState()
        {
            if (ShouldTriggerLegKnockdown)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Triggering leg knockdown animation");
                animationManager.TriggerLegKnockdown();
                ShouldTriggerLegKnockdown = false;
                return;
            }

            // If leg knockdown animation finished, transition to Chase (only if no other reaction is in progress)
            if (IsLegKnockdownFinished && !animationManager.IsHitReactionPlaying() && !IsReactionInProgress)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): IsLegKnockdownFinished: {IsLegKnockdownFinished}, transitioning to Chase");
                IsLegKnockdownFinished = false;
                ShouldTriggerLegKnockdown = false;
                enemy.stateMachine.SetState(enemy.Chase);
                return;
            }

            if (ShouldTriggerForwardKnockdown)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Triggering knockdown animation");
                animationManager.TriggerForwardKnockdown();
                IsKnockdownActive = true;
                enemy.StartCoroutine(LerpZombieBackwards(2f, 0.5f)); // TODO: add to zombie template
                ShouldTriggerForwardKnockdown = false;
                return;
            }

            // Only transition to Chase if no other reaction is in progress
            if (IsForwardKnockdownFinished && !IsKnockdownActive && !IsReactionInProgress)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): IsForwardKnockdownFinished: {IsForwardKnockdownFinished}, transitioning to Chase");
                IsForwardKnockdownFinished = false;
                enemy.stateMachine.SetState(enemy.Chase);
                return;
            }

            if (ShouldTriggerForwardKnockback)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Triggering knockback animation");
                animationManager.TriggerForwardKnockback();
                enemy.StartCoroutine(LerpZombieBackwards(1f, 0.2f)); // TODO: add to zombie template
                ShouldTriggerForwardKnockback = false;
                return;
            }

            if (IsForwardKnockbackFinished && !IsKnockdownActive && !animationManager.IsHitReactionPlaying() && !IsReactionInProgress)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): IsForwardKnockbackFinished: {IsForwardKnockbackFinished}, transitioning to Chase");
                IsForwardKnockbackFinished = false;
                enemy.stateMachine.SetState(enemy.Chase);
                return;
            }

            if (ShouldTriggerBackKnockdown)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Triggering back knockdown animation");
                animationManager.TriggerBackKnockdown();
                IsKnockdownActive = true;
                // TODO: Lerp zombie forwards
                ShouldTriggerBackKnockdown = false;
                return;
            }

            if (IsBackKnockdownFinished && !IsKnockdownActive && !IsReactionInProgress)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): IsBackKnockdownFinished: {IsBackKnockdownFinished}, transitioning to Chase");
                IsBackKnockdownFinished = false;
                enemy.stateMachine.SetState(enemy.Chase);
                return;
            }

            if (ShouldTriggerBackKnockback)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Triggering back knockback animation");
                animationManager.TriggerBackKnockback();
                //enemy.StartCoroutine(LerpZombieBackwards(1f, 0.2f)); // TODO: add to zombie template
                ShouldTriggerBackKnockback = false;
                return;
            }

            if (IsBackKnockbackFinished && !IsKnockdownActive && !animationManager.IsHitReactionPlaying() && !IsReactionInProgress)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): IsBackKnockbackFinished: {IsBackKnockbackFinished}, transitioning to Chase");
                IsBackKnockbackFinished = false;
                enemy.stateMachine.SetState(enemy.Chase);
                return;
            }
        }

        private System.Collections.IEnumerator LerpZombieBackwards(float backwardDistance, float duration = 0.2f)
        {
            // TODO: Use weapon template to determine distance and duration
            float elapsed = 0f;
            Vector3 startPos = enemy.transform.position;
            Vector3 endPos = startPos - enemy.transform.forward * backwardDistance;

            while (elapsed < duration)
            {
                enemy.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            enemy.transform.position = endPos;
        }

        public void OnHit(Limb limb, Vector3 bulletDirection)
        {
            if (enemy.stateMachine.currentState == enemy.Death)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Enemy is dead - no hit reaction needed");
                return;
            }


            HitLimb = limb;
            hitCount++;
            Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Handling hit. Hit limb: {limb}, Hit count: {hitCount}, current state: {enemy.stateMachine.currentState.GetType().Name}.");

            // Log all guard/active flags for debugging
            Debug.Log($"[{enemy.name}] HitReactionState.OnHit() FLAGS: " +
                $"ShouldTriggerForwardKnockback={ShouldTriggerForwardKnockback}, " +
                $"IsForwardKnockbackFinished={IsForwardKnockbackFinished}, " +
                $"ShouldTriggerForwardKnockdown={ShouldTriggerForwardKnockdown}, " +
                $"IsForwardKnockdownFinished={IsForwardKnockdownFinished}, " +
                $"ShouldTriggerBackKnockback={ShouldTriggerBackKnockback}, " +
                $"IsBackKnockbackFinished={IsBackKnockbackFinished}, " +
                $"ShouldTriggerBackKnockdown={ShouldTriggerBackKnockdown}, " +
                $"IsBackKnockdownFinished={IsBackKnockdownFinished}, " +
                $"ShouldTriggerLegKnockdown={ShouldTriggerLegKnockdown}, " +
                $"IsLegKnockdownFinished={IsLegKnockdownFinished}, " +
                $"IsKnockdownActive={IsKnockdownActive}, " +
                $"IsReactionInProgress={IsReactionInProgress}, " +
                $"IsHitReactionPlaying={animationManager.IsHitReactionPlaying()}" 
            );

            // 1. Handle arm hits (no reaction, reset count, possible state change)
            if (IsArm(HitLimb))
            {
                hitCount = 0;
                Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Resetting hit count to 0 after arm hit.");

                if (!enemy.HasAggroed)
                {
                    Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Setting state to Aggro (HasAggroed=false)");
                    enemy.stateMachine.SetState(enemy.Aggro);
                }

                return;
            }

            // Log bulletDirection and hitNormal for debugging
            float dot = Vector3.Dot(bulletDirection, enemy.transform.forward);
            Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): bulletDirection = {bulletDirection}, enemyDirection = {enemy.transform.forward}, dot = {dot}");

            // 2. Handle leg hits (trigger animation, wait for animation event to finish)
            // Only trigger if not already queued or playing
            if (IsLeg(HitLimb) && !ShouldTriggerLegKnockdown && !animationManager.IsHitReactionPlaying())
            {
                hitCount = 0;
                Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Hit limb is a leg ({HitLimb.LimbType}) - setting ShouldTriggerLegKnockdown to true.");
                ShouldTriggerLegKnockdown = true;
                stateMachine.SetState(enemy.HitReaction);
                return;
            }

            if (IsVitalPoint(HitLimb) && hitCount == 1 && !animationManager.IsHitReactionPlaying() && dot <= -0.3f)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Hit limb is a vital point ({HitLimb.LimbType}) - setting ShouldTriggerForwardKnockback to true.");
                ShouldTriggerForwardKnockback = true;
                stateMachine.SetState(enemy.HitReaction);
                // Wait for IsForwardKnockbackFinished flag to be set by animation event
                return;
            }
            else if (IsVitalPoint(HitLimb) && hitCount == 2 && animationManager.IsAnimationPlaying("Forward Knockback", 0) && dot <= -0.3f)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Hit limb is a vital point ({HitLimb.LimbType}) - setting ShouldTriggerKnockdown to true.");
                ShouldTriggerForwardKnockdown = true;
                hitCount = 0;
                // Wait for IsForwardKnockdownFinished flag to be set by animation event
                return;
            }
            else if (IsVitalPoint(HitLimb) && hitCount == 1 && !animationManager.IsHitReactionPlaying() && dot >= 0.3f)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Hit limb is a vital point ({HitLimb.LimbType}) - setting ShouldTriggerBackKnockback to true.");
                ShouldTriggerBackKnockback = true;
                stateMachine.SetState(enemy.HitReaction);
                // Wait for IsBackKnockbackFinished flag to be set by animation event
                return;
            }
            else if (IsVitalPoint(HitLimb) && hitCount == 2 && animationManager.IsAnimationPlaying("Backward Knockback", 0) && dot >= 0.3f)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Hit limb is a vital point ({HitLimb.LimbType}) - setting ShouldTriggerBackKnockdown to true.");
                ShouldTriggerBackKnockdown = true;
                hitCount = 0;
                // Wait for IsBackKnockdownFinished flag to be set by OnFaceDownGetUp animation event
                return;
            }
            else
            {
                Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Unhandled hit reaction. Hit limb: {HitLimb.LimbType}, Hit count: {hitCount}, dot: {dot}");
            }
        }

        public void OnForwardKnockbackFinished()
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnForwardKnockbackFinished(): Hit reaction finished. Hitcount: {hitCount}, current state: {enemy.stateMachine.currentState.GetType().Name}.");
            IsForwardKnockbackFinished = true;
            IsReactionInProgress = false;
        }

        public void OnForwardKnockdownFinished()
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnForwardKnockdownFinished()");
            IsKnockdownActive = false;
        }

        public void OnLegKnockdownFinished()
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnLegKnockdownFinished()");
            IsLegKnockdownFinished = true;
            IsReactionInProgress = false;
        }

        public void OnFaceUpGetUp()
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnGetUp()");
            IsForwardKnockdownFinished = true;
            IsReactionInProgress = false;
        }

        public void OnBackKnockbackFinished()
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnBackKnockbackFinished()");
            IsBackKnockbackFinished = true;
            IsReactionInProgress = false;
        }

        public void OnBackKnockdownFinished()
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnBackKnockdownFinished()");
            IsKnockdownActive = false;
        }

        public void OnFaceDownGetUp()
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnFaceDownGetUp()");
            IsBackKnockdownFinished = true;
            IsReactionInProgress = false;
        }

        private bool IsLeg(Limb limb)
        {
            return limb != null &&
                (limb.LimbType == LimbType.UpperLeg || limb.LimbType == LimbType.LowerLeg || limb.LimbType == LimbType.Foot);
        }

        private bool IsVitalPoint(Limb limb)
        {
            return limb != null &&
                (limb.LimbType == LimbType.Torso || limb.LimbType == LimbType.Stomach || limb.LimbType == LimbType.Head);
        }

        private bool IsArm(Limb limb)
        {
            return limb != null &&
                (limb.LimbType == LimbType.UpperArm || limb.LimbType == LimbType.LowerArm || limb.LimbType == LimbType.Hand);
        }
        // End of HitReactionState
    }
}
