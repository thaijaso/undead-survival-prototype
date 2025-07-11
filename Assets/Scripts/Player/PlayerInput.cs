using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction aimAction;
    private InputAction attackAction;

    public bool IsMoving { get; internal set; }

    // Used in the animator to determine if Player should enter walk cycle
    public bool MoveCommited { get; internal set; }

    private float moveGraceTimer = 0f;
    private float graceDuration = 0.2f;

    public bool IsSprinting { get; internal set; }
    public bool IsJumping { get; internal set; }
    public bool IsAiming { get; internal set; }
    public bool IsAttacking { get; internal set; }

    [SerializeField]
    private float movementThreshold = 0.2f;

    [SerializeField]
    private float animationSmoothTime = 0.05f;

    [SerializeField]
    private float maxInputThreshold = 0.6f;

    Vector3 currentAnimationBlendVector;
    Vector3 animationVelocity;

    // Tracks the last nonzero movement direction before stopping, for use in the animator
    public Vector3 stopDirection { get; private set; } = Vector3.forward;
    // Encoded as: 0 = forward, 1 = right, 2 = down, 3 = left
    public int stopDirectionIndex { get; private set; } = 0;

    private void Awake()
    {
        // Initialize the Input System
        if (InputSystem.settings == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerInput.Awake(): Input System settings not found.");
        }

        moveAction = InputSystem.actions.FindAction("Move");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        aimAction = InputSystem.actions.FindAction("Aim");
        attackAction = InputSystem.actions.FindAction("Attack");
    }

    // Update is called once per frame
    void Update()
    {
        IsMoving = Mathf.Abs(moveAction.ReadValue<Vector2>().x) > movementThreshold || Mathf.Abs(moveAction.ReadValue<Vector2>().y) > movementThreshold;
        IsSprinting = sprintAction.ReadValue<float>() > 0.0f;
        IsAiming = aimAction.ReadValue<float>() > 0.0f;
        IsAttacking = attackAction.ReadValue<float>() > 0.0f;

        if (IsMoving)
        {
            moveGraceTimer += Time.deltaTime;

            if (moveGraceTimer > graceDuration)
            {
                MoveCommited = true; // Allow movement to be committed after grace period
                Debug.Log($"[{gameObject.name}] PlayerInput.Update(): Move committed after grace period.");
            }
        }
        else
        {
            moveGraceTimer = 0f; // Reset grace timer when not moving
            MoveCommited = false; // Reset move committed state when not moving
            Debug.Log($"[{gameObject.name}] PlayerInput.Update(): Move reset, grace timer reset.");
        }
    }

    public Vector3 GetInputDirection()
    {
        float horizontal = moveAction.ReadValue<Vector2>().x;
        float vertical = moveAction.ReadValue<Vector2>().y;

        Vector3 direction = ClampInput(horizontal, vertical);

        // If input is below threshold, set direction to zero
        if (Mathf.Abs(horizontal) < movementThreshold && Mathf.Abs(vertical) < movementThreshold)
        {
            currentAnimationBlendVector = Vector3.zero; // Reset animation blend vector when no input
        }
        else
        {
            currentAnimationBlendVector = Vector3.SmoothDamp(
                currentAnimationBlendVector,
                direction,
                ref animationVelocity,
                animationSmoothTime
            );
            // Track the last nonzero direction for stop animation
            stopDirection = direction.normalized;
            stopDirectionIndex = GetDirectionIndex(stopDirection);
        }

        return currentAnimationBlendVector;
    }

    // 0 = forward, 1 = right, 2 = down, 3 = left
    private int GetDirectionIndex(Vector3 dir)
    {
        if (Vector3.Dot(dir, Vector3.forward) > 0.7f) return 0;
        if (Vector3.Dot(dir, Vector3.right) > 0.7f) return 1;
        if (Vector3.Dot(dir, Vector3.back) > 0.7f) return 2;
        if (Vector3.Dot(dir, Vector3.left) > 0.7f) return 3;
        return 0; // Default to forward if ambiguous
    }

    public float GetPositiveMaxInputThreshold() 
    {
        return maxInputThreshold;
    }

    // Clamp input for controller so animation plays at the same speed as keyboard
    private Vector3 ClampInput(float horizontal, float vertical)
    {
        if (horizontal >= maxInputThreshold)
        {
            horizontal = 1f;
        }

        if (horizontal <= -maxInputThreshold)
        {
            horizontal = -1f;
        }

        if (vertical >= maxInputThreshold)
        {
            vertical = 1f;
        }

        if (vertical <= -maxInputThreshold)
        {
            vertical = -1f;
        }

        Vector3 direction = new(horizontal, 0, vertical);
        return direction;
    }
}
