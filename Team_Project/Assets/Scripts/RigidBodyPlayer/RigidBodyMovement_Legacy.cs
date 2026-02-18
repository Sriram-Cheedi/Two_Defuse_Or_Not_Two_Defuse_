using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class RigidBodyMovement_Legacy : MonoBehaviour
{
    [Header("Movement")] 
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sideWayMoveSpeed = 30f;
    [SerializeField] private float walkSpeed = 10;
    [SerializeField] private float crouchSpeed = 5f;
    [SerializeField] private float groundAcceleration = 35f;
    
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    
    [Header("Setup")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CapsuleCollider controller;
    
    [Header("Slope")]
    private RaycastHit slopeHit;
    [SerializeField] private float slopeSpeed = 10f;
    private bool readyToJump = true;
    
    [Header("Camera")]
    public Transform orientation;
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private PlayerCam playerCam;
    [SerializeField] private float targetRotation = 180f;
    [SerializeField] private float camDownSpeed = 5;
    private float standCenterY;
    private float standHeight;
    private float crouchHeight;
    private Vector3 crouchOrigin;
    private Vector3 standOrigin;
    
    [Header("Air Control")]
    [SerializeField] private float jumpForce = 11.5f;
    [SerializeField] private float springForce = 45f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airAcceleration = 15;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] float gravityMultiplier = 2.0f;
    
    [Header("External")]
    private API_Provider _API_Provider = API_Provider.Instance;
    private API_Controller_Interface _activeHardware;
    private PlayerInputRelay relay;
    private Vector2 cachedMove;
    
    private Vector3 connectionVelocity;
    private Vector3 connectionWorldPosition;
    private Vector3 connectionLocalPosition;
    private Rigidbody connectedBody;
    private Rigidbody previousConnectedBody;
    public Rigidbody rb;
    
    private bool isCrouchPressed;
    private bool invertGravity;
    private bool hasInvertGravity;
    private bool isRotating;
    private bool onFlip;
    private bool onSlope;
    private bool onMovingPlatform;
    
    private float remainingRotation;
    private float step;
    
    private float horizontalInput;
    private float verticalInput;
    private Vector2 moveInput;

    Vector3 gravityDir = Physics.gravity.normalized;
    Vector3 movementDir;

    private void Awake()
    {

        InitializeComponents();
        
        
        
        
        
        standHeight = controller.height;
        standCenterY = controller.center.y;
        camDownSpeed = 5.0f;
        crouchHeight = controller.height / 2f;
        // _activeHardware = _API_Provider.ActiveAPIController;
        hasInvertGravity = false;
        isRotating = false;
        SetStandCenter();
        SetCrouchCenter();
        relay = GetComponent<PlayerInputRelay>();
        ValidateDependencies();
    }

    private void InitializeComponents()
    {
        controller = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        if (orientation == null) orientation = transform.Find("Orientation");
        if (playerCam == null)  playerCam = FindAnyObjectByType<PlayerCam>();
        if (relay == null) relay = GetComponent<PlayerInputRelay>();
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void ValidateDependencies()
    {
        if (rb == null) Debug.LogError($"RigidBodyPlayer: No RigidBody component attached on {gameObject.name}", this);
        if (controller == null) Debug.LogError($"RigidBodyPlayer: No Controller component attached on {gameObject.name}", this);
        if (playerCam == null) Debug.LogError($"RigidBodyPlayer: No camera found in the scene", this);
        if (orientation == null) Debug.LogError($"RigidBodyPlayer: No orientation component attached", this);
        if (relay == null) Debug.LogError("RigidBodyPlayer: No relay component attached", this);
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
        if (relay != null)
        {
            relay.Move += HandleMove;
            relay.Jump += HandleJump;
            relay.Crouch += HandleCrouch;
            relay.InvertGravity += HandleGravityInverted;
            relay.RespawnRequested += ResetPlayer;
        }
    }

    private void OnDisable()
    {
        if (_activeHardware != null)
            _activeHardware.OnImuChanged -= RotateLevel;
        if (relay != null)
        {
            relay.Move -= HandleMove;
            relay.Jump -= HandleJump;
            relay.Crouch -= HandleCrouch;
            relay.InvertGravity -= HandleGravityInverted;
            relay.RespawnRequested -= ResetPlayer;
        }
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

    private void HandleMove(Vector2 value)
    {
        cachedMove = value;
    }

    private void HandleCrouch(bool pressed)
    {
        isCrouchPressed = pressed;
    }

    private void HandleJump()
    {
        if (readyToJump && isGrounded && !onSlope)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    private void MyInput()
    {
        horizontalInput = cachedMove.x;
        verticalInput = cachedMove.y;
    }
    
    private void HandleGravityInverted(bool inverted)
    {
        invertGravity = inverted;
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
            controller.height = Mathf.Lerp(controller.height, crouchHeight, camDownSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, crouchOrigin, camDownSpeed * Time.deltaTime);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, standHeight, camDownSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, standOrigin, camDownSpeed * Time.deltaTime);
        }
    }

    private void ApplyMovement()
    {
        //get user input
        movementDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        Vector3 desiredRelativeVelocity;
        if (onSlope && isGrounded)
        {
            Vector3 slopDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
            Vector3 sidewayDir = Vector3.Cross(slopeHit.normal, slopDir).normalized;
            movementDir = sidewayDir * horizontalInput;
            desiredRelativeVelocity = movementDir.sqrMagnitude > 0.0001f ? movementDir.normalized * sideWayMoveSpeed : Vector3.zero;
            Vector3 slopVelocity = slopDir * slopeSpeed;
            rb.linearVelocity = desiredRelativeVelocity + slopVelocity;
        }
        else
        {
            Vector3 platformVelocity = (onMovingPlatform && isGrounded) ? connectionVelocity : Vector3.zero;
            Vector3 v = rb.linearVelocity;
            Vector3 relativeVelocity = v - platformVelocity;
            desiredRelativeVelocity = movementDir.sqrMagnitude > 0.0001f ? movementDir.normalized * moveSpeed : Vector3.zero;
            float acceleration = isGrounded ? groundAcceleration : airAcceleration;
            float maxDelta = acceleration * Time.fixedDeltaTime;
            float newX = Mathf.MoveTowards(relativeVelocity.x, desiredRelativeVelocity.x, maxDelta);
            float newZ = Mathf.MoveTowards(relativeVelocity.z, desiredRelativeVelocity.z, maxDelta);
            relativeVelocity = new Vector3(newX, relativeVelocity.y, newZ);
            rb.linearVelocity = relativeVelocity + platformVelocity;
        }
        
    }

    private void StateHandler()
    {
        if (isCrouchPressed && isGrounded)
        {
            moveSpeed = crouchSpeed;
        }else if (isGrounded)
        {
            moveSpeed = walkSpeed;
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
            cameraPosition.Rotate(Vector3.forward, step,Space.Self);
            playerCam.rollZ += step; 
            rb.MoveRotation(rb.rotation * deltaRotation);
            remainingRotation -= step;
            yield return new WaitForFixedUpdate();
        }
        playerMap.Enable();
        isRotating = false;
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
        cameraPosition.transform.localPosition = new Vector3(cameraPosition.transform.localPosition.x, 
            cameraCenterY,cameraPosition.transform.localPosition.z);
    }
    
    private void FixedUpdate()
    {
        previousConnectedBody = connectedBody;
        GroudCheck();
        UpdateConnectionState();
        Spring();
        ApplyCustomGravity();
        
        SetCrouch(isCrouchPressed);
        MyInput();
        StateHandler();
        ApplyMovement();
        InvertGravityDirection();
    }

    private void UpdateConnectionState()
    {
        connectionVelocity = Vector3.zero;
        if (!isGrounded || connectedBody == null)
        {
            return;
        }
        if (connectedBody == previousConnectedBody)
        {
            Vector3 newWorldPoint = connectedBody.transform.TransformPoint(connectionLocalPosition);
            Vector3 movement = newWorldPoint - connectionWorldPosition;
            connectionVelocity = movement / Time.fixedDeltaTime;
        }

        connectionWorldPosition = rb.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
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
            connectedBody = hit.rigidbody;
        }
        else
        {
            isGrounded = false;
            onSlope = false;
            onMovingPlatform = false;
            onFlip = false;
            connectedBody  = null;
        }
    }

    private void LateUpdate()
    {
        CamFollow();
    }
}