using UnityEngine;

public class StrafeState : MoveState
{
    private float strafeSpeed = 2.0f;

    public StrafeState(
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
        strafeSpeed = player.PlayerCharacterController.strafeSpeed;
    }

    public override void Enter()
    {
        Debug.Log($"[{player.name}] StrafeState.Enter(): Entering Strafe state");
        animationManager.SetIsStrafing(true);
    }

    public override void Exit(PlayerState nextState)
    {
        Debug.Log($"[{player.name}] StrafeState.Exit(): Exiting to {nextState.GetType().Name}");
        base.Exit(nextState);
        animationManager.SetIsStrafing(false);
    }

    public override void LogicUpdate()
    {
        // Guard: Only update if this is the current state
        if (stateMachine.currentState != this)
        {
            return;
        }

        base.LogicUpdate();

        if (!player.PlayerInput.IsMoving && !player.PlayerInput.IsAiming)
        {
            stateMachine.SetState(player.idle);
            return;
        }

        if (player.PlayerInput.IsSprinting && player.PlayerInput.IsMoving && !player.PlayerInput.IsAiming)
        {
            stateMachine.SetState(player.sprint);
            return;
        }

        if (player.PlayerInput.IsAiming && stateMachine.currentState != player.aim && stateMachine.currentState != player.shoot)
        {
            stateMachine.SetState(player.aim);
            return;
        }
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        HandleMovement(strafeSpeed, false);
    }
}



