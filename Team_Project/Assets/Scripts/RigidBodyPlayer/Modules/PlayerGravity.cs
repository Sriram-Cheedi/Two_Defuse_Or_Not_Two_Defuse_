using System.Collections;
using RigidBodyPlayer.Modules;
using RigidBodyPlayer.PlayerData;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGravity : MonoBehaviour, IPlayerModule
{
    private RigidBodyMovement Hub;

    [Header("Refs")]
    [SerializeField] private PlayerInputRelay relay;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private PlayerCam playerCam;

    [Header("Rotation")]
    [SerializeField] private float targetRotation = 180f;

    private Vector3 gravityDir = Vector3.down;
    private float gravityMangitude = Physics.gravity.magnitude;
    public bool invertRequested;
    private bool hasInvertRequest;
    
    public void Initialize(RigidBodyMovement hub)
    {
        Hub = hub;
        Hub.rb.useGravity = false;
        if (relay == null) relay = GetComponent<PlayerInputRelay>();
        if (relay != null) relay.RespawnRequested += ResetGravity;
        if (relay == null) relay = GetComponent<PlayerInputRelay>();
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (playerCam == null) playerCam = FindAnyObjectByType<PlayerCam>();
        if (gravityDir == Vector3.zero) gravityDir = Vector3.down;
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

    private void OnEnable()
    {
        
    }

    public void InvertGravityDirection()
    {
        gravityDir = -gravityDir;
    }

    private float GetRelativeVerticalVelocity()
    {
        return Vector3.Dot(Hub.rb.linearVelocity, gravityDir.normalized);
    }

    private float CalculateGravityMagnitude()
    {
        float relativeVerticalVelocity = GetRelativeVerticalVelocity();
        float baseGravityMagnitude = Physics.gravity.magnitude;

        if (!Hub.IsJumpHeld && relativeVerticalVelocity > 0f)
        {
            return baseGravityMagnitude * Hub.data.fallMultiplier;
        }
        
        if (!Hub.IsJumpHeld && relativeVerticalVelocity < 0f)
        {
            return baseGravityMagnitude * Hub.data.jumpCutMultiplier;
        }
        
        if (!Hub.sensor.IsGrounded && Mathf.Abs(relativeVerticalVelocity)  < Hub.data.apexThreshold)
        {
            return baseGravityMagnitude * Hub.data.apexGravityMultiplier;
        }
        
        return baseGravityMagnitude * Hub.data.gravityMultiplier;
    }
    
    private void OnDestroy()
    {
        var relay = GetComponent<PlayerInputRelay>();
        if (relay != null) relay.RespawnRequested -= ResetGravity;
    }
    
    public void ProcessAccelerometer(Vector3 accelerometer, Transform orientation)
    {
        if (orientation == null) return;

        // Are we upside down relative to world down?
        bool isUpsideDown = Vector3.Dot(Vector3.down, -orientation.up) < 0;

        if (accelerometer.z > 9f && isUpsideDown)
            RequestInvert(true);
        else if (accelerometer.z < -9f && !isUpsideDown)
            RequestInvert(true);
        else
            RequestInvert(false);
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

    private void ApexHang()
    {
        float relativeVerticalVelocity = GetRelativeVerticalVelocity();
        if (!Hub.sensor.IsGrounded && Mathf.Abs(relativeVerticalVelocity)  < Hub.data.apexThreshold)
        {
            gravityMangitude = Physics.gravity.magnitude * Hub.data.apexGravityMultiplier;
        }else if (relativeVerticalVelocity > 0f)
        {
            gravityMangitude = Physics.gravity.magnitude * Hub.data.fallMultiplier;
        }
    }

    private void HandleJumpCut()
    {
        
    }

    private void ResetGravity()
    {
        invertRequested = false;
        hasInvertRequest = false;

        gravityDir = Vector3.down;

        Hub.rb.rotation = Quaternion.identity;

        if (cameraPosition != null)
            cameraPosition.localRotation = Quaternion.identity;

        if (playerCam != null)
            playerCam.rollZ = 0f;
    }
}
