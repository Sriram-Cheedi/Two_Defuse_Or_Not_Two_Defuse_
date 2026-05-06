using System;
using System.Collections;
using RigidBodyPlayer.Modules;
using RigidBodyPlayer.PlayerData;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGravity : MonoBehaviour, IPlayerModule
{
    private RigidBodyMovement Hub;

    [Header("Refs")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private PlayerCam playerCam;
    
    public bool canFlipGravity;
    private API_Controller_Interface _activeHardware;
    
    private Vector3 filteredAcceleration = Vector3.zero;
    
    // [Header("Rotation")]
    // [SerializeField] private float targetRotation = 180f;

    private Vector3 gravityDir = Vector3.down;
    public bool invertRequested;
    private bool hasInvertRequest;
    
    public void Initialize(RigidBodyMovement hub)
    {
        Hub = hub;
        Hub.rb.useGravity = false;
        // if (relay != null) relay.RespawnRequested += ResetGravity;
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (playerCam == null) playerCam = FindAnyObjectByType<PlayerCam>();
        if (gravityDir == Vector3.zero) gravityDir = Vector3.down;
        if (Hub.relay != null)
        {
            Hub.relay.InvertGravity += HandleGravityInverted;
        }
        _activeHardware = API_Provider.Instance.ActiveAPIController;
        if (_activeHardware != null)
        {
            _activeHardware.OnImuChanged += RotateLevel;
        }
    }

    private void OnEnable()
    {
        SpawnManager.RespawnRequested += ResetGravity;
    }

    private void OnDisable()
    {
        if (_activeHardware != null)
            _activeHardware.OnImuChanged -= RotateLevel;
        if (Hub.relay != null)
        {
            Hub.relay.InvertGravity -= HandleGravityInverted;
        }
        SpawnManager.RespawnRequested -= ResetGravity;
    }

    private void ResetGravity(RespawnData data)
    {
        invertRequested = false;
        if (data.IsUpsideDown)
        {
            SetGravityDirection(Vector3.up);
        }
        else
        {
            SetGravityDirection(Vector3.down);
        }
    }
    
    private void RotateLevel(Vector3 gyro, Vector3 accelerometer)
    {
        ProcessAccelerometer(accelerometer);
    }

    public void Tick()
    {
    }

    public void FixedTick()
    {
        ApplyCustomGravity();
    }

    public void LateTick()
    {
        
    }

    public void InvertGravityDirection()
    {
        gravityDir = -gravityDir;
    }

    public void SetGravityDirection(Vector3 direction)
    {
        gravityDir = direction;
    }
    

    private float CalculateGravityMagnitude()
    {
        float relativeVerticalVelocity = Vector3.Dot(Hub.rb.linearVelocity, gravityDir.normalized);

        //faster fall
        if (!Hub.relay.IsJumpHeld && relativeVerticalVelocity > 0f)
        {
            return Physics.gravity.magnitude * Hub.data.fallMultiplier;
        }
        //var jump height
        if (!Hub.relay.IsJumpHeld && relativeVerticalVelocity < 0f)
        {
            return Physics.gravity.magnitude * Hub.data.jumpCutMultiplier;
        }
        //at apex fall slightly slower
        if (!Hub.sensor.IsGrounded && Mathf.Abs(relativeVerticalVelocity)  < Hub.data.apexThreshold)
        {
            return Physics.gravity.magnitude * Hub.data.apexGravityMultiplier;
        }
        
        return Physics.gravity.magnitude * Hub.data.gravityMultiplier;
    }
    
    public void ProcessAccelerometer(Vector3 accelerometer)
    {
        if (accelerometer.z < -12f || accelerometer.z > 12f) 
        {
            return; 
        }
        float a = Hub.data.gravityAlpha;
        //smooth signal
        if(filteredAcceleration == Vector3.zero) filteredAcceleration = accelerometer;
        else filteredAcceleration = a * accelerometer + (1 - a) * filteredAcceleration;
        Vector3 g = filteredAcceleration.normalized;
        
        bool isUpsideDown = Vector3.Dot(Hub.rb.transform.up, Vector3.down) > 0.95f;
        if (g.z < -0.9f && !isUpsideDown)
        {
            RequestInvert(true);
        }else if (g.z > 0.9f && isUpsideDown)
        {
            RequestInvert(true);
        }
        else
        {
            RequestInvert(false);
        }
    }
    
    
    private void HandleGravityInverted(bool inverted)
    {
        if (canFlipGravity)
        {
            RequestInvert(inverted);
        }
    }

    public void RequestInvert(bool value)
    {
        invertRequested = value;
    }
    
    private void ApplyCustomGravity()
    {
        if (Hub.data == null) return;
        float gravityMagnitude = CalculateGravityMagnitude();
        if (Hub.sensor == null || !Hub.sensor.IsGrounded || Hub.sensor.CurrentSurface != SurfaceType.Slope)
        {
            Hub.rb.AddForce(gravityDir * gravityMagnitude, ForceMode.Acceleration);
        }
    }
}
