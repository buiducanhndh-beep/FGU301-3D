using UnityEngine;
using UnityEngine.InputSystem;

public class newMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private float speed = 5f;

    [Header("Jump")]
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Crouch")]
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Physics")]
    [SerializeField] private float gravity = -9.8f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isCrouching = false;

    public MainCam mainCam;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = normalHeight;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        crouchAction.action.Enable();

        jumpAction.action.performed += OnJump;
        crouchAction.action.performed += OnCrouchStart;
        crouchAction.action.canceled += OnCrouchStop;
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        crouchAction.action.Disable();

        jumpAction.action.performed -= OnJump;
        crouchAction.action.performed -= OnCrouchStart;
        crouchAction.action.canceled -= OnCrouchStop;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (controller.isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void OnCrouchStart(InputAction.CallbackContext context)
    {
    Debug.Log("Crouch Start triggered");

        isCrouching = true;
        controller.height = crouchHeight;
        // Move controller down so feet stay on ground
        
    }

    private void OnCrouchStop(InputAction.CallbackContext context)
    {

        Debug.Log("Crouch Stop triggered");
        isCrouching = false;
        controller.height = normalHeight;
        
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    void PlayerMovement()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();

        // Apply camera Y rotation to movement direction
        Vector3 move = mainCam.flatrotation * new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // Use crouch speed if crouching
        float currentSpeed = isCrouching ? crouchSpeed : speed;
        controller.Move(move * currentSpeed * Time.fixedDeltaTime);

        // Smooth rotation
        if (move.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }

        // Gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.fixedDeltaTime;
        controller.Move(velocity * Time.fixedDeltaTime);
    }
}