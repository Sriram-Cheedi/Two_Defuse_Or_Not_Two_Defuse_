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
    
    private Vector3 filteredAcceleration = Vector3.zero;
    
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
        // if (relay != null) relay.RespawnRequested += ResetGravity;
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

    public void SetGravityDirection(Vector3 direction)
    {
        gravityDir = direction;
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
        // if (relay != null) relay.RespawnRequested -= ResetGravity;
    }
    
    public void ProcessAccelerometer(Vector3 accelerometer)
    {
        // Are we upside down relative to world down?
        Debug.Log("ProcessAccelerometer" + (accelerometer.z ));
        if (accelerometer.z < -12f || accelerometer.z > 12f) 
        {
            // Phone is moving too fast! Ignore the input this frame so we don't accidentally flip.
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
