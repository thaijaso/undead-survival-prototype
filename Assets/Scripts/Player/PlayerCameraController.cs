using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public float CameraHorizontalRotationSpeed { get; private set; }
    private float previousHorizontalAxisValue = 0f;
    [SerializeField] private CinemachineCamera playerCamera;

    [SerializeField] private Transform forwardsFollowTarget;
    [SerializeField] private Transform backwardsFollowTarget;

    [SerializeField] private Transform aimTarget;

    [SerializeField] private float followFOV = 40f;

    [SerializeField] private float aimFOV = 28.7f;

    [SerializeField] private float zoomSpeed = 5f;

    private CinemachineBasicMultiChannelPerlin noise;

    private CinemachineOrbitalFollow orbitalFollow;

    private float cameraSwayAmount = 1f; // Default sway amount

    public Transform GetForwardsFollowTarget() => forwardsFollowTarget;
    public Transform GetBackwardsFollowTarget() => backwardsFollowTarget;

    private float currentHorizontalAxisValue;

    private float currentVerticalAxisValue;

    private bool lockCursor = false; // Whether to lock the cursor in the center of the screen
    private bool lastLockCursorState = false; // Track previous cursor lock state

    private CameraRecoil cameraRecoil;
    private CinemachineInputAxisController inputAxisController;

    private void Awake()
    {
        SetupNoise();
        SetupOrbitalFollow();
        SetupCameraRecoil();
        SetupCinemachineInputAxisController();
    }

    private void Start()
    {
        currentHorizontalAxisValue = orbitalFollow.HorizontalAxis.Value;
        currentVerticalAxisValue = orbitalFollow.VerticalAxis.Value;
        
        // Initialize cursor to unlocked state so user can see it initially
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Force initial cursor lock state detection
        HandleCursorLock();
    }

    private void SetupNoise()
    {
        noise = playerCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        if (noise == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerCameraController.SetupCameraSway(): CinemachineBasicMultiChannelPerlin component not found on the follow camera.");
        }
    }

    private void SetupOrbitalFollow()
    {
        orbitalFollow = playerCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineOrbitalFollow;
        if (orbitalFollow == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerCameraController.SetupOrbitalFollow(): CinemachineOrbitalFollow component not found on the follow camera.");
        }
    }

    private void SetupCameraRecoil()
    {
        cameraRecoil = playerCamera.GetComponent<CameraRecoil>();
    }

    private void SetupCinemachineInputAxisController()
    {
        inputAxisController = playerCamera.GetComponent<CinemachineInputAxisController>();
        Debug.Log($"[{gameObject.name}] PlayerCameraController.SetupCinemachineInputAxisController(): CinemachineInputAxisController found: {(inputAxisController != null)}");
        
        if (inputAxisController != null)
        {
            Debug.Log($"[{gameObject.name}] PlayerCameraController.SetupCinemachineInputAxisController(): Initial inputAxisController.enabled state: {inputAxisController.enabled}.");
        }
    }

    void Update()
    {
        HandleCursorLock();
        UpdateRotationSpeed();
    }

    private void HandleCursorLock()
    {
        // Fallback ESC key handling - detect ESC press and unlock cursor manually
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"[{gameObject.name}] PlayerCameraController.HandleCursorLock(): ESC key detected - unlocking cursor.");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Detect mouse click to re-lock cursor
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Debug.Log($"[{gameObject.name}] PlayerCameraController.HandleCursorLock(): Mouse click detected - locking cursor.");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Monitor Unity's cursor state - check both lockState and visibility
        // Sometimes Unity changes visibility without changing lockState
        bool currentCursorLocked = (Cursor.lockState == CursorLockMode.Locked) && !Cursor.visible;
        
        // Only update inputAxisController if cursor lock state has changed
        if (lastLockCursorState != currentCursorLocked)
        {
            Debug.Log($"[{gameObject.name}] PlayerCameraController.HandleCursorLock(): Cursor lock state changed to: {currentCursorLocked}." +
                     $"(lockState: {Cursor.lockState}, visible: {Cursor.visible})");
            
            // Enable/disable input axis controller based on cursor lock state
            if (inputAxisController != null)
            {
                inputAxisController.enabled = currentCursorLocked;
                Debug.Log($"[{gameObject.name}] PlayerCameraController.HandleCursorLock(): Set inputAxisController.enabled to: {currentCursorLocked}.");
            }
            
            lastLockCursorState = currentCursorLocked;
            lockCursor = currentCursorLocked; // Keep our internal state in sync
        }
    }

    private void UpdateRotationSpeed()
    {
        float current = orbitalFollow.HorizontalAxis.Value;
        CameraHorizontalRotationSpeed = Mathf.Abs((current - previousHorizontalAxisValue) / Time.deltaTime);
        previousHorizontalAxisValue = current;
    }

    public void MoveAimTarget()
    {
        Camera unityCam = Camera.main;
        if (unityCam == null) return;

        Vector3 screenCenter = new(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = unityCam.ScreenPointToRay(screenCenter);

        RaycastHit hit;
        float maxDistance = 10f; // TODO: create a weapon data field for max distance of weapon
        float minDistance = 4f; // Minimum distance for aim target

        Vector3 targetPosition;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            float hitDistance = Vector3.Distance(ray.origin, hit.point);
            if (hitDistance < minDistance)
            {
                // If hit is too close, set target at minimum distance
                targetPosition = ray.origin + ray.direction * minDistance;
            }
            else
            {
                targetPosition = hit.point;
            }
        }
        else
        {
            targetPosition = ray.origin + ray.direction * maxDistance;
        }

        aimTarget.position = targetPosition;
    }

    public Vector3 GetAimTarget()
    {
        return aimTarget.position;
    }

    public Transform GetFollowCamTransform()
    {
        return playerCamera.transform;
    }

    public void ZoomIn()
    {
        if (Mathf.Abs(playerCamera.Lens.FieldOfView - aimFOV) > 0.01f)
        {
            playerCamera.Lens.FieldOfView = Mathf.Lerp(
                playerCamera.Lens.FieldOfView,
                aimFOV,
                Time.deltaTime * zoomSpeed
            );
        }
    }

    public void ZoomOut()
    {
        if (Mathf.Abs(playerCamera.Lens.FieldOfView - followFOV) > 0.01f)
        {
            playerCamera.Lens.FieldOfView = Mathf.Lerp(
                playerCamera.Lens.FieldOfView,
                followFOV,
                Time.deltaTime * zoomSpeed
            );
        }
    }

    public void SetCameraSwayAmount(float swayAmount)
    {
        cameraSwayAmount = swayAmount;

        if (noise != null)
        {
            noise.AmplitudeGain = swayAmount;
        }
    }

    public void EnableCameraSway()
    {
        if (noise != null)
        {
            noise.AmplitudeGain = cameraSwayAmount;
        }
    }

    public void DisableCameraSway()
    {
        if (noise != null)
        {
            noise.AmplitudeGain = 0f;
        }
    }

    public bool HasCameraAxisChanged()
    {
        if (Mathf.Abs(currentHorizontalAxisValue - orbitalFollow.HorizontalAxis.Value) > 0.01f ||
            Mathf.Abs(currentVerticalAxisValue - orbitalFollow.VerticalAxis.Value) > 0.01f)
        {
            currentHorizontalAxisValue = orbitalFollow.HorizontalAxis.Value;
            currentVerticalAxisValue = orbitalFollow.VerticalAxis.Value;

            return true;
        }

        return false;
    }

    public void ApplyCameraRecoil()
    {
        if (cameraRecoil != null)
        {
            cameraRecoil.Fire();
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerCameraController.ApplyCameraRecoil(): CameraRecoil component not found on the player camera.");
        }
    }

    public void SetCameraRecoilFromWeaponData(float recoilX, float recoilY, float recoilZ, float snapiness, float returnSpeed)
    {
        if (cameraRecoil != null)
        {
            cameraRecoil.recoilX = recoilX;
            cameraRecoil.recoilY = recoilY;
            cameraRecoil.recoilZ = recoilZ;
            cameraRecoil.snapiness = snapiness;
            cameraRecoil.returnSpeed = returnSpeed;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerCameraController.SetCameraRecoilFromWeaponData(): CameraRecoil component not found on the player camera.");
        }
    }
}
