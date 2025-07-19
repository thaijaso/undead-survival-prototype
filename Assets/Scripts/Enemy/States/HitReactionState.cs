using UnityEngine;

public class HitReactionState : EnemyState
{
    public Limb HitLimb { get; private set; }
    public int successiveHitCount { get; private set; } = 0;

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
        Debug.Log($"[{enemy.name}] HitReactionState.Enter(): Entered hit reaction state");

        successiveHitCount++;

        if (HitLimb.LimbType == LimbType.Torso || HitLimb.LimbType == LimbType.Stomach || HitLimb.LimbType == LimbType.Head)
        {
            animationManager.TriggerKnockback();
        }

        enemy.SetAndLogSpeed(0f, "HitReactionState.Enter(): Speed set to 0 during hit reaction");
        enemy.SetIsAggroed(true);
        enemy.SetHasAggroed(true);
        animationManager.SetIsAggro(true);
        animationManager.SetHasAggroed(true);

        // Move the zombie backwards a bit
        enemy.StartCoroutine(LerpZombieBackwards(0.5f, 0.2f));
    }

    public override void Exit(EnemyState nextState)
    {
        base.Exit(nextState);
        successiveHitCount = 0;
    }

    public void OnTorsoKnockbackFinished()
    {
        Debug.Log($"[{enemy.name}] HitReactionState.OnTorsoKnockbackFinished(): Hit reaction finished");
        if (successiveHitCount == 1)
        {
            if (enemy.HasAggroed)
            {
                enemy.stateMachine.SetState(enemy.Chase);
            }
            else
            {
                enemy.stateMachine.SetState(enemy.Aggro);
            }
        }
    }

    public void SetHitLimb(Limb limb)
    {
        HitLimb = limb;
        Debug.Log($"[{enemy.name}] HitReactionState.SetHitLimb(): Hit limb set to {HitLimb}");
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

    public void OnSuccessiveHit()
    {
        successiveHitCount++;
        Debug.Log($"[{enemy.name}] HitReactionState.OnSuccessiveHit(): Handling successive hit. Hit count: {successiveHitCount}");

        if (successiveHitCount > 1 &&
            (HitLimb.LimbType == LimbType.Torso ||
             HitLimb.LimbType == LimbType.Stomach ||
             HitLimb.LimbType == LimbType.Head))
        {
            Debug.Log($"[{enemy.name}] HitReactionState.OnSuccessiveHit(): Triggering knockdown");
            animationManager.TriggerKnockdown();
            enemy.StartCoroutine(LerpZombieBackwards(1f, 0.5f));
        }
    }

    public void OnKnockdownFinished()
    {
        Debug.Log($"[{enemy.name}] HitReactionState.OnKnockdownFinished(): Handling knockdown finished");
        enemy.stateMachine.SetState(enemy.GetUp);
    }
}
