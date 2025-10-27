using RootMotion.Dynamics;
using Sirenix.OdinInspector;
using UndeadSurvivalGame.Effects;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.UI;
using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{
    public class Player : MonoBehaviour
    {
        public bool IsInside = false;
        public PlayerDebugger Debugger { get; private set; }
        public PlayerInput PlayerInput { get; private set; }
        public PlayerCharacterController PlayerCharacterController { get; private set; }
        public PlayerCameraController PlayerCameraController { get; private set; }
        public PlayerIKController PlayerIKController { get; private set; }
        public AimPoseLayerWeightController AimPoseLayerWeightController { get; private set; }
        public AimPitchLayerWeightController AimPitchLayerWeightController { get; private set; }
        public UpperBodyLayerWeightController UpperBodyLayerWeightController { get; private set; }
        public PlayerAnimatorEvents PlayerAnimatorEvents { get; private set; }
        public AnimationManager AnimationManager { get; private set; }

        public PlayerWeaponManager WeaponManager { get; private set; }

        public HealthManager HealthManager { get; private set; }

        public RecoilIK Recoil { get; private set; }
        public BulletHitscan BulletHitscan { get; private set; }

        public BulletDecalManager BulletDecalManager { get; private set; }

        public PuppetMaster PuppetMaster { get; private set; }

        public Inventory PlayerInventory { get; private set; }

        public InteractionSensor InteractionSensor { get; private set; }
        public WallDetector WallDetector { get; private set; }
        public StairDetector StairDetector { get; private set; }

        public StateMachine<PlayerState> stateMachine;
        
        internal PlayerState idle;
        internal PlayerState sprint;
        internal PlayerState jump;
        internal PlayerState aim;
        internal PlayerState shoot;
        internal PlayerState strafe;
        internal PlayerState hitReaction;
        internal PlayerState death;
        internal PlayerState reload;
        internal PlayerState walk;

        [TabGroup("Configuration")]
        [Required]
        [AssetsOnly]
        [InfoBox("Player template containing health, movement speeds, and other core stats.")]
        public PlayerTemplate playerTemplate;

        [TabGroup("References")]
        [Header("References")]
        [SerializeField]
        private Transform weaponHand;
        public Transform WeaponHand => weaponHand;

        [TabGroup("References")]
        [SerializeField] 
        private CrosshairController crosshairController;
        public CrosshairController CrosshairController => crosshairController;

        [TabGroup("References")]
        [SerializeField]
        private OverlayUIController overlayUIController;
        public OverlayUIController OverlayUIController => overlayUIController;

        [TabGroup("References")]
        [SerializeField]
        private CurrentWeaponUIController currentWeaponUIController;
        public CurrentWeaponUIController CurrentWeaponUIController => currentWeaponUIController;

        [TabGroup("References")]
        [SerializeField]
        private HealthUIController healthUIController;
        public HealthUIController HealthUIController => healthUIController;

        [TabGroup("References")]
        [SerializeField]
        private PlayerMenuUIController playerMenuUIController;
        public PlayerMenuUIController PlayerMenuUIController => playerMenuUIController;

        private void Awake()
        {
            SetupPlayerDebugger();
            SetupPlayerInput();
            SetupPlayerCharacterController();
            SetupPlayerCameraController();
            SetupAnimator();
            SetupAnimatorEvents();
            SetupPlayerIKController();
            SetupWeaponManager();
            SetupHealthManager();
            SetupRecoil();
            SetupBulletHitscan();
            SetupBulletDecalManager();
            SetupPuppetMaster();
            SetupHealthManager();
            SetupPlayerInventory();
            SetupPlayerMenuUIController();
            SetupPlayerInteractionSensor();
            SetupAimPoseLayerWeightController();
            SetupAimPitchLayerWeightController();
            SetupUpperBodyLayerWeightController();
            SetupWallDetector();
            SetupStairDetector();

            stateMachine = new StateMachine<PlayerState>(gameObject.name);
        }

        private void SetupPlayerDebugger()
        {
            Debugger = GetComponent<PlayerDebugger>();

            if (Debugger == null)
                Debug.LogError($"[{gameObject.name}] Player.SetupPlayerDebugger(): PlayerDebugger component is missing!");
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

        private void SetupAnimatorEvents()
        {
            PlayerAnimatorEvents = GetComponent<PlayerAnimatorEvents>();

            if (PlayerAnimatorEvents == null)
                Debug.LogError($"[{gameObject.name}] Player.SetupAnimatorEvents(): PlayerAnimatorEvents component is missing!");
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

        private void SetupPuppetMaster()
        {
            // Get the parent transform
            Transform parent = transform.parent;
            if (parent == null)
            {
                Debug.LogError($"[{gameObject.name}] Player.SetupPuppetMaster(): No parent found.");
                return;
            }

            // Search all children of the parent (siblings) for PuppetMaster
            PuppetMaster = null;
            foreach (Transform child in parent)
            {
                if (child == transform) continue; // Skip self
                PuppetMaster pm = child.GetComponent<PuppetMaster>();
                if (pm != null)
                {
                    PuppetMaster = pm;
                    Debug.Log($"[{gameObject.name}] Player.SetupPuppetMaster(): Found PuppetMaster on sibling '{child.name}'.");
                    return;
                }
            }

            Debug.LogError($"[{gameObject.name}] Player.SetupPuppetMaster(): PuppetMaster component not found on any sibling.");
        }

        private void SetupPlayerMenuUIController()
        {
            if (playerMenuUIController == null)
            {
                playerMenuUIController = FindFirstObjectByType<PlayerMenuUIController>();
                if (playerMenuUIController == null)
                {
                    Debug.LogError($"[{gameObject.name}] Player.SetupPlayerMenuUIController(): No PlayerMenuUIController found in scene!");
                }
            }
            else
            {
                Debug.Log($"[{gameObject.name}] Player.SetupPlayerMenuUIController(): PlayerMenuUIController already assigned.");
            }
        }

        private void SetupPlayerInventory()
        {
            PlayerInventory = GetComponent<Inventory>();

            if (PlayerInventory == null)
                Debug.LogError($"[{gameObject.name}] Player.SetupPlayerInventory(): Inventory component is missing!");
        }

        private void SetupPlayerInteractionSensor()
        {
            InteractionSensor = GetComponentInChildren<InteractionSensor>();

            if (InteractionSensor == null)
                Debug.LogError($"[{gameObject.name}] Player.SetupPlayerInteractionSensor(): PlayerInteractionSensor component is missing!");
        }

        private void SetupAimPoseLayerWeightController()
        {
            AimPoseLayerWeightController = GetComponent<AimPoseLayerWeightController>();

            if (AimPoseLayerWeightController == null)
                Debug.LogError($"[{gameObject.name}] Player.SetupAimPoseLayerWeightController(): AimPoseLayerWeightController component is missing!");
        }

        private void SetupAimPitchLayerWeightController()
        {
            AimPitchLayerWeightController = GetComponent<AimPitchLayerWeightController>();

            if (AimPitchLayerWeightController == null)
                Debug.LogError($"[{gameObject.name}] Player.SetupAimPitchLayerWeightController(): AimPitchLayerWeightController component is missing!");
        }

        private void SetupUpperBodyLayerWeightController()
        {
            UpperBodyLayerWeightController = GetComponent<UpperBodyLayerWeightController>();

            if (UpperBodyLayerWeightController == null)
                Debug.LogError($"[{gameObject.name}] Player.SetupUpperBodyLayerWeightController(): UpperBodyLayerWeightController component is missing!");
        }

        private void SetupWallDetector()
        {
            WallDetector = GetComponentInChildren<WallDetector>();

            if (WallDetector == null)
                Debug.LogError($"[{gameObject.name}] Player.SetupWallDetector(): WallDetector component is missing!");
        }

        private void SetupStairDetector()
        {
            StairDetector = GetComponentInChildren<StairDetector>();

            if (StairDetector == null)
                Debug.LogError($"[{gameObject.name}] Player.SetupStairDetector(): StairDetector component is missing!");
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

            // UI initialization: 
            InitHealthUIController();

            WeaponManager.EquipFirstWeaponFound(); // TODO: save system to remember last weapon equipped between scenes

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

            idle = new IdleState(this, stateMachine, AnimationManager, "Idle", WeaponManager);
            Debug.Log($"[{gameObject.name}] ✓ Idle state initialized.");

            sprint = new SprintState(this, stateMachine, AnimationManager, "Sprint", WeaponManager);
            Debug.Log($"[{gameObject.name}] ✓ Sprint state initialized.");

            strafe = new StrafeState(this, stateMachine, AnimationManager, "Strafe", WeaponManager);
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

            hitReaction = new HitReactionState(
                this,
                stateMachine,
                AnimationManager,
                "HitReaction",
                WeaponManager
            );

            Debug.Log($"[{gameObject.name}] ✓ HitReaction state initialized");

            death = new DeathState(
                this,
                stateMachine,
                AnimationManager,
                "Death",
                WeaponManager
            );

            Debug.Log($"[{gameObject.name}] ✓ Death state initialized.");

            reload = new ReloadState(
                this,
                stateMachine,
                AnimationManager,
                "Reload",
                WeaponManager
            );

            Debug.Log($"[{gameObject.name}] ✓ Reload state initialized.");

            walk = new WalkState(
                this,
                stateMachine,
                AnimationManager,
                "Walk",
                WeaponManager
            );

            Debug.Log($"[{gameObject.name}] ✓ Walk state initialized.");
            Debug.Log($"[{gameObject.name}] All player states initialized. Setting initial state to Idle...");

            // Set initial state
            stateMachine.SetState(idle);
        }

        private void InitHealthUIController()
        {
            if (healthUIController != null && HealthManager != null)
            {
                healthUIController.Initialize(HealthManager.healthPercentage);
                return;
            }
            
            if (healthUIController == null)
            {
                Debug.LogError($"[{gameObject.name}] Player.SetupHealthBar(): HealthUIController is not assigned.");
            }

            if (HealthManager == null)
            {
                Debug.LogError($"[{gameObject.name}] Player.SetupHealthBar(): HealthManager is not assigned.");
            }
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

            float percentage = (float)HealthManager.currentHealth / HealthManager.maxHealth * 100f;
            healthUIController.UpdateHealthBar(percentage);
            Debug.Log($"Player.ProcessHit(): [{name}] Took {damage} damage. Remaining health: {HealthManager.currentHealth}");

            if (HealthManager.currentHealth <= 0)
            {
                Debug.Log($"Player.ProcessHit(): [{name}] Player health is 0!");
                stateMachine.SetState(death);
            }
        }
    }
}
