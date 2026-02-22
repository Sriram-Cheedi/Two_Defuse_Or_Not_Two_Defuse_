using System;
using RigidBodyPlayer.Modules;
using UnityEngine;

public class PlayerSensor : MonoBehaviour, IPlayerModule
{
    [SerializeField] private CapsuleCollider controller;
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public bool OnMetalSurface { get; private set; }
    public bool OnPlasticSurface { get; private set; }
    public bool OnSlope { get; private set; }
    public bool OnMovingPlatform { get; private set; }
    private RigidBodyMovement Hub;

    public Vector3 ConnectionVelocity { get; private set; }
    public RaycastHit GroundHit => slopeHit;

    public RaycastHit slopeHit;

    private Rigidbody connectedBody;
    private Rigidbody previousConnectedBody;

    private Vector3 connectionWorldPosition;
    private Vector3 connectionLocalPosition;

    public void Initialize(RigidBodyMovement hub)
    {
        if (controller == null) controller = GetComponent<CapsuleCollider>();
        Hub = hub;
    }

    public void Tick()
    {
    }

    public void FixedTick()
    {
        previousConnectedBody = connectedBody;
        GroundCheck();
        UpdateConnectionState();
    }

    public void LateTick()
    {
    }

    public void UpdateSensor(Rigidbody rb)
    {
        
    }

    private void GroundCheck()
    {
        float radius = controller.radius * 0.95f;
        Vector3 origin = controller.bounds.center;
        float rayLength = controller.bounds.extents.y - radius + 0.2f;

        if (Physics.SphereCast(origin, radius, -Hub.rb.transform.up, out RaycastHit hit, rayLength, groundLayer))
        {
            IsGrounded = true;
            slopeHit = hit;
            OnSlope = hit.collider.CompareTag("Slide");
            OnMovingPlatform = hit.collider.CompareTag("MovingPlatform");
            OnMetalSurface = hit.collider.CompareTag("MetalSurface");
            OnPlasticSurface = hit.collider.CompareTag("PlasticSurface");
            connectedBody = hit.rigidbody;
        }
        else
        {
            IsGrounded = false;
            OnSlope = false;
            OnMetalSurface = false;
            OnPlasticSurface = false;
            OnMovingPlatform = false;
            connectedBody = null;
        }
    }

    private void UpdateConnectionState()
    {
        ConnectionVelocity = Vector3.zero;

        if (!IsGrounded || connectedBody == null)
            return;

        if (connectedBody == previousConnectedBody)
        {
            Vector3 newWorldPoint = connectedBody.transform.TransformPoint(connectionLocalPosition);
            Vector3 movement = newWorldPoint - connectionWorldPosition;
            ConnectionVelocity = movement / Time.fixedDeltaTime;
        }

        connectionWorldPosition = Hub.rb.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }
    
}
