using UnityEngine;

public class SprintState : MoveState
{
    private float sprintSpeed = 5.0f; // Speed of the player movement when sprinting

    public SprintState(
        Player player,
        StateMachine<PlayerState> stateMachine,
        AnimationManager animationManager,
        string animationName
    ) : base(
        player,
        stateMachine,
        animationManager,
        animationName
    )
    {
        sprintSpeed = player.PlayerCharacterController.sprintSpeed;
    }

    public override void Enter()
    {
        Debug.Log($"[{player.name}] SprintState.Enter(): Entering Sprint state");
        base.Enter();
        // Set the sprinting animation
        animationManager.SetIsSprinting(true);
    }

    public override void Exit(PlayerState nextState)
    {
        Debug.Log($"[{player.name}] SprintState.Exit(): Exiting to {nextState.GetType().Name}");
        base.Exit(nextState);
        // Reset the sprinting animation
        animationManager.SetIsSprinting(false);
        animationManager.SetMoveParams(0f, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    
        if (!player.PlayerInput.IsMoving)
        {
            animationManager.SetIsSprinting(false);
            stateMachine.SetState(player.idle);
            return;
        }

        if (player.PlayerInput.IsAiming)
        {
            animationManager.SetIsSprinting(false);
            stateMachine.SetState(player.aim);
            return;
        }
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        HandleMovement(sprintSpeed, true);
    }
}
