using UnityEngine;

public class HitReactionState : EnemyState
{
    public Limb HitLimb { get; private set; }

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

        animationManager.TriggerIsHit();
        enemy.SetAndLogSpeed(0f, "HitReactionState.Enter(): Speed set to 0 during hit reaction");
        enemy.SetIsAggroed(true);
        enemy.SetHasAggroed(true);
        animationManager.SetIsAggro(true);
        animationManager.SetHasAggroed(true);

        // Move the zombie backwards a bit
        enemy.StartCoroutine(LerpZombieBackwards());
    }

    public void OnHitReactionFinished()
    {
        Debug.Log($"[{enemy.name}] HitReactionState.OnHitReactionFinished(): Hit reaction finished");
        if (enemy.HasAggroed)
        {
            enemy.stateMachine.SetState(enemy.Chase);
        }
        else
        {
            enemy.stateMachine.SetState(enemy.Aggro);
        }
    }

    public void SetHitLimb(Limb limb)
    {
        HitLimb = limb;
        Debug.Log($"[{enemy.name}] HitReactionState.SetHitLimb(): Hit limb set to {HitLimb}");
    }

    private System.Collections.IEnumerator LerpZombieBackwards()
    {
        float backwardDistance = 0.5f;
        float duration = 0.2f;
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
}
