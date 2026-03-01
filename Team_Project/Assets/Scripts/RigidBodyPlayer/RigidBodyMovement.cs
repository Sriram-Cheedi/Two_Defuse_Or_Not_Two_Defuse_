using System;
using RigidBodyPlayer.Jump;
using RigidBodyPlayer.Modules;
using RigidBodyPlayer.Movement;
using RigidBodyPlayer.PlayerData;
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
    
    public bool canJump;
    public bool canFlipGravity;
    public Vector3 slopeDir;
    public Vector3 slopeSideWayDir;

    private PlayerBaseState currentState;
    public PlayerGroundState GroundState;
    public PlayerAirborneState AirborneState;
    public PlayerSlopeState SlopeState;
    public PlayerMetalState MetalState;
    public PlayerPlasticState  PlasticState;
    public PlayerFlipState FlipState;
    public PlayerConnectionState ConnectionState;
    
    public IJumpStrategy currentJumpStrategy;
    public IMovementStrategy currentMovementStrategy;

    private IPlayerModule[] modules;

    [Header("Setup")] [SerializeField] private CapsuleCollider controller;

    [Header("Camera")] public Transform orientation;
    [SerializeField] private PlayerCam playerCam;

    [Header("External")]
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
        InitializeModules();
        InitializeStates();
        ValidateDependencies();
    }

    private void InitializeStates()
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
        controller = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        if (relay == null) relay = GetComponent<PlayerInputRelay>();
        if (orientation == null) orientation = transform.Find("Orientation");
        if (playerCam == null) playerCam = FindAnyObjectByType<PlayerCam>();

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void InitializeModules()
    {
        if (sensor == null) sensor = GetComponent<PlayerSensor>();
        if (gravity == null) gravity = GetComponent<PlayerGravity>();
        if (crouch == null) crouch = GetComponent<PlayerCrouch>();
        if (cameraHandler == null) cameraHandler = GetComponent<PlayerCameraHandler>();
        if (jump == null) jump = GetComponent<PlayerJump>();
        
        modules = GetComponentsInChildren<IPlayerModule>();
        foreach (var module in modules)
        {
            module.Initialize(this);
        }
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
        CheckInitialState();
    }

    private void Start()
    {
        if (_activeHardware == null && API_Provider.Instance != null)
        {
            _activeHardware = API_Provider.Instance.ActiveAPIController;
            if (_activeHardware != null)
            {
                _activeHardware.OnImuChanged += RotateLevel;
            }
        }
        CheckInitialState();
        currentState.EnterState();
    }

    public void CheckInitialState()
    {
        if (!sensor.IsGrounded)
        {
            TransitionToState(AirborneState);
            return;
        }

        switch (sensor.CurrentSurface)
        {
            case SurfaceType.Metal:
                TransitionToState(MetalState);
                break;
            case SurfaceType.Plastic:
                TransitionToState(MetalState);
                break;
            case SurfaceType.Slope:
                TransitionToState(SlopeState);
                break;
            default:
                TransitionToState(GroundState);
                break;
        }
    }

    private void OnEnable()
    {
        if (_activeHardware == null)
        {
            _activeHardware = API_Provider.Instance.ActiveAPIController;
        }

        if (_activeHardware != null)
        {
            _activeHardware.OnImuChanged += RotateLevel;
        }


        if (relay != null)
        {
            relay.Move += HandleMove;
            relay.JumpPressed += HandleJump;
            relay.Crouch += HandleCrouch;
            relay.InvertGravity += HandleGravityInverted;
            // relay.RespawnRequested += ResetPlayer;
        }
    }

    public PlayerCam GetPlayerCam()
    {
        return playerCam;
    }

    private void OnDisable()
    {
        if (_activeHardware != null)
            _activeHardware.OnImuChanged -= RotateLevel;

        if (relay != null)
        {
            relay.Move -= HandleMove;
            relay.JumpPressed -= HandleJump;
            relay.Crouch -= HandleCrouch;
            relay.InvertGravity -= HandleGravityInverted;
            // relay.RespawnRequested -= ResetPlayer;
        }
    }

    private void RotateLevel(Vector3 gyro, Vector3 accelerometer)
    {
        Debug.Log("RotateLevel");
        if (gravity == null || sensor == null) return;
        gravity.ProcessAccelerometer(accelerometer);
    }

    // private void ProcessAccelerometer()
    // {
    //     Debug.Log("gyro: " + _activeHardware.getAccelerometerValue);
    //     gravity.ProcessAccelerometer(_activeHardware.getAccelerometerValue, orientation);
    // }

    private void HandleMove(Vector2 value) => cachedMove = value;

    private void HandleCrouch(bool pressed) => isCrouchPressed = pressed;

    private void HandleGravityInverted(bool inverted)
    {
        if (gravity == null) return;
        // Debug.Log("can Flip" + canFlipGravity);
        if (canFlipGravity)
        {
            gravity.RequestInvert(inverted);
        }
    }

    private void HandleJump()
    {
        if (data == null) return;
        jump.SetJumpBuffer();
    }

    public bool IsJumpHeld => relay.IsJumpHeld;

    private void MyInput()
    {
        horizontalInput = cachedMove.x;
        verticalInput = cachedMove.y;
    }

    private void FixedUpdate()
    {
        MyInput();
        foreach (var module in modules)
        {
            module.FixedTick();
        }
        currentState?.FixedUpdateState();
        currentMovementStrategy = MovementStrategyFactory.GetMovementStrategy(this);
        currentMovementStrategy.ApplyMovement(this);
        currentJumpStrategy = MovementStrategyFactory.GetJumpStrategy(this);
        currentJumpStrategy.ExecuteJump(this);
    }

    public void Update()
    {
        foreach (var module in modules)
        {
            module.Tick();
        }
        currentState?.UpdateState();
    }

    public void LateUpdate()
    {
        foreach (var module in modules)
        {
            module.LateTick();
        }
    }

    public void TransitionToState(PlayerBaseState newState)
    {
        if (newState == currentState) return;
        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();
    }
    
}
