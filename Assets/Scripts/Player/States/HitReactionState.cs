using UndeadSurvivalGame.Player;
using UnityEngine;

namespace UndeadSurvivalGame.Player
{
    public class HitReactionState : PlayerState
    {
        public HitReactionState(
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
        {
        }
    }
}
