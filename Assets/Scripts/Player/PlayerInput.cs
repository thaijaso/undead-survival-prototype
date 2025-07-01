using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction aimAction;
    private InputAction attackAction;

    public bool IsMoving { get; internal set; }

    public bool IsSprinting { get; internal set; }
    public bool IsJumping { get; internal set; }
    public bool IsAiming { get; internal set; }
    public bool IsAttacking { get; internal set; }

    [SerializeField]
    private float MovementThreshold = 0.2f;

    [SerializeField]
    private float animationSmoothTime = 0.05f;

    [SerializeField]
    private float maxInputThreshold = 0.6f;

    Vector3 currentAnimationBlendVector;
    Vector3 animationVelocity;

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
        IsMoving = Mathf.Abs(moveAction.ReadValue<Vector2>().x) > MovementThreshold || Mathf.Abs(moveAction.ReadValue<Vector2>().y) > MovementThreshold;
        IsSprinting = sprintAction.ReadValue<float>() > 0.0f;
        IsAiming = aimAction.ReadValue<float>() > 0.0f;
        IsAttacking = attackAction.ReadValue<float>() > 0.0f;

        //IsJumping = Input.GetButtonDown("Jump");
    }

    public Vector3 GetInputDirection()
    {
        float horizontal = moveAction.ReadValue<Vector2>().x;
        float vertical = moveAction.ReadValue<Vector2>().y;

        Vector3 direction = ClampInput(horizontal, vertical);

        // If input is below threshold, set direction to zero
        if (Mathf.Abs(horizontal) < MovementThreshold && Mathf.Abs(vertical) < MovementThreshold)
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
        }


        return currentAnimationBlendVector;
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
