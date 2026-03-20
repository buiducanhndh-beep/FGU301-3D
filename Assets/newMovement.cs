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

    [Header("Moving Platform Support")]
    [SerializeField] private bool stickToMovingPlatform = true;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Player Animation")]
    public Animator animator;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isCrouching = false;
    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;

    // ✅ Matches your Animator parameter exactly
    private static readonly int MoveAmountHash = Animator.StringToHash("moveAmount");
    private static readonly int IsCrouchHash   = Animator.StringToHash("IsCrouching");
    private static readonly int JumpHash       = Animator.StringToHash("Jump");

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
            if (animator.parameters.Length > 0)
                animator.SetTrigger(JumpHash);
        }
    }

    private void OnCrouchStart(InputAction.CallbackContext context)
    {
        Debug.Log("Crouch Start triggered");
        isCrouching = true;
        controller.height = crouchHeight;
        animator.SetBool(IsCrouchHash, true);
    }

    private void OnCrouchStop(InputAction.CallbackContext context)
    {
        Debug.Log("Crouch Stop triggered");
        isCrouching = false;
        controller.height = normalHeight;
        animator.SetBool(IsCrouchHash, false);
    }

    private void FixedUpdate()
    {
        UpdatePlatformAttachment();

        if (stickToMovingPlatform)
        {
            ApplyPlatformDelta();
        }

        PlayerMovement();
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

    void PlayerMovement()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();

        Vector3 move = mainCam.flatrotation * new Vector3(moveInput.x, 0, moveInput.y).normalized;

        float currentSpeed = isCrouching ? crouchSpeed : speed;
        controller.Move(move * currentSpeed * Time.fixedDeltaTime);

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

        UpdateAnimator(move);
    }

    void UpdateAnimator(Vector3 move)
    {
        // ✅ Drives your blend tree: 0 = Idle, 0.5 = Walk, 1 = Run
        float moveAmount = move.magnitude;
        animator.SetFloat(MoveAmountHash, moveAmount, 0.1f, Time.fixedDeltaTime);
    }
}