using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class RigidBodyMovement : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float moveSpeed;
    [SerializeField] private float sideWayMoveSpeed = 20f;
    [SerializeField] public Transform orientation;
    [SerializeField] private float walkSpeed = 10;
    [SerializeField] private float sprintSpeed = 13;
    
    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDrag;
    private bool isGrounded;

    [Header("Crouching")] [SerializeField] private float crouchSpeed = 5f;
    private float standCenterY;
    private float standHeight;
    private float crouchHeight;
    private Vector3 crouchOrigin;
    private Vector3 standOrigin;
    [SerializeField]private PlayerInput playerInput;
    [SerializeField]private Transform cameraPostion;
    private MovePlat movingPlatform;
    
    [Header("Slope")]
    private RaycastHit slopeHit;
    [SerializeField] private float slopeSpeed = 30f;
    
    [SerializeField] private CapsuleCollider controller;

    [SerializeField] private float jumpForce = 11.5f;
    [SerializeField] private float springForce = 45f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airMultiplier = 0.4f;
    private bool readyToJump = true;
    private bool isCrouchPressed;
    [SerializeField] private PlayerCam playerCam;
    
    private float remainingRotation;
    private float step;
    [SerializeField] private float targetRotation = 180f;
    private Vector2 moveInput;
    private float horizontaInput;
    private float verticalInput;
    private bool isSprintPressed;
    private bool onSpring;
    private bool onSlope;
    private bool onMovingPlatform;
    [SerializeField] float gravityMultiplier = 2.0f;
    [SerializeField] private float downSpeed;
    [Header("External")]
    [Header("External")]
    private API_Provider _API_Provider = API_Provider.Instance;
    private API_Controller_Interface _activeHardware;
    [Header("Rotation")] [SerializeField] private float rotationSpeed = 20f;

    Vector3 gravityDir = Physics.gravity.normalized;
    Vector3 movementDir;
    private Vector3 platformVel;
    private bool invertGravity;
    private bool hasInvertGravity;
    private Rigidbody rb;
    private MovementState movementState;
    private bool invertedPlayer;
    private bool isRotating;
    private bool onFlip;
    
    private enum MovementState
    {
        Walking,
        Couching,
        Sprinting,
        Air,
    }

    private void Awake()
    {
        controller = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        standHeight = controller.height;
        standCenterY = controller.center.y;
        downSpeed = 5.0f;
        crouchHeight = controller.height / 2f;
        _activeHardware = _API_Provider.ActiveAPIController;
        hasInvertGravity = false;
        isRotating = false;
        SetStandCenter();
        SetCrouchCenter();
    }

    public void ResetPlayer()
    {
        hasInvertGravity = false;
        invertGravity = false;
        isRotating = false;
        playerCam.rollZ = 0;
        orientation.rotation = Quaternion.Euler(0, 0, 0);
        rb.rotation = Quaternion.Euler(0, 0, 0);
        rb.linearVelocity = Vector3.zero; 
        gravityDir = Vector3.down;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.useGravity = false;
        rb.freezeRotation = true;
        moveSpeed = walkSpeed;
    }
    private void OnEnable()
    {
        if (_activeHardware != null)
            _activeHardware.OnImuChanged += RotateLevel;
    }

    private void OnDisable()
    {
        if (_activeHardware != null)
            _activeHardware.OnImuChanged -= RotateLevel;
    }
    
    private void RotateLevel(Vector3 gyro, Vector3 accelerometer)
    {

        // Flip gravity direction (so jump/gravity math stays consistent with sign)
        // gravity = -gravity;
        // Rotate the player (or level) around Z, as you had
        if (accelerometer.z > 9f && Vector3.Dot(Vector3.down,-orientation.transform.up) < 0 && onFlip)
        {
            invertGravity = true;
        }
        else if(accelerometer.z < -9f && Vector3.Dot(Vector3.down,-orientation.transform.up) > 0 && onFlip)
        {
            invertGravity = true;
        }
        else
        {
            invertGravity = false;
        }
    }

    private void MyInput()
    {
        horizontaInput = moveInput.x;
        verticalInput = moveInput.y;
    }
    
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            isCrouchPressed = true;
        }

        if (ctx.canceled)
        {
            isCrouchPressed = false;
        }
    }

    public void OnInvertGravity(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            invertGravity = true;
        }

        if (ctx.canceled)
        {
            invertGravity = false;
        }
    }
    
    private void Spring()
    {
        
        if (Physics.Raycast(controller.bounds.center, Vector3.down, out RaycastHit hit,controller.bounds.extents.y + 0.2f, groundLayer)
            && hit.collider.CompareTag("Spring"))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(transform.up * springForce, ForceMode.Impulse);
        }
    }

    private void SetCrouchCenter()
    {
        float bottomLocal = standCenterY - (standHeight * 0.5f);
        float newCenterY = bottomLocal + crouchHeight / 2f;
        crouchOrigin = new Vector3(controller.center.x, newCenterY, controller.center.z);
    }

    private void SetStandCenter()
    {
        float bottomLocal = standCenterY - (standHeight * 0.5f);
        float newCenterY = bottomLocal + standHeight / 2f;
        standOrigin = new Vector3(controller.center.x, newCenterY, controller.center.z);
    }
    
    private void SetCrouch(bool isCrouch)
    {

        if (isCrouch)
        {
            controller.height = Mathf.Lerp(controller.height, crouchHeight, downSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, crouchOrigin, downSpeed * Time.deltaTime);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, standHeight, downSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, standOrigin, downSpeed * Time.deltaTime);
        }
    }

    private void MovePlayer()
    {
        movementDir = orientation.forward * verticalInput + orientation.right * horizontaInput;
        if (onSlope && isGrounded)
        {
            Vector3 slopDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
            Vector3 sidewayDir = Vector3.Cross(slopeHit.normal, slopDir).normalized;
            movementDir = sidewayDir * horizontaInput;
            rb.AddForce(slopDir * slopeSpeed * 10f, ForceMode.Force);
            rb.AddForce(movementDir * sideWayMoveSpeed * 10f, ForceMode.Force);
        }else if (isGrounded)
        {
            rb.AddForce(movementDir.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(movementDir.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
        
    }

    private void StateHandler()
    {
        if (isGrounded && isSprintPressed)
        {
            movementState = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
        }else if (isCrouchPressed && isGrounded)
        {
            movementState = MovementState.Couching;
            moveSpeed = crouchSpeed;
        }else if (isGrounded)
        {
            movementState = MovementState.Walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            movementState = MovementState.Air;
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f,  rb.linearVelocity.z);
        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y,  limitedVelocity.z);
        }
    }

    private void ApplyCustomGravity()
    {
        if (!isGrounded || !onSlope)
        {
            rb.AddForce(gravityDir * 9.81f * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    private void InvertGravityDirection()
    {
        if (isRotating)
        {
            return;
        }
        if (!hasInvertGravity && invertGravity)
        {
            gravityDir = -gravityDir;
            StartCoroutine(SmoothRotate());
        }

        hasInvertGravity = invertGravity;
    }
    
    IEnumerator SmoothRotate()
    {
        isRotating = true;
        var playerMap = playerInput.actions.FindActionMap("Player");
        playerMap.Disable();
        remainingRotation = targetRotation;
        while (remainingRotation > 0)
        {
            step = rotationSpeed * Time.fixedDeltaTime;
            step = Mathf.Min(step, remainingRotation);
            Quaternion deltaRotation = Quaternion.Euler(0, 0, step);
            cameraPostion.Rotate(Vector3.forward, step,Space.Self);
            playerCam.rollZ += step; 
            rb.MoveRotation(rb.rotation * deltaRotation);
            remainingRotation -= step;
            yield return new WaitForFixedUpdate();
        }
        playerMap.Enable();
        isRotating = false;
    }
    
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && readyToJump && isGrounded && !onSlope)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isSprintPressed = true;
        }

        if (ctx.canceled)
        {
            isSprintPressed = false;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
    private void CamFollow()
    {
        float bottomLocal = controller.center.y - controller.height / 2.0f;
        float cameraCenterY = bottomLocal + controller.height;
        cameraPostion.transform.localPosition = new Vector3(cameraPostion.transform.localPosition.x, 
            cameraCenterY,cameraPostion.transform.localPosition.z);
    }

    private void MoveWithPlat()
    {
        movementDir = orientation.forward * verticalInput + orientation.right * horizontaInput;
        if (onMovingPlatform && isGrounded)
        {
            platformVel = movingPlatform.Velocity;
            
            rb.linearVelocity = new Vector3(
                platformVel.x + rb.linearVelocity.x, 
                rb.linearVelocity.y, 
                platformVel.z + rb.linearVelocity.z
            );
            platformVel = Vector3.zero;
        }
    }
    
    private void FixedUpdate()
    {
        GroudCheck();
        Spring();
        ApplyCustomGravity();
        
        SetCrouch(isCrouchPressed);
        MovePlayer();
        SpeedControl();
        MoveWithPlat();
        InvertGravityDirection();
        rb.linearDamping = isGrounded ? groundDrag : 0f;
    }

    private void GroudCheck()
    {
        Vector3 origin = controller.bounds.center;
        float rayLength = controller.bounds.extents.y + 0.2f;

        if (Physics.Raycast(origin, -rb.transform.up, out RaycastHit hit, rayLength, groundLayer))
        {
            isGrounded = true;
            slopeHit = hit;
            onSlope = hit.collider.CompareTag("Slide");
            onMovingPlatform = hit.collider.CompareTag("MovingPlatform");
            onFlip = hit.collider.CompareTag("FlipSection");
        }
        else
        {
            isGrounded = false;
            onSlope = false;
            onMovingPlatform = false;
            onFlip = false;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        MyInput();
        StateHandler();
    }

    private void LateUpdate()
    {
        CamFollow();
    }
}
