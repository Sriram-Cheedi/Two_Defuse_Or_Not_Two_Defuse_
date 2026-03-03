using UnityEngine;

namespace RigidBodyPlayer.Movement
{
    public class SlopeMovementStrategy : IMovementStrategy
    {
        public void ApplyMovement(RigidBodyMovement hub)
        {
            Vector3 surfaceNormal = hub.sensor.GroundHit.normal;
            hub.slopeDir = Vector3.ProjectOnPlane(-hub.rb.transform.up, surfaceNormal).normalized;
            hub.slopeSideWayDir = Vector3.Cross(surfaceNormal, hub.slopeDir).normalized;

            hub.movementDir = hub.slopeSideWayDir * hub.horizontalInput;

            Vector3 desiredRelativeVelocity = hub.movementDir.sqrMagnitude > 0.0001f
                ? hub.movementDir.normalized * hub.data.sideWayMoveSpeed
                : Vector3.zero;

            Vector3 slopVelocity = hub.slopeDir * hub.data.slopeSpeed;
            hub.rb.linearVelocity = desiredRelativeVelocity + slopVelocity;
        }
    }
}