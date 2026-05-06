using UnityEngine;

namespace RigidBodyPlayer.Movement
{
    public class SlopeMovementStrategy : IMovementStrategy
    {
        public void ApplyMovement(RigidBodyMovement hub)
        {
            Vector3 surfaceNormal = hub.sensor.GroundHit.normal;
            Vector3 slopeDir = Vector3.ProjectOnPlane(-hub.rb.transform.up, surfaceNormal).normalized;
            Vector3 slopeSideWayDir = Vector3.Cross(surfaceNormal, slopeDir).normalized;
            
            float facingDirection = Mathf.Sign(Vector3.Dot(hub.orientation.forward, slopeDir));
            
            Vector3 movementDir = slopeSideWayDir * (hub.relay.MoveValue.x * facingDirection);

            Vector3 desiredRelativeVelocity = movementDir.normalized * hub.data.sideWayMoveSpeed;

            Vector3 slopVelocity = slopeDir * hub.data.slopeSpeed;
            hub.rb.linearVelocity = desiredRelativeVelocity + slopVelocity;
        }
    }
}