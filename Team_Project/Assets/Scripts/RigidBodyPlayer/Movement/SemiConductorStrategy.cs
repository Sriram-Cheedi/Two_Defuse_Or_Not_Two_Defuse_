using UnityEngine;

namespace RigidBodyPlayer.Movement
{
    public class SemiConductorStrategy : IMovementStrategy
    {
        public void ApplyMovement(RigidBodyMovement hub)
        {
            if (hub.data == null) return;
            Vector3 movementDir = hub.orientation.forward * hub.relay.MoveValue.y + hub.orientation.right * hub.relay.MoveValue.x;
            Vector3 velocity = hub.rb.linearVelocity;
            float speed = hub.crouch.isCrouchPressed ? hub.data.crouchSpeed : hub.data.gridSpeed;
            Vector3 desiredRelativeVelocity = movementDir.normalized * speed;
            
            float maxDelta = hub.data.groundAcceleration * Time.fixedDeltaTime;

            float newX = Mathf.MoveTowards(velocity.x, desiredRelativeVelocity.x, maxDelta);
            float newZ = Mathf.MoveTowards(velocity.z, desiredRelativeVelocity.z, maxDelta);

            velocity = new Vector3(newX, velocity.y, newZ);
            hub.rb.linearVelocity = velocity;
        }
    }
}