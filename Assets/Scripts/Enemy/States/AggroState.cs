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
        Debug.Log("AggroState: Entering Aggro state - yell animation should start");
        animationManager.SetIsAggro(true); // Set aggro state in animation manager
    }

    public void OnYellFinished()
    {
        Debug.Log("AggroState: Yell animation finished - enabling AI movement");
        enemy.AIDestinationSetter.enabled = true;
    }
}
