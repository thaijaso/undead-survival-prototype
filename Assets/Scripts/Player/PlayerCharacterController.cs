using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerIKController))]
public class PlayerCharacterController : MonoBehaviour
{
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

        CharacterController.Move(playerVelocity * Time.deltaTime);
    }

    public void Move(Vector3 direction, float speed)
    {
        CharacterController.Move(speed * Time.deltaTime * direction);
    }
}


