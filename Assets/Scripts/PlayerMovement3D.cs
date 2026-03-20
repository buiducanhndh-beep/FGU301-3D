using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 6.5f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.25f;
    [SerializeField] private float gravity = -20f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Moving Platform Support")]
    [SerializeField] private bool stickToMovingPlatform = true;

    private CharacterController controller;
    private float verticalVelocity;

    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        UpdatePlatformAttachment();

        if (stickToMovingPlatform)
        {
            ApplyPlatformDelta();
        }

        MovePlayer();
    }

    private void MovePlayer()
    {
        bool grounded = IsGrounded();

        if (grounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        Vector2 input = ReadMoveInput();
        bool isSprinting = ReadSprintInput();
        bool jumpPressed = ReadJumpPressed();

        Vector3 moveDirection = GetMoveDirection(input);
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;

        controller.Move(moveDirection * targetSpeed * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (jumpPressed && grounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private Vector3 GetMoveDirection(Vector2 input)
    {
        Vector3 inputDirection = new Vector3(input.x, 0f, input.y);
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        if (inputDirection.sqrMagnitude < 0.001f)
        {
            return Vector3.zero;
        }

        if (cameraTransform == null)
        {
            return transform.TransformDirection(inputDirection);
        }

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * inputDirection.z + camRight * inputDirection.x;
        return moveDirection.normalized;
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        float rayDistance = controller.skinWidth + groundCheckDistance + 0.05f;

        return Physics.SphereCast(origin, controller.radius * 0.9f, Vector3.down, out _, rayDistance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void UpdatePlatformAttachment()
    {
        if (!stickToMovingPlatform)
        {
            currentPlatform = null;
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float rayDistance = controller.height * 0.6f + groundCheckDistance;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.GetComponentInParent<MovingPlatfor>() != null)
            {
                if (currentPlatform != hit.transform)
                {
                    currentPlatform = hit.transform;
                    lastPlatformPosition = currentPlatform.position;
                }

                return;
            }
        }

        currentPlatform = null;
    }

    private void ApplyPlatformDelta()
    {
        if (currentPlatform == null)
        {
            return;
        }

        Vector3 platformDelta = currentPlatform.position - lastPlatformPosition;

        if (platformDelta.sqrMagnitude > 0f)
        {
            controller.Move(platformDelta);
        }

        lastPlatformPosition = currentPlatform.position;
    }

    private static Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            float x = 0f;
            float y = 0f;

            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;
            if (Keyboard.current.wKey.isPressed) y += 1f;

            return new Vector2(x, y);
        }
#endif

        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private static bool ReadJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.spaceKey.wasPressedThisFrame;
        }
#endif

        return Input.GetButtonDown("Jump");
    }

    private static bool ReadSprintInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        }
#endif

        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
}
