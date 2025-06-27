using UnityEngine;
using PlayerStates;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerCharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerCameraController))]
[RequireComponent(typeof(PlayerIKController))]
[RequireComponent(typeof(PlayerWeaponManager))]
[RequireComponent(typeof(IKRecoil))]
[RequireComponent(typeof(BulletHitscan))]
[RequireComponent(typeof(BulletDecalManager))]
public class Player : MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; }
    public PlayerCharacterController PlayerCharacterController { get; private set; }
    public PlayerCameraController PlayerCameraController { get; private set; }
    public PlayerIKController PlayerIKController { get; private set; }
    public AnimationManager AnimationManager { get; private set; }

    public PlayerWeaponManager WeaponManager { get; private set; }

    public IKRecoil Recoil { get; private set; }
    public BulletHitscan BulletHitscan { get; private set; }

    public BulletDecalManager BulletDecalManager { get; private set; }

    private StateMachine<PlayerState> stateMachine;
    
    internal PlayerState idle;
    internal PlayerState sprint;
    internal PlayerState jump;
    internal PlayerState aim;
    internal PlayerState shoot;
    internal PlayerState strafe;

    [Header("References")]
    [SerializeField] private Transform weaponHand;
    public Transform WeaponHand => weaponHand;

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
        SetupRecoil();
        SetupBulletHitscan();
        SetupBulletDecalManager();

        stateMachine = new StateMachine<PlayerState>(gameObject.name);
    }

    private void SetupPlayerInput()
    {
        PlayerInput = GetComponent<PlayerInput>();

        if (PlayerInput == null)
            Debug.LogError("PlayerInput component is missing!");
    }

    private void SetupPlayerCharacterController()
    {
        PlayerCharacterController = GetComponent<PlayerCharacterController>();

        if (PlayerCharacterController == null)
            Debug.LogError("PlayerCharacterController component is missing!");
    }

    private void SetupPlayerCameraController()
    {
        PlayerCameraController = GetComponent<PlayerCameraController>();

        if (PlayerCameraController == null)
            Debug.LogError("PlayerCameraController component is missing!");
    }

    private void SetupAnimator()
    {
        Animator animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component is missing!");
            return;
        }

        AnimationManager = new AnimationManager(animator);
    }

    private void SetupPlayerIKController()
    {
        PlayerIKController = GetComponent<PlayerIKController>();

        if (PlayerIKController == null)
            Debug.LogError("PlayerIKController component is missing!");
    }

    private void SetupWeaponManager()
    {
        WeaponManager = GetComponent<PlayerWeaponManager>();

        if (WeaponManager == null)
            Debug.LogError("PlayerWeaponManager component is missing!");
    }

    private void SetupRecoil()
    {
        Recoil = GetComponent<IKRecoil>();

        if (Recoil == null)
            Debug.LogError("Recoil component is missing!");
    }

    private void SetupBulletHitscan()
    {
        BulletHitscan = GetComponent<BulletHitscan>();

        if (BulletHitscan == null)
            Debug.LogError("BulletHitscan component is missing!");
    }

    private void SetupBulletDecalManager()
    {
        BulletDecalManager = GetComponent<BulletDecalManager>();

        if (BulletDecalManager == null)
            Debug.LogError("BulletDecalManager component is missing!");
    }

    void Start()
	{
        // Initialize states:
        idle = new IdleState(this, stateMachine, AnimationManager, "Idle");
        sprint = new SprintState(this, stateMachine, AnimationManager, "Sprint");
        strafe = new StrafeState(this, stateMachine, AnimationManager, "Strafe");
        
        aim = new AimState(
            this,
            stateMachine,
            AnimationManager,
            "Aim",
            WeaponManager
        );

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
}
