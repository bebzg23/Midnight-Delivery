using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset InputActions;
    public CharacterController playerCharacterController;

    private InputAction playerMoveAction;
    private InputAction playerJumpAction;
    private InputAction playerSprintAction;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private Animator animator;
    private Vector2 playerMoveAmount;

    public float playerWalkSpeed = 5.0f;
    public float playerSprintSpeed = 8.5f;
    private float playerRotateDampening = 0.1f;
    private float turnSmoothingVelocity;

    private bool isSprinting;

    private float verticalVelocity = 0f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int IsGroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int VerticalVelocityHash =
        Animator.StringToHash("VerticalVelocity");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        var playerMap = InputActions.FindActionMap("Player");

        playerMoveAction = playerMap.FindAction("Move");
        playerJumpAction = playerMap.FindAction("Jump");

        // Optional: add a "Sprint" action (Left Shift) to your Input Actions.
        // If it doesn't exist, we fall back to reading Left Shift directly.
        playerSprintAction = playerMap.FindAction("Sprint", false);

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        HideMouse();
    }

    void HideMouse()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        playerMoveAmount = playerMoveAction.ReadValue<Vector2>();

        UpdateSprintState();
        PlayerMoveAndRotate();
        Jump();
        UpdateAnimator();
    }

    private void UpdateSprintState()
    {
        bool sprintHeld;

        if (playerSprintAction != null)
        {
            sprintHeld = playerSprintAction.IsPressed();
        }
        else
        {
            sprintHeld = Keyboard.current != null
                && Keyboard.current.leftShiftKey.isPressed;
        }

        // Only sprint while actually moving
        isSprinting = sprintHeld && playerMoveAmount.magnitude > 0.1f;
    }

    private void PlayerMoveAndRotate()
    {
        Vector3 playerDirection = new Vector3(
            playerMoveAmount.x, 0f, playerMoveAmount.y
        ).normalized;

        Vector3 verticalMove =
            new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;

        if (playerDirection.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(playerDirection.x, playerDirection.z)
                * Mathf.Rad2Deg + playerCamera.eulerAngles.y;

            float smoothTargetAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothingVelocity,
                playerRotateDampening
            );

            transform.rotation =
                Quaternion.Euler(0f, smoothTargetAngle, 0f);

            Vector3 moveDirection =
                Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            float currentSpeed =
                isSprinting ? playerSprintSpeed : playerWalkSpeed;

            playerCharacterController.Move(
                moveDirection.normalized * currentSpeed
                * Time.deltaTime + verticalMove
            );
        }
        else
        {
            playerCharacterController.Move(verticalMove);
        }
    }

    private void Jump()
    {
        if (playerCharacterController.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (playerJumpAction.WasPressedThisFrame())
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (animator != null)
                {
                    animator.SetTrigger(JumpHash);
                }
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        // 0 = idle, 0.5 = walk, 1 = sprint
        float targetAnimSpeed = 0f;
        if (playerMoveAmount.magnitude > 0.1f)
            targetAnimSpeed = isSprinting ? 1f : 0.5f;

        animator.SetFloat(SpeedHash, targetAnimSpeed, 0.1f, Time.deltaTime);
        animator.SetBool(
            IsGroundedHash,
            playerCharacterController.isGrounded
        );
        animator.SetFloat(
            VerticalVelocityHash,
            verticalVelocity
        );
    }
}