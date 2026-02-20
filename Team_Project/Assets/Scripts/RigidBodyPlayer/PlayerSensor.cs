using System;
using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    [SerializeField] private CapsuleCollider controller;
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public bool OnSlope { get; private set; }
    public bool OnMovingPlatform { get; private set; }
    public bool OnFlip { get; private set; }
    public bool InGravityZone;

    public Vector3 ConnectionVelocity { get; private set; }
    public RaycastHit GroundHit => slopeHit;

    public RaycastHit slopeHit;

    private Rigidbody connectedBody;
    private Rigidbody previousConnectedBody;

    private Vector3 connectionWorldPosition;
    private Vector3 connectionLocalPosition;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CapsuleCollider>();
    }

    public void UpdateSensor(Rigidbody rb)
    {
        previousConnectedBody = connectedBody;
        GroundCheck(rb);
        UpdateConnectionState(rb);
    }

    private void GroundCheck(Rigidbody rb)
    {
        float radius = controller.radius * 0.95f;
        Vector3 origin = controller.bounds.center;
        float rayLength = controller.bounds.extents.y - radius + 0.2f;

        if (Physics.SphereCast(origin, radius, -rb.transform.up, out RaycastHit hit, rayLength, groundLayer))
        {
            IsGrounded = true;
            slopeHit = hit;
            OnSlope = hit.collider.CompareTag("Slide");
            OnMovingPlatform = hit.collider.CompareTag("MovingPlatform");
            OnFlip = hit.collider.CompareTag("FlipSection");
            connectedBody = hit.rigidbody;
        }
        else
        {
            IsGrounded = false;
            OnSlope = false;
            OnMovingPlatform = false;
            OnFlip = false;
            connectedBody = null;
        }
    }

    private void UpdateConnectionState(Rigidbody rb)
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

        connectionWorldPosition = rb.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }
}
