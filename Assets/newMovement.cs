using UnityEngine;
using UnityEngine.InputSystem;

public class NewMovement : MonoBehaviour
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
    [SerializeField] private float gravity = -9.81f;

    [Header("Moving Platform Support")]
    [SerializeField] private bool stickToMovingPlatform = true;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Player Visual Fix")]
    public Animator animator;
    public MainCam mainCam;
    [Tooltip("Kéo Model nhân vật vào đây để sửa lỗi lún khi ngồi")]
    [SerializeField] private Transform modelTransform; 
    [SerializeField] private float crouchVisualOffsetY = 0f; 

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isCrouching = false;

    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;

    // Animator hashes
    private static readonly int MoveAmountHash = Animator.StringToHash("moveAmount");
    private static readonly int IsCrouchHash = Animator.StringToHash("IsCrouching");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    // Thêm hash cho biến Grounded để khóa animation trên không
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = normalHeight;
        controller.center = new Vector3(0, normalHeight / 2f, 0);
        velocity = Vector3.zero;
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

    private void FixedUpdate()
    {
        UpdatePlatformAttachment();

        if (stickToMovingPlatform && currentPlatform != null)
        {
            ApplyPlatformDelta();
        }

        PlayerMovement();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // Chỉ cho phép kích hoạt Trigger Jump khi thực sự ở dưới đất
        if (controller.isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetTrigger(JumpHash);
        }
    }

    private void OnCrouchStart(InputAction.CallbackContext context)
    {
        isCrouching = true;
        SetControllerHeight(crouchHeight);
        
        if (modelTransform != null)
            modelTransform.localPosition = new Vector3(0, crouchVisualOffsetY, 0);
    }

    private void OnCrouchStop(InputAction.CallbackContext context)
    {
        isCrouching = false;
        SetControllerHeight(normalHeight);

        if (modelTransform != null)
            modelTransform.localPosition = Vector3.zero;
    }

    private void SetControllerHeight(float height)
    {
        controller.height = height;
        controller.center = new Vector3(0, height / 2f, 0);
    }

    private void UpdatePlatformAttachment()
    {
        if (!stickToMovingPlatform)
        {
            currentPlatform = null;
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float rayDistance = (controller.height * 0.5f) + groundCheckDistance;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, groundLayers))
        {
            if (hit.transform.GetComponentInParent<MovingPlatform>() != null)
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
        Vector3 delta = currentPlatform.position - lastPlatformPosition;
        if (delta.sqrMagnitude > 0f)
        {
            controller.Move(delta);
        }
        lastPlatformPosition = currentPlatform.position;
    }

    private void PlayerMovement()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();
        Vector3 move = mainCam.flatrotation * new Vector3(moveInput.x, 0, moveInput.y).normalized;

        float currentSpeed = isCrouching ? crouchSpeed : speed;
        controller.Move(move * currentSpeed * Time.fixedDeltaTime);

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }

        ApplyGravity();
        UpdateAnimator(move);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.fixedDeltaTime;
        }
        controller.Move(velocity * Time.fixedDeltaTime);
    }

    private void UpdateAnimator(Vector3 move)
    {
        if (animator == null) return;

        animator.SetFloat(MoveAmountHash, move.magnitude, 0.05f, Time.fixedDeltaTime);
        animator.SetBool(IsCrouchHash, isCrouching);
        
        // CẬP NHẬT TRẠNG THÁI CHẠM ĐẤT: 
        // Trong Animator, tạo Transition từ Jump/Fall về Idle khi isGrounded = true
        animator.SetBool(IsGroundedHash, controller.isGrounded);
    }
}