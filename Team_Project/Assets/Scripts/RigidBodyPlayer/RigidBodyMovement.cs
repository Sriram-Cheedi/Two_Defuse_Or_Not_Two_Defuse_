using System;
using RigidBodyPlayer.Modules;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidBodyMovement : MonoBehaviour
{
    [Header("Data")] [SerializeField] public MovementData data;

    [Header("Modules")] [SerializeField] public PlayerSensor sensor;
    [SerializeField] public PlayerGravity gravity;
    [SerializeField] public PlayerCrouch crouch;
    [SerializeField] public PlayerCameraHandler cameraHandler;
    [SerializeField] public PlayerJump jump;
    [SerializeField] public FootStepSystem footStepSystem;

    [HideInInspector] public PlayerStateMachine stateMachine;

    private IPlayerModule[] modules;

    [Header("Setup")] [SerializeField] private CapsuleCollider controller;

    [Header("Camera")] public Transform orientation;
    [SerializeField] private PlayerCam playerCam;

    [Header("External")]
    public PlayerInputRelay relay;

    public Rigidbody rb;
    

    private void Awake()
    {
        //self setup
        CacheReferences();
        ConfigureRigidBody();
        CacheModules();
    }

    private void OnEnable()
    {
        SpawnManager.RespawnRequested += ResetPlayer;
    }

    private void OnDisable()
    {
        SpawnManager.RespawnRequested -= ResetPlayer;
    }

    private void Start()
    {
        //external wiring
        // InitializeModules();
        InitializeModules();
        
        if (playerCam == null)
            playerCam = FindAnyObjectByType<PlayerCam>();

        ValidateDependencies();
        if (stateMachine != null)
        {
            stateMachine.Initialize(this);
        }
    }

    private void ResetPlayer(RespawnData respawnData)
    {
        rb.isKinematic = true;
        rb.position = respawnData.Position;
        rb.linearVelocity = Vector3.zero;
        if (respawnData.IsUpsideDown)
        {
            rb.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            rb.rotation = Quaternion.Euler(0, 0, 0);
        }
        rb.isKinematic = false;
    }

    private void InitializeComponents()
    {
        controller = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        if (relay == null) relay = GetComponent<PlayerInputRelay>();    
        if (stateMachine == null) stateMachine = GetComponent<PlayerStateMachine>();
        
        if (orientation == null) orientation = transform.Find("Orientation");

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void CacheReferences()
    {
        if (controller == null)  controller = GetComponent<CapsuleCollider>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (relay == null) relay = GetComponent<PlayerInputRelay>();    
        if (stateMachine == null) stateMachine = GetComponent<PlayerStateMachine>();
        
        if (sensor == null) sensor = GetComponent<PlayerSensor>();
        if (gravity == null) gravity = GetComponent<PlayerGravity>();
        if (crouch == null) crouch = GetComponent<PlayerCrouch>();
        if (cameraHandler == null) cameraHandler = GetComponent<PlayerCameraHandler>();
        if (jump == null) jump = GetComponent<PlayerJump>();
        if (orientation == null) orientation = transform.Find("Orientation");
        if (footStepSystem == null) footStepSystem = GetComponent<FootStepSystem>();
    }

    private void ConfigureRigidBody()
    {
        if (rb == null) return;

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    
    private void InitializeModules()
    {
        if (modules == null) return;

        foreach (var module in modules)
        {
            module.Initialize(this);
        }
    }

    
    private void CacheModules()
    {
        modules = GetComponentsInChildren<IPlayerModule>(true);
    }

    // private void InitializeModules()
    // {
    //     if (playerCam == null) playerCam = FindAnyObjectByType<PlayerCam>();
    //     
    //     if (sensor == null) sensor = GetComponent<PlayerSensor>();
    //     if (gravity == null) gravity = GetComponent<PlayerGravity>();
    //     if (crouch == null) crouch = GetComponent<PlayerCrouch>();
    //     if (cameraHandler == null) cameraHandler = GetComponent<PlayerCameraHandler>();
    //     if (jump == null) jump = GetComponent<PlayerJump>();
    //     
    //     modules = GetComponentsInChildren<IPlayerModule>();
    //     foreach (var module in modules)
    //     {
    //         module.Initialize(this);
    //     }
    // }

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
        if (stateMachine == null) Debug.LogError("RigidBodyMovement: No PlayerStateMachine on player", this);
    }
    
    private void FixedUpdate()
    {
        foreach (var module in modules)
        {
            module.FixedTick();
        }
    }

    public void Update()
    {
        foreach (var module in modules)
        {
            module.Tick();
        }
    }

    public void LateUpdate()
    {
        foreach (var module in modules)
        {
            module.LateTick();
        }
    }
    
}
