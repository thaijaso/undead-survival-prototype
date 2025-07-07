using UnityEngine;
using PlayerStates;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using RootMotion.FinalIK;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(-100)] // Ensure Player runs before other components
public class Player : MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; }
    public PlayerCharacterController PlayerCharacterController { get; private set; }
    public PlayerCameraController PlayerCameraController { get; private set; }
    public PlayerIKController PlayerIKController { get; private set; }
    public AnimationManager AnimationManager { get; private set; }

    public PlayerWeaponManager WeaponManager { get; private set; }

    public HealthManager HealthManager { get; private set; }

    public RecoilIK Recoil { get; private set; }
    public BulletHitscan BulletHitscan { get; private set; }

    public BulletDecalManager BulletDecalManager { get; private set; }

    public StateMachine<PlayerState> stateMachine;
    
    internal PlayerState idle;
    internal PlayerState sprint;
    internal PlayerState jump;
    internal PlayerState aim;
    internal PlayerState shoot;
    internal PlayerState strafe;

    [TabGroup("Configuration")]
    [Required]
    [AssetsOnly]
    [InfoBox("Player template containing health, movement speeds, and other core stats.")]
    public PlayerTemplate playerTemplate;

    [TabGroup("References")]
    [Header("References")]
    [SerializeField] private Transform weaponHand;
    public Transform WeaponHand => weaponHand;

    [TabGroup("References")]
    [SerializeField] private CrosshairController crosshairController;
    public CrosshairController CrosshairController => crosshairController;

    private void Awake()
    {
        SetupPlayerInput();
        SetupPlayerCharacterController();
        SetupPlayerCameraController();
        SetupAnimator();
        SetupPlayerIKController();
        SetupWeaponManager();
        SetupHealthManager();
        SetupRecoil();
        SetupBulletHitscan();
        SetupBulletDecalManager();

        stateMachine = new StateMachine<PlayerState>(gameObject.name);
    }

    private void SetupPlayerInput()
    {
        PlayerInput = GetComponent<PlayerInput>();

        if (PlayerInput == null)
            Debug.LogError($"[{gameObject.name}] Player.SetupPlayerInput(): PlayerInput component is missing!");
    }

    private void SetupPlayerCharacterController()
    {
        PlayerCharacterController = GetComponent<PlayerCharacterController>();

        if (PlayerCharacterController == null)
            Debug.LogError($"[{gameObject.name}] Player.SetupPlayerCharacterController(): PlayerCharacterController component is missing!");
    }

    private void SetupPlayerCameraController()
    {
        PlayerCameraController = GetComponent<PlayerCameraController>();

        if (PlayerCameraController == null)
            Debug.LogError($"[{gameObject.name}] Player.SetupPlayerCameraController(): PlayerCameraController component is missing!");
    }

    private void SetupAnimator()
    {
        Animator animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"[{gameObject.name}] Player.SetupAnimator(): Animator component is missing!");
            return;
        }

        animator.applyRootMotion = false; // Disable root motion to control movement manually
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        AnimationManager = new AnimationManager(animator);
    }

    private void SetupPlayerIKController()
    {
        PlayerIKController = GetComponent<PlayerIKController>();

        if (PlayerIKController == null)
            Debug.LogError($"[{gameObject.name}] Player.SetupPlayerIKController(): PlayerIKController component is missing!");
    }

    private void SetupWeaponManager()
    {
        WeaponManager = GetComponent<PlayerWeaponManager>();

        if (WeaponManager == null)
            Debug.LogError($"[{gameObject.name}] Player.SetupWeaponManager(): PlayerWeaponManager component is missing!");
    }

    private void SetupRecoil()
    {
        Recoil = GetComponent<RecoilIK>();

        if (Recoil == null)
            Debug.LogError($"[{gameObject.name}] Player.SetupRecoil(): Recoil component is missing!");
    }

    private void SetupBulletHitscan()
    {
        BulletHitscan = GetComponent<BulletHitscan>();

        if (BulletHitscan == null)
            Debug.LogError($"[{gameObject.name}] Player.SetupBulletHitscan(): BulletHitscan component is missing!");
    }

    private void SetupBulletDecalManager()
    {
        BulletDecalManager = GetComponent<BulletDecalManager>();

        if (BulletDecalManager == null)
            Debug.LogError($"[{gameObject.name}] Player.SetupBulletDecalManager(): BulletDecalManager component is missing!");
    }

    private void SetupHealthManager()
    {
        HealthManager = GetComponent<HealthManager>();

        if (HealthManager == null)
            Debug.LogError($"[{gameObject.name}] Player.SetupHealthManager(): HealthManager component is missing!");
        else
            Debug.Log($"[{gameObject.name}] HealthManager initialized successfully.");
    }

    void Start()
	{
        // Initialize HealthManager with template data
        if (HealthManager != null && playerTemplate != null)
        {
            Debug.Log($"[{gameObject.name}] About to initialize HealthManager. Current health: {HealthManager.currentHealth}.");
            HealthManager.Initialize(playerTemplate.maxHealth);
            Debug.Log($"[{gameObject.name}] HealthManager initialized. New health: {HealthManager.currentHealth}.");
        }
        else if (playerTemplate == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerTemplate is not assigned. Cannot initialize HealthManager.");
        }

        // Initialize PlayerCharacterController with template data
        if (PlayerCharacterController != null && playerTemplate != null)
        {
            PlayerCharacterController.Initialize(playerTemplate.strafeSpeed, playerTemplate.sprintSpeed, playerTemplate.gravity);
        }
        else if (playerTemplate == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerTemplate is not assigned. Cannot initialize PlayerCharacterController.");
        }

        // Initialize states:
        Debug.Log($"[{gameObject.name}] Initializing player states...");

        idle = new IdleState(this, stateMachine, AnimationManager, "Idle");
        Debug.Log($"[{gameObject.name}] ✓ Idle state initialized.");

        sprint = new SprintState(this, stateMachine, AnimationManager, "Sprint");
        Debug.Log($"[{gameObject.name}] ✓ Sprint state initialized.");

        strafe = new StrafeState(this, stateMachine, AnimationManager, "Strafe");
        Debug.Log($"[{gameObject.name}] ✓ Strafe state initialized.");
        
        aim = new AimState(
            this,
            stateMachine,
            AnimationManager,
            "Aim",
            WeaponManager
        );
        Debug.Log($"[{gameObject.name}] ✓ Aim state initialized.");

        shoot = new ShootState(
            this,
            stateMachine,
            AnimationManager,
            "Shoot",
            WeaponManager,
            Recoil,
            BulletHitscan,
            BulletDecalManager
        );
        Debug.Log($"[{gameObject.name}] ✓ Shoot state initialized");

        Debug.Log($"[{gameObject.name}] All player states initialized. Setting initial state to Idle...");
        // Set initial state
        stateMachine.SetState(idle);
    }

    // Update is called once per frame
    void Update()
	{
        stateMachine.LogicUpdate();
    }

    void FixedUpdate()
    {
        stateMachine.PhysicsUpdate();
    }

    void LateUpdate()
    {
        stateMachine.LateUpdate();
    }

    /// <summary>
    /// Processes damage to the player
    /// </summary>
    /// <param name="damage">Amount of damage to take</param>
    public void ProcessHit(int damage)
    {
        if (HealthManager == null) return;

        HealthManager.TakeDamage(damage);
        Debug.Log($"[{name}] Player.ProcessHit(): Took {damage} damage. Remaining health: {HealthManager.currentHealth}");

        if (HealthManager.currentHealth <= 0)
        {
            Debug.Log($"[{name}] Player defeated!");
            // TODO: Handle player death (game over, respawn, etc.)
        }
    }
}
