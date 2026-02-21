using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class RigidBodyMovement : MonoBehaviour
{
    [Header("Data")] [SerializeField] public MovementData data;

    [Header("Modules")] [SerializeField] public PlayerSensor sensor;
    [SerializeField] public PlayerGravity gravity;
    [SerializeField] private PlayerCrouch crouch;
    [SerializeField] public PlayerCameraHandler cameraHandler;
    [SerializeField] public PlayerJump jump;
    
    public bool CanJump;
    public bool CanFlipGravity;

    private PlayerBaseState currentState;
    public PlayerGroundState GroundState;
    public PlayerAirborneState AirborneState;
    public PlayerSlopeState SlopeState;
    public PlayerMetalState MetalState;
    public PlayerPlasticState  PlasticState;
    public PlayerFlipState FlipState;
    public PlayerConnectionState ConnectionState;

    [Header("Movement (runtime)")] [SerializeField]
    public float moveSpeed; // runtime (set from data)

    [Header("Setup")] [SerializeField] private CapsuleCollider controller;

    [Header("Camera")] public Transform orientation;
    [SerializeField] private PlayerCam playerCam;

    [Header("External")] private API_Provider _API_Provider = API_Provider.Instance;
    private API_Controller_Interface _activeHardware;
    private PlayerInputRelay relay;

    public Rigidbody rb;

    private Vector2 cachedMove;
    public bool isCrouchPressed;

    public float horizontalInput;
    public float verticalInput;

    public Vector3 movementDir;
    private bool readyToJump = true;

    private void Awake()
    {
        InitializeComponents();
        InitiaizeStates();
        relay = GetComponent<PlayerInputRelay>();
        
        ValidateDependencies();
    }

    private void InitiaizeStates()
    {
        GroundState = new PlayerGroundState(this);
        AirborneState = new PlayerAirborneState(this);
        SlopeState = new PlayerSlopeState(this);
        FlipState = new PlayerFlipState(this);
        MetalState = new PlayerMetalState(this);
        PlasticState = new PlayerPlasticState(this);
        ConnectionState = new PlayerConnectionState(this);
    }

    private void InitializeComponents()
    {
        if (sensor == null) sensor = GetComponent<PlayerSensor>();
        if (gravity == null) gravity = GetComponent<PlayerGravity>();
        if (crouch == null) crouch = GetComponent<PlayerCrouch>();
        if(cameraHandler == null) cameraHandler = GetComponent<PlayerCameraHandler>();
        if (jump == null) jump = GetComponent<PlayerJump>();

        controller = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        // _activeHardware = _API_Provider.ActiveAPIController;
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
        if (jump == null)  Debug.LogError("RigidBodyMovement: No PlayerJump on player", this);
    }

    public void ResetPlayer()
    {
        if (playerCam != null) playerCam.rollZ = 0f;
        if (orientation != null) orientation.rotation = Quaternion.Euler(0, 0, 0);
        rb.linearVelocity = Vector3.zero;
        readyToJump = true;
        CheckInitialState();
    }

    private void Start()
    {
        rb.useGravity = false;
        rb.freezeRotation = true;
        CheckInitialState();
        currentState.EnterState();
        if (data != null) moveSpeed = data.walkSpeed;
    }

    private void CheckInitialState()
    {
        if (sensor.OnMetalSurface)
        {
            currentState = MetalState;
        }
        if (!sensor.IsGrounded)
        {
            currentState = AirborneState;
        }

        if (sensor.IsGrounded)
        {
            currentState = GroundState;
        }
        if (sensor.OnPlasticSurface)
        {
            currentState = PlasticState;
        }
    }

    private void OnEnable()
    {

        if (_activeHardware != null)
        {
            _activeHardware.OnImuChanged += RotateLevel;
        }


        if (relay != null)
        {
            relay.Move += HandleMove;
            relay.JumpPressed += HandleJump;
            relay.JumpCanceled += HandleJumpCut;
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
            relay.JumpPressed -= HandleJump;
            relay.JumpCanceled -= HandleJumpCut;
            relay.Crouch -= HandleCrouch;
            relay.InvertGravity -= HandleGravityInverted;
            relay.RespawnRequested -= ResetPlayer;
        }
    }

    private void RotateLevel(Vector3 gyro, Vector3 accelerometer)
    {
        Debug.Log("RotateLevel");
        if (gravity == null || sensor == null) return;
        gravity.ProcessAccelerometer(accelerometer, sensor.InGravityZone, orientation);
    }

    private void HandleMove(Vector2 value) => cachedMove = value;

    private void HandleCrouch(bool pressed) => isCrouchPressed = pressed;

    private void HandleGravityInverted(bool inverted)
    {
        if (gravity == null) return;
        Debug.Log("can Flip" + CanFlipGravity);
        if (CanFlipGravity)
        {
            gravity.RequestInvert(inverted);
        }
    }

    private void HandleJump()
    {
        if (data == null) return;
        jump.SetJumpBuffer();
        Debug.Log("ready to jump" + jump.readyToJump + "coyote"+ jump.coyoteTimeCounter +"buffer"+  jump.jumpBufferCounter + "canjump" + CanJump);
        
        if (jump.readyToJump && jump.coyoteTimeCounter > 0f && jump.jumpBufferCounter > 0f && CanJump)
        {
            jump.ExecuteJump();
        }
    }

    private void HandleJumpCut()
    {
        jump.HandleJumpCut();
    }

    private void MyInput()
    {
        horizontalInput = cachedMove.x;
        verticalInput = cachedMove.y;
    }

    public static float GetVerticalVelocity(Rigidbody rb, Vector3 upDirection) {
        return Vector3.Dot(rb.linearVelocity, upDirection);
    }
    
    public void ApplyMovement(float targetSpeed, float acceleration, bool connected)
    {
        if (data == null) return;
        Vector3 connectionVelocity = sensor.ConnectionVelocity;

        movementDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        Vector3 platformVelocity = connected ? connectionVelocity : Vector3.zero;

        Vector3 v = rb.linearVelocity;
        Vector3 relativeVelocity = v - platformVelocity;

        Vector3 desiredRelativeVelocity = movementDir.sqrMagnitude > 0.0001f
            ? movementDir.normalized * targetSpeed
            : Vector3.zero;
        
        float maxDelta = acceleration * Time.fixedDeltaTime;

        float newX = Mathf.MoveTowards(relativeVelocity.x, desiredRelativeVelocity.x, maxDelta);
        float newZ = Mathf.MoveTowards(relativeVelocity.z, desiredRelativeVelocity.z, maxDelta);

        relativeVelocity = new Vector3(newX, relativeVelocity.y, newZ);
        rb.linearVelocity = relativeVelocity + platformVelocity;
    }

    public void ApplySlopeMovement(Vector3 surfaceNormal, float horizontalInput)
    {
        Vector3 slopDir = Vector3.ProjectOnPlane(Vector3.down, surfaceNormal).normalized;
        Vector3 sidewayDir = Vector3.Cross(surfaceNormal, slopDir).normalized;

        movementDir = sidewayDir * horizontalInput;

        Vector3 desiredRelativeVelocity = movementDir.sqrMagnitude > 0.0001f
            ? movementDir.normalized * data.sideWayMoveSpeed
            : Vector3.zero;

        Vector3 slopVelocity = slopDir * data.slopeSpeed;
        rb.linearVelocity = desiredRelativeVelocity + slopVelocity;
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
        gravity.UpdateGravity();
        crouch.UpdateCrouch(isCrouchPressed);
        MyInput();
        currentState.FixedUpdateState();
    }

    public void Update()
    {
        jump.UpdateJumpState();
        currentState.UpdateState();
    }

    public void TransitionToState(PlayerBaseState newState)
    {
        if (newState == currentState) return;
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
    
}
