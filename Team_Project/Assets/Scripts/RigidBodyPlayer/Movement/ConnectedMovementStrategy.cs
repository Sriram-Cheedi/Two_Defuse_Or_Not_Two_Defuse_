using UnityEngine;

namespace RigidBodyPlayer.Movement
{
    public class ConnectedMovementStrategy : IMovementStrategy
    {
        public void ApplyMovement(RigidBodyMovement hub)
        {
            if (hub.data == null) return;
            Vector3 connectionVelocity = hub.sensor.ConnectionVelocity;

            Vector3 movementDir = hub.orientation.forward * hub.verticalInput + hub.orientation.right * hub.horizontalInput;
            Vector3 platformVelocity = hub.sensor.OnMovingPlatform ? connectionVelocity : Vector3.zero;

            Vector3 v = hub.rb.linearVelocity;
            Vector3 relativeVelocity = v - platformVelocity;
            float speed = hub.isCrouchPressed ? hub.data.crouchSpeed : hub.data.walkSpeed;
            Vector3 desiredRelativeVelocity = movementDir.sqrMagnitude > 0.0001f
                ? movementDir.normalized * speed
                : Vector3.zero;
            
            float maxDelta = hub.data.groundAcceleration * Time.fixedDeltaTime;

            float newX = Mathf.MoveTowards(relativeVelocity.x, desiredRelativeVelocity.x, maxDelta);
            float newZ = Mathf.MoveTowards(relativeVelocity.z, desiredRelativeVelocity.z, maxDelta);

            relativeVelocity = new Vector3(newX, relativeVelocity.y, newZ);
            hub.rb.linearVelocity = relativeVelocity + platformVelocity;
        }
    }
}