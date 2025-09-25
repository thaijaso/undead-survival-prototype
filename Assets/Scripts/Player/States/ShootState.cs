using RootMotion.Dynamics;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.Effects;
using UndeadSurvivalGame.EnemySystems;
using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{
    public class ShootState : AimState
    {
        private Camera mainCamera;
        private readonly RecoilIK recoilIK;
        private readonly BulletHitscan bulletHitscan;

        private readonly BulletDecalManager bulletDecalManager;

        private float animationRecoilMagnitude = .5f;
        private float bulletSpreadHorizontal = .5f;
        private float bulletSpreadVertical = .5f;

        public ShootState(
            Player player,
            StateMachine<PlayerState> stateMachine,
            AnimationManager animationManager,
            string animationName,
            PlayerWeaponManager weaponManager,
            RecoilIK recoilIK,
            BulletHitscan bulletHitscan,
            BulletDecalManager bulletDecalManager
        ) : base(
            player,
            stateMachine,
            animationManager,
            animationName,
            weaponManager
        )
        {
            SetupCamera();

            this.recoilIK = recoilIK;
            this.bulletHitscan = bulletHitscan;
            this.bulletDecalManager = bulletDecalManager;
        }

        private void SetupCamera()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError($"[{player.name}] ShootState.SetupCamera(): Main camera not found. Ensure a camera with the 'MainCamera' tag is present in the scene.");
                }
            }
        }

        public override void Enter()
        {
            Debug.Log($"[{player.name}] ShootState.Enter(): Entering Shoot state");
            SetupShootState();
            SetupWeaponDataForShooting();
            SetupWeaponDataForCameraRecoil();
            player.PlayerIKController.DisableIK();
        }

        private void SetupShootState()
        {
            animationManager.SetIsShooting(true);
        }

        private void SetupWeaponDataForShooting()
        {
            animationRecoilMagnitude = weaponManager.CurrentWeaponConfig.animationRecoilMagnitude;
            bulletSpreadHorizontal = weaponManager.CurrentWeaponConfig.bulletSpreadHorizontal;
            bulletSpreadVertical = weaponManager.CurrentWeaponConfig.bulletSpreadVertical;
            Debug.Log($"[{player.name}] ShootState.SetupWeaponDataForShooting(): Animation recoil magnitude: {animationRecoilMagnitude}, Bullet spread: {bulletSpreadHorizontal}/{bulletSpreadVertical}");
        }

        private void SetupWeaponDataForCameraRecoil()
        {
            player.PlayerCameraController.SetCameraRecoilFromWeaponData(
                weaponManager.CurrentWeaponConfig.cameraRecoilX,
                weaponManager.CurrentWeaponConfig.cameraRecoilY,
                weaponManager.CurrentWeaponConfig.cameraRecoilZ,
                weaponManager.CurrentWeaponConfig.cameraRecoilSnapiness,
                weaponManager.CurrentWeaponConfig.cameraRecoilReturnSpeed
            );
        }

        public override void Exit(PlayerState nextState)
        {
            Debug.Log($"[{player.name}] ShootState.Exit(): Exiting to {nextState.GetType().Name}");

            base.Exit(nextState);
            animationManager.SetIsShooting(false);

            if (weaponManager.CurrentWeaponConfig.isAutomatic)
            {
                weaponManager.StopMuzzleEffect();
            }
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            if (!player.PlayerInput.IsAttacking && player.PlayerInput.IsAiming)
            {
                stateMachine.SetState(player.aim);
                return;
            }

            bool isAutomatic = weaponManager.CurrentWeaponConfig.isAutomatic;

            if (isAutomatic)
            {
                // Automatic: fire while held
                if (player.PlayerInput.IsAttacking && weaponManager.IsFireCooldownComplete())
                {
                    Debug.Log("[ShootState] Automatic fire triggered");
                    Shoot();
                }
            }
            else
            {
                // Semi-auto: fire only if timer is ready, ignore rapid clicks
                if (player.PlayerInput.AttackBuffered)
                {
                    if (weaponManager.IsFireCooldownComplete())
                    {
                        if (weaponManager.IsChamberEmpty())
                        {
                            Debug.Log("[ShootState] Cannot fire: chamber is empty");
                            weaponManager.PlayEmptyGunClick();
                        }
                        else
                        {
                            Debug.Log("[ShootState] Semi-auto fire triggered (buffered)");
                            Shoot();
                        }
                    }
                    // Always consume buffer, even if timer not ready
                    player.PlayerInput.ConsumeAttackBuffer();
                    // After firing or consuming buffer, return to aim state
                    stateMachine.SetState(player.aim);
                    return;
                }
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        private void Shoot()
        {
            Debug.Log($"[{player.name}] ShootState.Shoot(): Firing weapon");
            animationManager.TriggerPistolShootPowerful();
            ApplyAnimationRecoil();
            ApplyCameraRecoil();
            FireRigidbodyBullet();

            player.CrosshairController.SetCrosshair(
                1f,
                bulletSpreadHorizontal,
                bulletSpreadVertical,
                0.1f
            );

            PlayWeaponEffects();
            weaponManager.ResetFireTimer();
            weaponManager.DecrementLoadedAmmoCount();
            //weaponManager.UpdateCurrentLoadedAmmoUI();
        }

        // Apply visual recoil to weapon and animation:
        private void ApplyAnimationRecoil()
        {
            recoilIK.Fire(animationRecoilMagnitude);
        }

        private void ApplyCameraRecoil()
        {
            player.PlayerCameraController.ApplyCameraRecoil();
        }

        private Vector3 GetShotDirection()
        {
            Vector3 direction = mainCamera.transform.forward;

            if (player.CrosshairController.IsCrosshairExpanded())
            {
                Vector3 recoilOffsets = GetRecoilEulerOffsets();
                direction = Quaternion.Euler(recoilOffsets) * direction;
            }

            return direction;
        }

        private Vector3 GetRecoilEulerOffsets()
        {
            float pitch = Random.Range(-bulletSpreadVertical, bulletSpreadVertical);
            float yaw = Random.Range(-bulletSpreadHorizontal, bulletSpreadHorizontal);
            return new Vector3(pitch, yaw, 0f);
        }

        private bool TryFireHitscanBullet(Vector3 origin, Vector3 direction, float range, out RaycastHit hit)
        {
            return bulletHitscan.Fire(origin, direction, range, out hit);
        }

        private void FireRigidbodyBullet()
        {
            Weapon weapon = weaponManager.CurrentWeaponGameObject.GetComponent<Weapon>();
            weapon.Fire();
        }

        private void HandleBulletImpact(RaycastHit hit, Vector3 direction)
        {
            HandleEnemyHitboxImpact(hit);
            SpawnBulletDecal(hit, direction);
        }

        private void HandleEnemyHitboxImpact(RaycastHit hit)
        {
            // Check if the hit object is on the Enemy layer
            if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Hitbox"))
                return;

            // Try to get the Enemy and BodyPart components
            PuppetMaster puppetMaster = hit.collider.GetComponentInParent<PuppetMaster>();

            if (puppetMaster == null)
            {
                Debug.LogWarning($"[{player.name}] ShootState.HandleEnemyHitboxImpact(): PuppetMaster component not found in parent hierarchy.");
                return;
            }

            Enemy enemy = puppetMaster.targetRoot.GetComponent<Enemy>();
            Limb limb = hit.collider.gameObject.GetComponent<Limb>();

            if (enemy == null)
            {
                Debug.LogWarning($"[{player.name}] ShootState.HandleEnemyHitboxImpact(): Enemy component not found in parent hierarchy.");
                return;
            }

            if (limb == null)
            {
                Debug.LogWarning($"[{player.name}] ShootState.HandleEnemyHitboxImpact(): Limb component not found in parent hierarchy.");
            }

            // Do damage to the enemy
            enemy.ProcessHit(weaponManager.CurrentWeaponConfig.damage, limb);

            // Spawn blood effect regardless of body part presence
            SpawnBloodEffect(hit, enemy);

            // Only force aggro transition if the enemy is not in debug mode
            if (!enemy.DebugModeEnabled)
            {
                enemy.stateMachine.SetState(enemy.Aggro);
                Debug.Log($"[{player.name}] ShootState.HandleEnemyHitboxImpact(): Forced enemy to Aggro state");
            }
            else
            {
                Debug.Log($"[{player.name}] ShootState.HandleEnemyHitboxImpact(): Enemy in debug mode - not forcing Aggro transition");
            }
        }

        private void SpawnBloodEffect(RaycastHit hit, Enemy enemy)
        {
            GameObject bloodEffect = Object.Instantiate(
                enemy.enemyTemplate.bloodEffectPrefab,
                hit.point,
                Quaternion.LookRotation(hit.normal)
            );
            Object.Destroy(bloodEffect, 2f);
        }

        private void SpawnBulletDecal(RaycastHit hit, Vector3 direction)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                bulletDecalManager.SpawnBulletDecal(hit);
            }
        }

        private void PlayWeaponEffects()
        {
            weaponManager.PlayMuzzleEffect();
            weaponManager.PlayGunshotSound();
        }
    }
}
