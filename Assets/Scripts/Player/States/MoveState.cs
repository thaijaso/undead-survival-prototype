using UnityEngine;

public class MoveState : PlayerState
{
    float playerRotationSpeedNotAiming = 5f;
    float playerRotationSpeedAiming = 40f;

    public MoveState(
        Player player,
        StateMachine<PlayerState> stateMachine,
        AnimationManager animationManager,
        string animationName
    ) : base(
        player,
        stateMachine,
        animationManager,
        animationName
    )
    { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        Vector3 direction = player.PlayerInput.GetInputDirection();
        animationManager.SetMoveParams(direction.x, direction.z);

        player.PlayerIKController.BlendIKWeights();
    }

    protected void HandleMovement(float speed, bool faceMoveDirection)
    {
        // 1. Get input direction
        Vector3 direction = player.PlayerInput.GetInputDirection();

        // 2. Get camera transform (choose follow or aim camera as needed)
        Transform cameraTransform = player.PlayerCameraController.GetFollowCamTransform();

        // 3. Flatten camera vectors
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 4. Prevent diagonal speed boost
        direction = Vector3.ClampMagnitude(direction, 1f);

        // 5. Calculate movement direction relative to camera
        Vector3 moveDirection = cameraForward * direction.z + cameraRight * direction.x;

        if (direction.z < 0) // Player is trying to move backwards
        {
            // Cast a ray from chest height behind the player to check for actual walls
            Vector3 rayStart = player.transform.position + Vector3.up * 1f; // Start from chest height
            Vector3 back = -player.transform.forward;
            
            if (Physics.Raycast(rayStart, back, out RaycastHit wallHit, 0.8f))
            {
                float wallAngle = Vector3.Angle(wallHit.normal, Vector3.up);
                Debug.Log($"[MoveState] Wall raycast hit: {wallHit.collider.name}, angle: {wallAngle:F1}");
                if (wallAngle > 45f) // Only walls steeper than 45 degrees
                {
                    float dotProduct = Vector3.Dot(moveDirection.normalized, -wallHit.normal);
                    Debug.Log($"[MoveState] Wall is steep enough. Dot product with moveDirection: {dotProduct:F2}");
                    if (dotProduct > 0.1f) // Only if we're moving somewhat towards the wall
                    {
                        Debug.Log($"[MoveState] Wall sliding triggered! Sliding along: {wallHit.collider.name} at point {wallHit.point}");
                        // Project moveDirection onto the plane of the wall's normal to allow sliding along the wall
                        moveDirection = Vector3.ProjectOnPlane(moveDirection, wallHit.normal);
                        
                        // Debug visualization for wall detection
                        Debug.DrawRay(rayStart, back * 0.8f, Color.yellow);
                        Debug.DrawRay(wallHit.point, wallHit.normal, Color.magenta);
                    }
                }
            }
        }

        // 6. Slope adjustment
        if (Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit, 1.5f))
        {
            Debug.DrawRay(player.transform.position, hit.normal, Color.red);
            Debug.DrawRay(player.transform.position, moveDirection, Color.green); 
            moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
        }

        // 7. Player rotation
        Quaternion targetRotation = faceMoveDirection
            ? Quaternion.LookRotation(moveDirection)
            : Quaternion.LookRotation(cameraForward);

        float rotationSpeed = player.PlayerInput.IsAiming
            ? playerRotationSpeedAiming
            : playerRotationSpeedNotAiming;

        player.transform.rotation = Quaternion.Slerp(
            player.transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
        
        // 8. Move the player
        player.PlayerCharacterController.Move(moveDirection, speed);

        // 9. Debug lines for visualization
        Debug.DrawLine(player.transform.position, player.transform.position + cameraForward * 2f, Color.blue);
        Debug.DrawLine(player.transform.position, player.transform.position + cameraRight * 2f, Color.red);
        Debug.DrawLine(player.transform.position, player.transform.position + moveDirection * 2f, Color.green);
    }
}
