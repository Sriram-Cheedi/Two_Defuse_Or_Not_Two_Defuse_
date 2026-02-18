using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class RigidBodyMovement : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MovementData data;

    [Header("Modules")]
    [SerializeField] private PlayerSensor sensor;
    [SerializeField] private PlayerGravity gravity;
    [SerializeField] private PlayerCrouch crouch;

    [Header("Movement (runtime)")]
    [SerializeField] private float moveSpeed; // runtime (set from data)

    [Header("Setup")]
    [SerializeField] private CapsuleCollider controller;

    [Header("Camera")]
    public Transform orientation;
    [SerializeField] private PlayerCam playerCam;

    [Header("External")]
    private API_Provider _API_Provider = API_Provider.Instance;
    private API_Controller_Interface _activeHardware;
    private PlayerInputRelay relay;

    public Rigidbody rb;

    private Vector2 cachedMove;
    private bool isCrouchPressed;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 movementDir;
    private bool readyToJump = true;

    private void Awake()
    {
        InitializeComponents();
        relay = GetComponent<PlayerInputRelay>();
        ValidateDependencies();
    }

    private void InitializeComponents()
    {
        if (sensor == null) sensor = GetComponent<PlayerSensor>();
        if (gravity == null) gravity = GetComponent<PlayerGravity>();
        if (crouch == null) crouch = GetComponent<PlayerCrouch>();

        controller = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

        if (orientation == null) orientation = transform.Find("Orientation");
        if (playerCam == null) playerCam = FindAnyObjectByType<PlayerCam>();
        if (relay == null) relay = GetComponent<PlayerInputRelay>();

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void ValidateDependencies()
    {
        if (data == null) Debug.LogError("RigidBodyMovement: MovementData not assigned!", this);
        if (rb == null) Debug.LogError($"RigidBodyMovement: No Rigidbody on {gameObject.name}", this);
        if (controller == null) Debug.LogError($"RigidBodyMovement: No CapsuleCollider on {gameObject.name}", this);
        if (orientation == null) Debug.LogError("RigidBodyMovement: No Orientation child found", this);
        if (relay == null) Debug.LogError("RigidBodyMovement: No PlayerInputRelay on player", this);
        if (gravity == null) Debug.LogError("RigidBodyMovement: No PlayerGravity on player", this);
        if (sensor == null) Debug.LogError("RigidBodyMovement: No PlayerSensor on player", this);
        if (crouch == null) Debug.LogError("RigidBodyMovement: No PlayerCrouch on player", this);
    }

    public void ResetPlayer()
    {
        if (playerCam != null) playerCam.rollZ = 0f;
        if (orientation != null) orientation.rotation = Quaternion.Euler(0, 0, 0);
        rb.linearVelocity = Vector3.zero;
        readyToJump = true;
    }

    private void Start()
    {
        rb.useGravity = false;
        rb.freezeRotation = true;

        if (data != null) moveSpeed = data.walkSpeed;
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
        if (gravity == null || sensor == null) return;
        gravity.ProcessAccelerometer(accelerometer, sensor.OnFlip, orientation);
    }

    private void HandleMove(Vector2 value) => cachedMove = value;

    private void HandleCrouch(bool pressed) => isCrouchPressed = pressed;

    private void HandleGravityInverted(bool inverted)
    {
        if (gravity != null) gravity.RequestInvert(inverted);
    }

    private void HandleJump()
    {
        if (data == null) return;

        if (readyToJump && sensor.IsGrounded && !sensor.OnSlope)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), data.jumpCooldown);
        }
    }

    private void MyInput()
    {
        horizontalInput = cachedMove.x;
        verticalInput = cachedMove.y;
    }

    private void ApplyMovement()
    {
        if (data == null) return;

        bool isGrounded = sensor.IsGrounded;
        bool onSlope = sensor.OnSlope;
        bool onMovingPlatform = sensor.OnMovingPlatform;
        Vector3 connectionVelocity = sensor.ConnectionVelocity;
        RaycastHit slopeHit = sensor.GroundHit;

        movementDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (onSlope && isGrounded)
        {
            Vector3 slopDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
            Vector3 sidewayDir = Vector3.Cross(slopeHit.normal, slopDir).normalized;

            movementDir = sidewayDir * horizontalInput;

            Vector3 desiredRelativeVelocity = movementDir.sqrMagnitude > 0.0001f
                ? movementDir.normalized * data.sideWayMoveSpeed
                : Vector3.zero;

            Vector3 slopVelocity = slopDir * data.slopeSpeed;
            rb.linearVelocity = desiredRelativeVelocity + slopVelocity;
        }
        else
        {
            Vector3 platformVelocity = (onMovingPlatform && isGrounded) ? connectionVelocity : Vector3.zero;

            Vector3 v = rb.linearVelocity;
            Vector3 relativeVelocity = v - platformVelocity;

            Vector3 desiredRelativeVelocity = movementDir.sqrMagnitude > 0.0001f
                ? movementDir.normalized * moveSpeed
                : Vector3.zero;

            float acceleration = isGrounded ? data.groundAcceleration : data.airAcceleration;
            float maxDelta = acceleration * Time.fixedDeltaTime;

            float newX = Mathf.MoveTowards(relativeVelocity.x, desiredRelativeVelocity.x, maxDelta);
            float newZ = Mathf.MoveTowards(relativeVelocity.z, desiredRelativeVelocity.z, maxDelta);

            relativeVelocity = new Vector3(newX, relativeVelocity.y, newZ);
            rb.linearVelocity = relativeVelocity + platformVelocity;
        }
    }

    private void StateHandler()
    {
        if (data == null) return;

        if (isCrouchPressed && sensor.IsGrounded)
            moveSpeed = data.crouchSpeed;
        else if (sensor.IsGrounded)
            moveSpeed = data.walkSpeed;
    }

    private void Jump()
    {
        if (data == null) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * data.jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() => readyToJump = true;

    private void FixedUpdate()
    {
        sensor.UpdateSensor(rb);

        if (gravity != null) gravity.UpdateGravity();
        if (crouch != null) crouch.Tick(isCrouchPressed);

        MyInput();
        StateHandler();
        ApplyMovement();
    }
}
