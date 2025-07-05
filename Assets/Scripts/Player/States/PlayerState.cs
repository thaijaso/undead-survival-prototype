public class PlayerState : IState<PlayerState>
{
    protected Player player;
    protected StateMachine<PlayerState> stateMachine;
    protected AnimationManager animationManager;
    protected string animationName;

    /// <summary>
    /// Returns true if debug aim lock is active (prevents automatic state transitions for offset editing)
    /// </summary>
    protected virtual bool IsDebugAimLockActive => PlayerDebugger.ForceAimDebugMode;

    public PlayerState(
        Player player,
        StateMachine<PlayerState> stateMachine,
        AnimationManager animationManager,
        string animationName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animationManager = animationManager;
        this.animationName = animationName;
    }

    public virtual void Enter()
    {
        // Logic to be implemented in derived classes
    }

    public virtual void Exit(PlayerState nextState)
    {
        animationManager.StopAnimation();
    }

    public virtual void LogicUpdate()
    {
        // Logic to be implemented in derived classes
        if (stateMachine.currentState != player.aim && stateMachine.currentState != player.shoot)
        {
            player.PlayerCameraController.ZoomOut();
        }
    }

    public virtual void PhysicsUpdate()
    {
        // Physics logic to be implemented in derived classes
    }

    public virtual void LateUpdate()
    {
        // Late update logic to be implemented in derived classes
    }
}

