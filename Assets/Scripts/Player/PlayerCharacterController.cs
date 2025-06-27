using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerIKController))]
public class PlayerCharacterController : MonoBehaviour
{
    public CharacterController CharacterController { get; private set; }
    public PlayerIKController PlayerIKController { get; private set; }

    [SerializeField] private float gravityValue = -9.81f;

    [SerializeField]
    public float strafeSpeed = 3.0f;

    [SerializeField]
    public float sprintSpeed = 6.0f;

    private Vector3 playerVelocity;

    private void Start()
    {
        CharacterController = GetComponent<CharacterController>();
        PlayerIKController = GetComponent<PlayerIKController>();
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
            playerVelocity.y += gravityValue * Time.deltaTime;
        }

        CharacterController.Move(playerVelocity * Time.deltaTime);
    }

    public void Move(Vector3 direction, float speed)
    {
        CharacterController.Move(speed * Time.deltaTime * direction);
    }
}


