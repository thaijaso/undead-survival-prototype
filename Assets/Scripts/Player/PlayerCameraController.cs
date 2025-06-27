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
    [SerializeField] private float aimDistance = 10.0f; // Distance from the camera to the aim target

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

    private bool lockCursor = true; // Whether to lock the cursor in the center of the screen

    private CameraRecoil cameraRecoil;

    private void Awake()
    {
        SetupNoise();
        SetupOrbitalFollow();
        SetupCameraRecoil();
    }

    private void Start()
    {
        currentHorizontalAxisValue = orbitalFollow.HorizontalAxis.Value;
        currentVerticalAxisValue = orbitalFollow.VerticalAxis.Value;
    }

    private void SetupNoise()
    {
        noise = playerCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        if (noise == null)
        {
            Debug.LogError("CinemachineBasicMultiChannelPerlin component not found on the follow camera.");
        }
    }

    private void SetupOrbitalFollow()
    {
        orbitalFollow = playerCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineOrbitalFollow;
        if (orbitalFollow == null)
        {
            Debug.LogError("CinemachineOrbitalFollow component not found on the follow camera.");
        }
    }

    private void SetupCameraRecoil()
    {
        cameraRecoil = playerCamera.GetComponent<CameraRecoil>();
    }

    void Update()
    {
        HandleCursorLock();
        UpdateRotationSpeed();
    }

    private void HandleCursorLock()
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = lockCursor ? false : true;
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
        float maxDistance = 100f; // TODO: create a weapon data field for max distance of weapon

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            aimTarget.position = hit.point;
        }
        else
        {
            aimTarget.position = ray.origin + ray.direction * maxDistance;
        }
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
            Debug.LogWarning("CameraRecoil component not found on the player camera.");
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
            Debug.LogWarning("CameraRecoil component not found on the player camera.");
        }
    }
}
