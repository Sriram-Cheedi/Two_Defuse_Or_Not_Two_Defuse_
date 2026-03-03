using System;
using RigidBodyPlayer.Modules;
using RigidBodyPlayer.PlayerData;
using UnityEngine;

public class PlayerSensor : MonoBehaviour, IPlayerModule
{
    [SerializeField] private CapsuleCollider controller;
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public bool OnMovingPlatform { get; private set; }
    public bool OnSlope { get; private set; }
    private RigidBodyMovement Hub;

    public SurfaceType CurrentSurface { get; private set; }
    private SurfaceType PreviousSurface;

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
        switch (CurrentSurface)
        {
            case SurfaceType.Metal:
                Debug.Log("Current Surface : Metal");
                break;
            case SurfaceType.Plastic:
                Debug.Log("Current Surface : Plastic");
                break;
            case SurfaceType.SemiConductor:
                Debug.Log("Current Surface : SemiConductor");
                break;
            default:
                Debug.Log("Current Surface : Default");
                break;
        }
        
    }

    public SurfaceType GetPreviousSurface() => PreviousSurface;

    public void SetPreviousSurface(SurfaceType value)
    {
        PreviousSurface = value;    
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

    private void GroundCheck()
    {
        float radius = controller.radius * 0.95f;
        Vector3 origin = controller.bounds.center;
        float rayLength = controller.bounds.extents.y - radius + 0.2f;

        if (Physics.SphereCast(origin, radius, -Hub.rb.transform.up, out RaycastHit hit, rayLength, groundLayer, QueryTriggerInteraction.Ignore))
        {
            IsGrounded = true;
            slopeHit = hit;
            connectedBody = hit.rigidbody;
            if (hit.collider.TryGetComponent(out SurfaceIdentity identity))
            {
                CurrentSurface = identity.type;
                OnMovingPlatform = identity.isMovingPlatform;
            }
            else
            {
                CurrentSurface = SurfaceType.Default;
                OnMovingPlatform = false;
            }
        }
        else
        {
            CurrentSurface = SurfaceType.Default;
            IsGrounded = false;
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
