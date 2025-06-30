using RootMotion.Dynamics;
using UnityEngine;

public class ShootState : AimState
{
    private Camera mainCamera;
    private readonly IKRecoil ikRecoil;
    private readonly BulletHitscan bulletHitscan;

    private readonly BulletDecalManager bulletDecalManager;

    private float animationRecoilMagnitude = .5f;
    private float bulletSpreadHorizontal = .5f;
    private float bulletSpreadVertical = .5f;
    private float fireRate = 0.1f;

    private float fireTimer = 0f;

    public ShootState(
        Player player,
        StateMachine<PlayerState> stateMachine,
        AnimationManager animationManager,
        string animationName,
        PlayerWeaponManager weaponManager,
        IKRecoil ikRecoil,
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

        this.ikRecoil = ikRecoil;
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
                Debug.LogError("Main camera not found. Ensure a camera with the 'MainCamera' tag is present in the scene.");
            }
        }
    }   

    public override void Enter()
    {
        SetupShootState();
        SetupWeaponDataForShooting();
        SetupWeaponDataForCameraRecoil();
    }

    private void SetupShootState()
    {
        animationManager.SetIsShooting(true);
        fireTimer = 0f;
    }

    private void SetupWeaponDataForShooting()
    {
        animationRecoilMagnitude = weaponManager.CurrentWeaponData.animationRecoilMagnitude;
        bulletSpreadHorizontal = weaponManager.CurrentWeaponData.bulletSpreadHorizontal;
        bulletSpreadVertical = weaponManager.CurrentWeaponData.bulletSpreadVertical;
        fireRate = weaponManager.CurrentWeaponData.fireRate;
    }

    private void SetupWeaponDataForCameraRecoil()
    {
        player.PlayerCameraController.SetCameraRecoilFromWeaponData(
            weaponManager.CurrentWeaponData.cameraRecoilX,
            weaponManager.CurrentWeaponData.cameraRecoilY,
            weaponManager.CurrentWeaponData.cameraRecoilZ,
            weaponManager.CurrentWeaponData.cameraRecoilSnapiness,
            weaponManager.CurrentWeaponData.cameraRecoilReturnSpeed
        );
    }

    public override void Exit(PlayerState nextState)
    {
        base.Exit(nextState);
        animationManager.SetIsShooting(false);
        weaponManager.StopMuzzleEffect();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        fireTimer -= Time.deltaTime;

        if (!player.PlayerInput.IsAttacking && player.PlayerInput.IsAiming)
        {
            stateMachine.SetState(player.aim);
            return;
        }

        if (player.PlayerInput.IsAttacking && fireTimer <= 0f)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        ApplyAnimationRecoil();
        ApplyCameraRecoil();

        // Get the point where the camera is aiming
        Vector3 origin = mainCamera.transform.position;
        Vector3 direction = GetShotDirection();
        float range = weaponManager.CurrentWeaponData.range;

        // if (TryFireHitscanBullet(origin, direction, range, out RaycastHit hit))
        // {
        //     HandleBulletImpact(hit, direction);
        // }
        FireRigidbodyBullet();

        player.CrosshairController.SetCrosshair(
            1f,
            bulletSpreadHorizontal,
            bulletSpreadVertical,
            0.1f
        );

        PlayWeaponEffects();
        fireTimer = fireRate;
    }

    // Apply visual recoil to weapon and animation:
    private void ApplyAnimationRecoil()
    {
        ikRecoil.Fire(animationRecoilMagnitude);
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
        Weapon weapon = weaponManager.CurrentWeaponInstance.GetComponent<Weapon>();
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
            Debug.LogWarning("PuppetMaster component not found in parent hierarchy.");
            return;
        }

        Enemy enemy = puppetMaster.targetRoot.GetComponent<Enemy>();
        Limb limb = hit.collider.gameObject.GetComponent<Limb>();

        if (enemy == null)
        {
            Debug.LogWarning("Enemy component not found in parent hierarchy.");
            return;
        }

        if (limb == null)
        {
            Debug.LogWarning("Limb component not found in parent hierarchy.");
        }

        // Do damage to the enemy
        enemy.ProcessHit(weaponManager.CurrentWeaponData.damage, limb);

        // Spawn blood effect regardless of body part presence
        SpawnBloodEffect(hit, enemy);

        enemy.stateMachine.SetState(enemy.Aggro);
    }

    private void SpawnBloodEffect(RaycastHit hit, Enemy enemy)
    {
        GameObject bloodEffect = Object.Instantiate(
            enemy.template.bloodEffectPrefab,
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
