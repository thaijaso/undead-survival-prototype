using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerIKController))]
public class PlayerCharacterController : MonoBehaviour
{
    // Set this to true to enable debug logs
    [SerializeField] private bool debugLogs = false;

    private const float EPSILON = 0.01f; // For velocity clamping
    public CharacterController CharacterController { get; private set; }
    public PlayerIKController PlayerIKController { get; private set; }

    // These will be set from PlayerTemplate at runtime via Initialize()
    public float strafeSpeed { get; private set; } = 2.0f; // Default fallback
    public float sprintSpeed { get; private set; } = 5.0f; // Default fallback
    public float gravity { get; private set; } = -9.81f; // Default fallback

    // Expose velocity for debugging
    public Vector3 velocity => playerVelocity;

    private Vector3 playerVelocity;
    private bool isInitialized = false;

    [SerializeField]
    private float deceleration = 50f; // Increased for snappier stop
    [SerializeField]
    private float acceleration = 10f; // Units per second^2, tweak as needed

    private void Start()
    {
        CharacterController = GetComponent<CharacterController>();
        PlayerIKController = GetComponent<PlayerIKController>();
        
        // Fallback initialization if Initialize() wasn't called
        if (!isInitialized)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerCharacterController.Start(): Wasn't initialized from template, using default values - Strafe: {strafeSpeed}, Sprint: {sprintSpeed}, Gravity: {gravity}.");
        }
    }

    public void Initialize(float templateStrafeSpeed, float templateSprintSpeed, float templateGravity)
    {
        strafeSpeed = templateStrafeSpeed;
        sprintSpeed = templateSprintSpeed;
        gravity = templateGravity;
        isInitialized = true;
        Debug.Log($"[{gameObject.name}] PlayerCharacterController.Initialize(): Initialized from template - Strafe: {strafeSpeed}, Sprint: {sprintSpeed}, Gravity: {gravity}.");
    }

    private void LateUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (CharacterController.isGrounded && playerVelocity.y < 0)
        {
            // Reset the vertical velocity when grounded to prevent falling
            playerVelocity.y = -0.1f;
        } 
        else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
        // Remove CharacterController.Move from here
    }

    private Vector3 CalculateHorizontalVelocity(Vector3 direction, float speed, Vector3 currentHorizontalVelocity)
    {
        if (debugLogs)
            Debug.Log($"[PlayerCharacterController] RAW direction: {direction}, sqrMagnitude: {direction.sqrMagnitude}");
        if (direction.sqrMagnitude > 0.01f) // Increased threshold for input noise
        {
            if (debugLogs)
                Debug.Log($"[PlayerCharacterController] ACCELERATE: direction={direction}, speed={speed}, currentHorizontalVelocity={currentHorizontalVelocity}");
            // Desired velocity in the given direction
            Vector3 desiredVelocity = speed * direction.normalized;
            // Accelerate towards desired velocity
            return Vector3.MoveTowards(currentHorizontalVelocity, desiredVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            if (debugLogs)
                Debug.Log($"[PlayerCharacterController] DECELERATE: direction={direction}, currentHorizontalVelocity={currentHorizontalVelocity}");
            // Decelerate to zero
            return Vector3.MoveTowards(currentHorizontalVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }
    }

    public void Move(Vector3 direction, float speed)
    {
        Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);

        if (debugLogs)
            Debug.Log($"[PlayerCharacterController] direction: {direction}, direction.sqrMagnitude: {direction.sqrMagnitude}, horizontalVelocity: {horizontalVelocity}, horizontalVelocity.sqrMagnitude: {horizontalVelocity.sqrMagnitude}");

        // Always use acceleration/deceleration for transitions
        horizontalVelocity = CalculateHorizontalVelocity(direction, speed, horizontalVelocity);

        // Clamp very small velocities to zero to avoid floating-point sliding
        if (Mathf.Abs(horizontalVelocity.x) < EPSILON) horizontalVelocity.x = 0f;
        if (Mathf.Abs(horizontalVelocity.z) < EPSILON) horizontalVelocity.z = 0f;

        // Update playerVelocity with new horizontal values, keep vertical (gravity) unchanged
        playerVelocity = new Vector3(horizontalVelocity.x, playerVelocity.y, horizontalVelocity.z);
        CharacterController.Move(playerVelocity * Time.deltaTime);
    }
}


