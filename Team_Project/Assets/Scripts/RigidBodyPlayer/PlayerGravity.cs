using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerGravity : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MovementData data;

    [Header("Refs")]
    [SerializeField] private PlayerSensor sensor;
    [SerializeField] private PlayerInputRelay relay;
    [SerializeField] private PlayerInput playerInput;      // kept if you still disable action map (Phase 2 will remove)
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private PlayerCam playerCam;

    [Header("Rotation")]
    [SerializeField] private float targetRotation = 180f;

    private Rigidbody rb;

    public Vector3 gravityDir = Vector3.down;
    public bool invertRequested;
    private bool hasInvertRequest;
    private bool isRotating;

    public bool IsRotating => isRotating;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        if (sensor == null) sensor = GetComponent<PlayerSensor>();
        if (relay == null) relay = GetComponent<PlayerInputRelay>();
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (playerCam == null) playerCam = FindAnyObjectByType<PlayerCam>();
        if (gravityDir == Vector3.zero) gravityDir = Vector3.down;
    }

    private void OnEnable()
    {
        if (relay == null) relay = GetComponent<PlayerInputRelay>();
        if (relay != null) relay.RespawnRequested += ResetGravity;
    }

    private void OnDisable()
    {
        if (relay != null) relay.RespawnRequested -= ResetGravity;
    }
    
    public void ProcessAccelerometer(Vector3 accelerometer, bool canFlip, Transform orientation)
    {
        if (!canFlip || orientation == null) return;

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

    public void UpdateGravity()
    {
        ApplyCustomGravity();
        // InvertKeyBoardGravityDirection();
    }

    private void ApplyCustomGravity()
    {
        if (data == null) return;

        if (sensor == null || !sensor.IsGrounded || !sensor.OnSlope)
        {
            rb.AddForce(gravityDir * 9.81f * data.gravityMultiplier, ForceMode.Acceleration);
        }
    }

    private void InvertGyroGravityDirection()
    {
        if (!hasInvertRequest && invertRequested)
        {
            gravityDir = -gravityDir;
        }

        hasInvertRequest = invertRequested;
    }

    private void InvertKeyBoardGravityDirection()
    {
        gravityDir = -gravityDir;
    }

    // private IEnumerator SmoothRotate()
    // {
    //     if (data == null)
    //         yield break;
    //
    //     isRotating = true;
    //
    //     // Kept for identical behavior. Phase 2 removes this and makes it state-driven.
    //     var playerMap = playerInput != null ? playerInput.actions.FindActionMap("Player") : null;
    //     if (playerMap != null) playerMap.Disable();
    //
    //     float remaining = targetRotation;
    //
    //     while (remaining > 0f)
    //     {
    //         float step = data.rotationSpeed * Time.fixedDeltaTime;
    //         step = Mathf.Min(step, remaining);
    //
    //         Quaternion deltaRotation = Quaternion.Euler(0f, 0f, step);
    //
    //         if (cameraPosition != null)
    //             cameraPosition.Rotate(Vector3.forward, step, Space.Self);
    //
    //         if (playerCam != null)
    //             playerCam.rollZ += step;
    //
    //         rb.MoveRotation(rb.rotation * deltaRotation);
    //
    //         remaining -= step;
    //         yield return new WaitForFixedUpdate();
    //     }
    //
    //     if (playerMap != null) playerMap.Enable();
    //
    //     isRotating = false;
    // }

    private void ResetGravity()
    {

        invertRequested = false;
        hasInvertRequest = false;
        isRotating = false;

        gravityDir = Vector3.down;

        rb.rotation = Quaternion.identity;

        if (cameraPosition != null)
            cameraPosition.localRotation = Quaternion.identity;

        if (playerCam != null)
            playerCam.rollZ = 0f;
    }
}
