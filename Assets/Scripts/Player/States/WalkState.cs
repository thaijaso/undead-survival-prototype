using UndeadSurvivalGame.PlayerSystems;
using Unity.VisualScripting;

public class WalkState : MoveState
{
    public WalkState(
        Player player,
        StateMachine<PlayerState> stateMachine,
        AnimationManager animationManager,
        string animationName,
        PlayerWeaponManager weaponManager
    ) : base(
        player,
        stateMachine,
        animationManager,
        animationName,
        weaponManager
    )
    { }

    public override void Enter()
    {
        base.Enter();
        animationManager.SetIsWalking(true);
    }

    public override void Exit(PlayerState nextState)
    {
        base.Exit(nextState);
        animationManager.SetIsWalking(false);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        HandleMovement(player.PlayerCharacterController.strafeSpeed, true);

        if (!player.PlayerInput.IsMoving)
        {
            stateMachine.SetState(player.idle);
            return;
        }

        if (player.PlayerInput.IsSprinting)
        {
            stateMachine.SetState(player.sprint);
            return;
        }
    }
}
