using UnityEngine;

public class GetUpState : EnemyState
{
    public GetUpState(
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

    public void OnGetUp()
    {
        Debug.Log($"[{enemy.name}] GetUpState.OnGetUp(): Enemy got up");
        enemy.stateMachine.SetState(enemy.Chase);
    }
}

