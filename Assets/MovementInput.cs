
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementInput : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.8f;


    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;

    public MainCam mainCam;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
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

        controller.Move(move * speed * Time.fixedDeltaTime);

        // Rotation follows move direction
        if (move.magnitude > 0)
        {
            transform.rotation = Quaternion.LookRotation(move);
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