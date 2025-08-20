using UnityEngine;

namespace UndeadSurvivalGame.Player.States
{
    public class ReloadState : StrafeState
    {
        public ReloadState(
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
            Debug.Log($"[{player.name}] ReloadState.Enter(): Entering Reload state");
            animationManager.TriggerRevolverReloadAnimation();
            player.PlayerInput.ConsumeAimBuffer(); // Consume any buffered aim input on entering reload
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            if (player.PlayerInput.AimBuffered)
            {
                stateMachine.SetState(player.aim);
                return;
            }
        }

        public void OnChamberLoaded()
        {
            Debug.Log($"[{player.name}] ReloadState.OnChamberLoaded(): Chamber loaded");
            // Logic to handle when the chamber is loaded, e.g. increment ammo
            if (weaponManager.CurrentWeaponScript != null)
            {
                //weaponManager.IncrementAmmoCount();
                //weaponManager.DecrementTotalAmmoCount();
                //weaponManager.UpdateCurrentLoadedAmmoUI();
                //weaponManager.UpdateTotalAmmoUI();
                Debug.Log($"[{player.name}] Ammo after reload: {weaponManager.CurrentWeaponScript.currentLoadedAmmo}");

                if (weaponManager.CurrentWeaponScript.currentLoadedAmmo >= weaponManager.CurrentWeaponConfig.maxAmmo)
                {
                    Debug.Log($"[{player.name}] Reload complete, switching to Aim state.");
                    stateMachine.SetState(player.aim);
                }
            }
            else
            {
                Debug.LogWarning($"[{player.name}] No current weapon script found during reload.");
            }
        }
    }
}
