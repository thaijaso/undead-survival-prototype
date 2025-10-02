using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{
    public class PlayerIKController : MonoBehaviour
    {
        [Header("IK System")]
        public bool IKEnabled = false;

        [SerializeField]
        [Range(0f, 1f)]
        private float targetAimIKWeight = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float targetFBBIKWeight = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float targetHeadLookWeight = 1f;

        private float currentHeadLookWeight = 1f;

        private float currentAimIKWeight = 0f;
        private float currentFBBIKWeight = 0f;

        [SerializeField]
        private float aimIKBlendInSpeed = 3f;

        [SerializeField]
        private float aimIKBlendOutSpeed = 3f;

        [SerializeField]
        private float fbbikBlendInSpeed = 3f;

        [SerializeField]
        private float fbbikBlendOutSpeed = 3f;

        [SerializeField]
        private float headLookBlendInSpeed = 3f;

        [SerializeField]
        private float headLookBlendOutSpeed = 3f;

        public Vector3 gunHoldOffset;
        public Vector3 leftHandOffset;
        public RecoilIK recoil;

        // The IK components
        private AimIK aimIK;
        private FullBodyBipedIK fullBodyBipedIK;
        private LookAtIK lookAtIK;

        // Reference to Player component
        private Player player;

        private Vector3 headLookAxis;
        private Vector3 leftHandPosRelToRightHand;
        private Quaternion leftHandRotRelToRightHand;
        private Vector3 aimTarget;
        private Quaternion rightHandRotation;

        [SerializeField]
        private Transform leftHandIKTarget;

        [SerializeField]
        private Transform leftHandGripSource;

        protected void Awake()
        {
            // Cache Player component
            player = GetComponent<Player>();
            // Find the IK components (they may be null if not present)
            aimIK = GetComponent<AimIK>();
            fullBodyBipedIK = GetComponent<FullBodyBipedIK>();
            lookAtIK = GetComponent<LookAtIK>();

            // Only setup FBBIK if it exists
            if (fullBodyBipedIK != null)
            {
                // Assign left hand effector target if available
                if (leftHandIKTarget != null)
                {
                    fullBodyBipedIK.solver.leftHandEffector.target = leftHandIKTarget;
                    fullBodyBipedIK.solver.leftHandEffector.positionWeight = 1f;
                    fullBodyBipedIK.solver.leftHandEffector.rotationWeight = 1f;
                    fullBodyBipedIK.solver.leftHandEffector.maintainRelativePositionWeight = 1f;
                    Debug.Log($"[PlayerIKController] Assigned leftHandIKTarget to FBBIK leftHandEffector.");
                }

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

        public void SetAimIkWeight(float weight)
        {
            targetAimIKWeight = weight;
        }

        public void SetFBBIKWeight(float weight)
        {
            targetFBBIKWeight = weight;
        }

        public void SetHeadLookAtWeight(float weight)
        {
            targetHeadLookWeight = weight;
        }

        public void DisableIK()
        {
            // zero targets
            targetAimIKWeight = 0f;
            targetFBBIKWeight = 0f;
            targetHeadLookWeight = 0f;
        }

        public void SetLeftHandGripSource(Transform gripSource)
        {
            leftHandGripSource = gripSource;
        }

        void Update()
        {
            BlendAllIKWeights();
        }

        void LateUpdate()
        {
            //BlendAllIKWeights(); // Ensure smooth blending every frame
            UpdateAllIKWeights();

            if (aimIK != null && aimIK.enabled)
            {
                Debug.DrawLine(aimIK.solver.transform.position, aimIK.solver.target.position, Color.green);
                Debug.DrawRay(aimIK.solver.transform.position, aimIK.solver.transform.forward * 2f, Color.red);
            }

            if (leftHandIKTarget != null && fullBodyBipedIK != null && fullBodyBipedIK.references.leftHand != null)
            {
                Debug.DrawLine(
                    fullBodyBipedIK.references.leftHand.position,
                    leftHandIKTarget.position,
                    Color.magenta
                );
            }
        }

        public void UpdateAllIKWeights()
        {
            if (aimIK != null)
                aimIK.solver.IKPositionWeight = currentAimIKWeight;
            if (fullBodyBipedIK != null)
                fullBodyBipedIK.solver.IKPositionWeight = currentFBBIKWeight;
            if (lookAtIK != null)
                lookAtIK.solver.IKPositionWeight = currentHeadLookWeight;
        }

        private void BlendAllIKWeights()
        {
            // AimIK blending
            float aimBlendSpeed = currentAimIKWeight < targetAimIKWeight ? aimIKBlendInSpeed : aimIKBlendOutSpeed;
            currentAimIKWeight = Mathf.MoveTowards(currentAimIKWeight, targetAimIKWeight, Time.deltaTime * aimBlendSpeed);

            // FBBIK blending
            float fbbikBlendSpeed = currentFBBIKWeight < targetFBBIKWeight ? fbbikBlendInSpeed : fbbikBlendOutSpeed;
            currentFBBIKWeight = Mathf.MoveTowards(currentFBBIKWeight, targetFBBIKWeight, Time.deltaTime * fbbikBlendSpeed);

            // HeadLook blending
            float headLookBlendSpeed = currentHeadLookWeight < targetHeadLookWeight ? headLookBlendInSpeed : headLookBlendOutSpeed;
            currentHeadLookWeight = Mathf.MoveTowards(currentHeadLookWeight, targetHeadLookWeight, Time.deltaTime * headLookBlendSpeed);

            UpdateAllIKWeights();
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

            // 2. Store current hand rotations for recoil math (before FBBIK modifies anything)
            StoreHandRotationsForRecoil();

            // FBBIK pass - put the left hand back to where it was relative to the right hand before AimIK solved
            FBBIK();
            HeadLookAt(aimTarget);
            UpdateLeftHandIKTarget();
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
            if (aimIK != null && aimIK.solver != null && aimIK.solver.target != null && aimIK.solver.bones != null && aimIK.solver.bones.Length > 0)
            {
                if (!aimIK.solver.initiated)
                    aimIK.solver.Initiate(player.transform); // Or hips.x

                aimIK.solver.Update();
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
            }
            else
            {
                fullBodyBipedIK.references.rightHand.rotation = rightHandRotation;
            }
        }

        // Rotating the head to look at the target
        private void HeadLookAt(Vector3 lookAtTarget)
        {
            // Only execute if FBBIK is available
            if (fullBodyBipedIK == null || fullBodyBipedIK.references.head == null)
                return;

            Quaternion headRotationTarget = Quaternion.FromToRotation(fullBodyBipedIK.references.head.rotation * headLookAxis, lookAtTarget - fullBodyBipedIK.references.head.position);
            fullBodyBipedIK.references.head.rotation = Quaternion.Lerp(Quaternion.identity, headRotationTarget, currentHeadLookWeight) * fullBodyBipedIK.references.head.rotation;
        }

        public void SetAimTransform(Transform aimTransform)
        {
            if (aimIK != null)
            {
                aimIK.solver.transform = aimTransform;
            }
        }

        [Button("Freeze Animator (Set Speed 0)")]
        public void FreezeAnimator()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 0f;
                Debug.Log("[PlayerIKController] Animator speed set to 0 (frozen)");
            }
            else
            {
                Debug.LogWarning("[PlayerIKController] No Animator component found to freeze.");
            }
        }

        [Button("Unfreeze Animator (Set Speed 1)")]
        public void UnfreezeAnimator()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 1f;
                Debug.Log("[PlayerIKController] Animator speed set to 1 (unfrozen)");
            }
            else
            {
                Debug.LogWarning("[PlayerIKController] No Animator component found to unfreeze.");
            }
        }


        public void UpdateLeftHandIKTarget()
        {
            if (leftHandIKTarget != null && leftHandGripSource != null)
            {
                leftHandIKTarget.SetPositionAndRotation(leftHandGripSource.position, leftHandGripSource.rotation);
            }
        }

        private void StoreHandRotationsForRecoil()
        {
            if (recoil != null && fullBodyBipedIK != null)
            {
                recoil.SetHandRotations(
                    fullBodyBipedIK.references.leftHand.rotation,
                    fullBodyBipedIK.references.rightHand.rotation
                );
            }
        }
    }
}
