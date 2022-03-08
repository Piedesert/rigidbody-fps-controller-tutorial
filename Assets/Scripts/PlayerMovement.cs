using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float playerHeight = 2f;

    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    float movementMultiplier = 10f;

    [Header("Sprinting")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float crouchedSpeed = 2f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    
    [Header("Wall Running/Jumping")]
    [SerializeField] WallRun wallRun;

    /*[Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;*/

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    //float horizontalMovement;
    //float verticalMovement;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;
    public bool isGrounded { get; private set; }

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

    Rigidbody rb;

    RaycastHit slopeHit;

    protected float crouchTimer;
    private bool lerpCrouch;
    private bool isCrouching;

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);

        if (lerpCrouch)
        {
            crouchTimer += Time.fixedDeltaTime;
            float p = crouchTimer / 1;
            p *= p;
            if (isCrouching)
            {
                playerHeight = Mathf.Lerp(playerHeight, 1, p);
                moveSpeed = Mathf.Lerp(moveSpeed, crouchedSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                playerHeight = Mathf.Lerp(playerHeight, 2, p);
                moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
            }

            if (p > 1)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }
    }

    public void Jump()
    {
        if (isGrounded && !wallRun.isWallRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        else if (wallRun.isWallRunning)
        {
            wallRun.WallJump();
        }
    }

    public void Crouch()
    {
        /*if (isSprinting)
        {
            //Perform a slide
        }*/

        isCrouching = !isCrouching;
        crouchTimer = 0;
        lerpCrouch = true;
    }

    public void Sprint()
    {
        if (isGrounded && !isCrouching)
        {
            ControlSpeed(true);
        }
    }

    void ControlSpeed(bool isSprinting)
    {
        if (isGrounded && isSprinting)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    //receive the inputs for our InputManger.cs and apply them to our character controller.
    public void ProcessMove(Vector2 input)
    {
        moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        moveDirection = orientation.forward * moveDirection.z + orientation.right * moveDirection.x;

        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }

        ControlDrag();
    }
}