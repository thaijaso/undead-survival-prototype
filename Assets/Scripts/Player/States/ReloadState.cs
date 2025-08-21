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
            animationManager.SetIsReloading(true);
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
             // Logic to handle when the chamber is loaded, e.g. increment ammo
            Debug.Log($"[{player.name}] ReloadState.OnChamberLoaded(): Chamber loaded");

            if (weaponManager.CurrentWeaponScript != null)
            {
                weaponManager.IncrementLoadedAmmoCount();
                player.PlayerInventory.DecrementAmmo(weaponManager.CurrentWeaponConfig.ammoType);

                int totalAmmo = player.PlayerInventory.GetAmmoTypeQuantity(weaponManager.CurrentWeaponConfig.ammoType);
                Debug.Log($"[{player.name}] Ammo loaded after reload: {weaponManager.CurrentWeaponScript.currentLoadedAmmo}, totalAmmo: {totalAmmo}");

                if (weaponManager.CurrentWeaponScript.currentLoadedAmmo >= weaponManager.CurrentWeaponConfig.maxAmmo || totalAmmo == 0)
                {
                    animationManager.SetIsReloading(false);
                    Debug.Log($"[{player.name}] Reload complete, switching to idle state.");
                    stateMachine.SetState(player.idle);
                }
            }
            else
            {
                Debug.LogWarning($"[{player.name}] No current weapon script found during reload.");
            }
        }
    }
}
