using Unity.VisualScripting;
using UnityEngine;

public class HitReactionState : EnemyState
{
    public Limb HitLimb { get; private set; }
    public int hitCount { get; private set; } = 0;

    public bool ShouldTriggerKnockback = false;
    public bool IsTorsoKnockbackFinished = false;

    public bool ShouldTriggerKnockdown = false;
    public bool IsKnockdownFinished = false;

    public bool ShouldTriggerLegKnockdown = false;
    public bool IsLegKnockdownFinished = false;

    public bool IsGetUpFinished = false; // Indicates if the get-up action is complete

    public HitReactionState(
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
        enemy.SetAndLogSpeed(0f, "HitReactionState.Enter(): Speed set to 0 during hit reaction");
        enemy.SetIsAggroed(true);
        enemy.SetHasAggroed(true);
        animationManager.SetIsAggro(true);
        animationManager.SetHasAggroed(true);
    }

    public override void Exit(EnemyState nextState)
    {
        Debug.Log($"[{enemy.name}] HitReactionState.Exit(): Exiting to {nextState?.GetType().Name}");
        base.Exit(nextState);
        hitCount = 0;
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

        // If leg knockdown animation finished, transition to Chase
        if (IsLegKnockdownFinished && !animationManager.IsHitReactionPlaying())
        {
            Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Leg knockdown finished, transitioning to Chase");
            IsLegKnockdownFinished = false;
            ShouldTriggerLegKnockdown = false;
            enemy.stateMachine.SetState(enemy.Chase);
            return;
        }

        if (ShouldTriggerKnockdown)
        {
            Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Triggering knockdown animation");
            animationManager.TriggerKnockdown();
            enemy.StartCoroutine(LerpZombieBackwards(2f, 0.5f)); // TODO: add to zombie template
            ShouldTriggerKnockdown = false;
        }

        if (IsKnockdownFinished && !animationManager.IsHitReactionPlaying())
        {
            Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Knockdown finished, transitioning to Chase");
            IsKnockdownFinished = false;
            enemy.stateMachine.SetState(enemy.Chase);
            return;
        }

        if (ShouldTriggerKnockback)
        {
            Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Triggering knockback animation");
            animationManager.TriggerKnockback();
            enemy.StartCoroutine(LerpZombieBackwards(1f, 0.2f)); // TODO: add to zombie template
            ShouldTriggerKnockback = false;
            return;
        }

        if (IsTorsoKnockbackFinished && !animationManager.IsHitReactionPlaying())
        {
            Debug.Log($"[{enemy.name}] HitReactionState.UpdateHitReactionState(): Torso knockback finished, transitioning to Chase");
            IsTorsoKnockbackFinished = false;
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

    public void OnHit(Limb limb)
    {
        if (enemy.stateMachine.currentState == enemy.Death)
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Enemy is dead - no hit reaction needed");
            return;
        }

        HitLimb = limb;
        hitCount++;
        Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Handling hit. Hit limb: {limb}, Hit count: {hitCount}, current state: {enemy.stateMachine.currentState.GetType().Name}.");

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

        // 2. Handle leg hits (trigger animation, wait for animation event to finish)
        // Only trigger if not already queued or playing
        if (IsLeg(HitLimb) && !ShouldTriggerLegKnockdown && !animationManager.IsHitReactionPlaying())
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Hit limb is a leg ({HitLimb.LimbType}) - setting ShouldTriggerLegKnockdown to true.");
            ShouldTriggerLegKnockdown = true;
            stateMachine.SetState(enemy.HitReaction);
            return;
        }

        // 3. Handle vital point escalation
        if (IsVitalPoint(HitLimb) && hitCount == 1)
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Hit limb is a vital point ({HitLimb.LimbType}) - but is first hit.");
            if (!enemy.HasAggroed)
            {
                Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Setting state to Aggro (HasAggroed=false, first vital hit)");
                enemy.stateMachine.SetState(enemy.Aggro);
            }
            return;
        }
        else if (IsVitalPoint(HitLimb) && hitCount == 2 && !animationManager.IsHitReactionPlaying())
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Hit limb is a vital point ({HitLimb.LimbType}) - setting ShouldTriggerKnockback to true.");
            ShouldTriggerKnockback = true;
            stateMachine.SetState(enemy.HitReaction);
            // Wait for IsKnockdownFinished flag to be set by animation event
            return;
        }
        else if (IsVitalPoint(HitLimb) && hitCount == 3 && animationManager.IsAnimationPlaying("Forward Knockback", 0))
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnHit(): Hit limb is a vital point ({HitLimb.LimbType}) - setting ShouldTriggerKnockdown to true.");
            ShouldTriggerKnockdown = true;
            // Wait for IsKnockdownFinished flag to be set by animation event
            return;
        }
    }

    public void OnTorsoKnockbackFinished()
    {
        Debug.Log($"[{enemy.name}] HitReactionState.OnTorsoKnockbackFinished(): Hit reaction finished. Hitcount: {hitCount}, current state: {enemy.stateMachine.currentState.GetType().Name}.");
        IsTorsoKnockbackFinished = true;
    }

    public void OnKnockdownFinished()
    {
        Debug.Log($"[{enemy.name}] HitReactionState.OnKnockdownFinished()");
        // wait for OnGetUp() to be called
    }

    public void OnLegKnockdownFinished()
    {
        Debug.Log($"[{enemy.name}] HitReactionState.OnLegKnockdownFinished()");
        IsLegKnockdownFinished = true;
    }

    public void OnGetUp()
    {
        Debug.Log($"[{enemy.name}] HitReactionState.OnGetUp()");
        IsKnockdownFinished = true;
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
}
