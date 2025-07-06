using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerIKController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float headLookWeight = 1f;

    // IK weight blending
    private float currentIKWeight = 0f;
    private float targetIKWeight = 1f;

    [Header("IK Blending Settings")]
    [Range(0.1f, 20f)]
    [SerializeField]
    private float blendSpeed = 5f;

    [Range(0f, 1f)]
    [SerializeField]
    private float inspectorTargetIKWeight = 1f;

    public Vector3 gunHoldOffset;
    public Vector3 leftHandOffset;
    public RecoilIK recoil;

    // The IK components
    private AimIK aimIK;
    private FullBodyBipedIK fullBodyBipedIK;
    private LookAtIK lookAtIK;

    private Vector3 headLookAxis;
    private Vector3 leftHandPosRelToRightHand;
    private Quaternion leftHandRotRelToRightHand;
    private Vector3 aimTarget;
    private Quaternion rightHandRotation;

    // Debug flag to allow inspector override of IK weights
    [Header("Debug")]
    [SerializeField]
    public bool DebugOverrideIKWeight = false;


    protected void Awake()
    {
        // Find the IK components (they may be null if not present)
        aimIK = GetComponent<AimIK>();
        fullBodyBipedIK = GetComponent<FullBodyBipedIK>();
        lookAtIK = GetComponent<LookAtIK>();

        // Only setup FBBIK if it exists
        if (fullBodyBipedIK != null)
        {
            fullBodyBipedIK.solver.OnPreRead += OnPreRead;

            // Disable the FBBIK component to manage its updating
            fullBodyBipedIK.enabled = false;

            // Presuming head is rotated towards character forward at Start
            headLookAxis = fullBodyBipedIK.references.head.InverseTransformVector(fullBodyBipedIK.references.root.forward);
        }

        // Only disable AimIK if it exists
        if (aimIK != null)
        {
            aimIK.enabled = false;
        }

        // Log which components were found for debugging
        Debug.Log($"[PlayerIKController] Components found - AimIK: {aimIK != null}, FBBIK: {fullBodyBipedIK != null}, LookAtIK: {lookAtIK != null}");
    }

    public void UpdateIKs(Vector3 faceDirection, Vector3 aimTarget)
    {
        // Snatch the aim target from the Move call, it will be used by AimIK (Move is called by CharacterController3rdPerson that controls the actual motion of the character)
        this.aimTarget = aimTarget;

        // IK procedures, make sure this updates AFTER the camera is moved/rotated
        // Sample something from the current pose of the character
        Read();

        // AimIK pass
        AimIK();

        // FBBIK pass - put the left hand back to where it was relative to the right hand before AimIK solved
        FBBIK();

        // AimIK pass
        AimIK();

        // Rotate the head to look at the aim target
        HeadLookAt(aimTarget);
    }

    public void SetGunHoldOffset(WeaponIKOffsets offsets)
    {
        gunHoldOffset = offsets.gunHoldOffset;
        leftHandOffset = offsets.leftHandOffset;
    }

    private void Read()
    {
        // Only read hand positions if FBBIK is available
        if (fullBodyBipedIK != null && fullBodyBipedIK.references.rightHand != null && fullBodyBipedIK.references.leftHand != null)
        {
            // Remember the position and rotation of the left hand relative to the right hand
            leftHandPosRelToRightHand = fullBodyBipedIK.references.rightHand.InverseTransformPoint(fullBodyBipedIK.references.leftHand.position);
            leftHandRotRelToRightHand = Quaternion.Inverse(fullBodyBipedIK.references.rightHand.rotation) * fullBodyBipedIK.references.leftHand.rotation;
        }
    }

    private void AimIK()
    {
        // Only update AimIK if it exists
        if (aimIK != null)
        {
            // Set AimIK target position and update
            aimIK.solver.IKPosition = aimTarget;
            aimIK.solver.Update(); // Update AimIK
        }
    }

    // Positioning the left hand on the gun after aiming has finished
    private void FBBIK()
    {
        // Only update FBBIK if it exists
        if (fullBodyBipedIK == null || fullBodyBipedIK.references.rightHand == null || fullBodyBipedIK.references.leftHand == null)
            return;

        // Store the current rotation of the right hand
        rightHandRotation = fullBodyBipedIK.references.rightHand.rotation;

        // Offsetting hands, you might need that to support multiple weapons with the same aiming pose
        Vector3 rightHandOffset = fullBodyBipedIK.references.rightHand.rotation * gunHoldOffset;
        fullBodyBipedIK.solver.rightHandEffector.positionOffset += rightHandOffset;

        if (recoil != null) recoil.SetHandRotations(rightHandRotation * leftHandRotRelToRightHand, rightHandRotation);

        // Update FBBIK
        fullBodyBipedIK.solver.Update();

        // Rotating the hand bones after IK has finished
        if (recoil != null)
        {
            fullBodyBipedIK.references.rightHand.rotation = recoil.rotationOffset * rightHandRotation;
            fullBodyBipedIK.references.leftHand.rotation = recoil.rotationOffset * rightHandRotation * leftHandRotRelToRightHand;
        }
        else
        {
            fullBodyBipedIK.references.rightHand.rotation = rightHandRotation;
            fullBodyBipedIK.references.leftHand.rotation = rightHandRotation * leftHandRotRelToRightHand;
        }
    }

    // Final calculations before FBBIK solves. Recoil has already solved by, so we can use its calculated offsets. 
    // Here we set the left hand position relative to the position and rotation of the right hand.
    private void OnPreRead()
    {
        // Only execute if FBBIK is available
        if (fullBodyBipedIK == null || fullBodyBipedIK.references.rightHand == null || fullBodyBipedIK.references.leftHand == null)
            return;

        Quaternion r = recoil != null ? recoil.rotationOffset * rightHandRotation : rightHandRotation;
        Vector3 leftHandTarget = fullBodyBipedIK.references.rightHand.position + fullBodyBipedIK.solver.rightHandEffector.positionOffset + r * leftHandPosRelToRightHand;
        fullBodyBipedIK.solver.leftHandEffector.positionOffset += leftHandTarget - fullBodyBipedIK.references.leftHand.position - fullBodyBipedIK.solver.leftHandEffector.positionOffset + r * leftHandOffset;
    }

    // Rotating the head to look at the target
    private void HeadLookAt(Vector3 lookAtTarget)
    {
        // Only execute if FBBIK is available
        if (fullBodyBipedIK == null || fullBodyBipedIK.references.head == null)
            return;

        Quaternion headRotationTarget = Quaternion.FromToRotation(fullBodyBipedIK.references.head.rotation * headLookAxis, lookAtTarget - fullBodyBipedIK.references.head.position);
        fullBodyBipedIK.references.head.rotation = Quaternion.Lerp(Quaternion.identity, headRotationTarget, headLookWeight) * fullBodyBipedIK.references.head.rotation;
    }

    // Cleaning up the delegates
    void OnDestroy()
    {
        if (fullBodyBipedIK != null) fullBodyBipedIK.solver.OnPreRead -= OnPreRead;
    }

    public void SetIKWeights(float weight)
    {
        currentIKWeight = weight;
        if (aimIK != null)
            aimIK.solver.IKPositionWeight = currentIKWeight;
        if (fullBodyBipedIK != null)
            fullBodyBipedIK.solver.IKPositionWeight = currentIKWeight;
        if (lookAtIK != null)
            lookAtIK.solver.IKPositionWeight = currentIKWeight;
    }

    public void SetIKTargetWeight(float target)
    {
        if (DebugOverrideIKWeight) return; // Prevent state machine from overriding in debug mode
        targetIKWeight = Mathf.Clamp01(target);
        // Do not call SetIKWeights here for smooth blending
    }

    public void BlendIKWeights()
    {
        currentIKWeight = Mathf.MoveTowards(currentIKWeight, targetIKWeight, Time.deltaTime * blendSpeed);
        SetIKWeights(currentIKWeight);
    }

    public void SetAimTransform(Transform aimTransform)
    {
        if (aimIK != null)
        {
            aimIK.solver.transform = aimTransform;
        }
    }

    public void EnableAimIK()
    {
        var aimIK = GetComponent<RootMotion.FinalIK.AimIK>();
        if (aimIK != null)
            aimIK.enabled = true;
    }

    public void DisableAimIK()
    {
        var aimIK = GetComponent<RootMotion.FinalIK.AimIK>();
        if (aimIK != null)
            aimIK.enabled = false;
    }

    [Button("Apply IK Weights"), Sirenix.OdinInspector.EnableIf("@UnityEngine.Application.isPlaying")]
    public void ApplyIKWeights()
    {
        SetIKTargetWeight(inspectorTargetIKWeight);
        SetIKWeights(inspectorTargetIKWeight);
    }

    void Update()
    {
        // Skip all IK blending and updates if debug mode disables IK
        if (PlayerDebugger.DebugDisableIK)
        {
            SetIKWeights(0f);
            return;
        }
        if (DebugOverrideIKWeight)
        {
            SetIKWeights(inspectorTargetIKWeight); // Directly set from inspector
            return;
        }
        BlendIKWeights(); // Ensure smooth blending every frame
        // Optionally, for live inspector changes:
        // SetIKTargetWeight(inspectorTargetIKWeight);
    }

    void LateUpdate()
    {
        if (aimIK != null && aimIK.enabled)
        {
            Debug.DrawLine(aimIK.solver.transform.position, aimIK.solver.target.position, Color.green);
            Debug.DrawRay(aimIK.solver.transform.position, aimIK.solver.transform.forward * 2f, Color.red);
        }
    }
}
