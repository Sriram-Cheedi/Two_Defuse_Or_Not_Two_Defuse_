using UnityEngine;

namespace RigidBodyPlayer.Movement
{
    public class AirMovementStrategy : IMovementStrategy
    {
        public void ApplyMovement(RigidBodyMovement hub)
        {
            if (hub.data == null) return;
            if (hub.data == null) return;
            Vector3 movementDir = hub.orientation.forward * hub.verticalInput + hub.orientation.right * hub.horizontalInput;
            Vector3 velocity = hub.rb.linearVelocity;
            float speed = hub.data.walkSpeed;
            Vector3 desiredRelativeVelocity = movementDir.sqrMagnitude > 0.0001f
                ? movementDir.normalized * speed
                : Vector3.zero;
            
            float maxDelta = hub.data.airAcceleration * Time.fixedDeltaTime;

            float newX = Mathf.MoveTowards(velocity.x, desiredRelativeVelocity.x, maxDelta);
            float newZ = Mathf.MoveTowards(velocity.z, desiredRelativeVelocity.z, maxDelta);

            velocity = new Vector3(newX, velocity.y, newZ);
            hub.rb.linearVelocity = velocity;
        }
    }
}