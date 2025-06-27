using UnityEngine;

public class AggroState : EnemyState
{
    public AggroState(
        Enemy enemy,
        StateMachine<EnemyState> stateMachine,
        AnimationManager animationManager,
        string animationName,
        Transform player
    ) : base(
        enemy,
        stateMachine,
        animationManager,
        animationName)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        animationManager.SetIsAggro(true); // Set aggro state in animation manager
    }
}
