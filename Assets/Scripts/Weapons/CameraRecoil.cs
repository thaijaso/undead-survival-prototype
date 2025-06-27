using Unity.Cinemachine;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    private Vector3 currentRecoilOffset;
    private Vector3 targetRecoilOffset;

    [SerializeField]
    public float recoilX;

    [SerializeField]
    public float recoilY;

    [SerializeField]
    public float recoilZ;

    [SerializeField]
    public float snapiness;

    [SerializeField]
    public float returnSpeed;

    private CinemachineCamera playerCamera;
    private CinemachineOrbitalFollow orbitalFollow;

    private float lastRecoilOffsetY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        playerCamera = GetComponent<CinemachineCamera>();

        if (playerCamera == null)
        {
            Debug.LogError("CinemachineCamera component not found on the GameObject.");
        }

        orbitalFollow = playerCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineOrbitalFollow;
    }

    private void Update()
    {

        targetRecoilOffset = Vector3.Lerp(targetRecoilOffset, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilOffset = Vector3.Lerp(currentRecoilOffset, targetRecoilOffset, snapiness * Time.deltaTime);

        float deltaY = currentRecoilOffset.y - lastRecoilOffsetY;
        orbitalFollow.VerticalAxis.Value -= deltaY;
        lastRecoilOffsetY = currentRecoilOffset.y;
    }

    public void Fire()
    {
        float randomX = Random.Range(-recoilX, recoilX);
        float randomY = Random.Range(0, recoilY);
        float randomZ = Random.Range(-recoilZ, recoilZ);
        targetRecoilOffset = new Vector3(randomX, randomY, randomZ);
    }
}
