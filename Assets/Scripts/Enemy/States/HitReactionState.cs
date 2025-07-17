using UnityEngine;

public class HitReactionState : EnemyState
{
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
}
