using UnityEngine;
using RootMotion.Dynamics; // Add this if PuppetMaster is from RootMotion.Dynamics

namespace UndeadSurvivalGame.Player.States
{
    public class DeathState : PlayerState
    {
        public DeathState(
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
            Debug.Log($"[{player.name}] DeathState.Enter(): Entering Death state.");
            base.Enter();
            player.PuppetMaster.state = PuppetMaster.State.Dead;
        }

        public override void Exit(PlayerState nextState)
        {
            Debug.Log($"[{player.name}] DeathState.Exit(): Exiting to {nextState.GetType().Name}.");
        }
    }
}

