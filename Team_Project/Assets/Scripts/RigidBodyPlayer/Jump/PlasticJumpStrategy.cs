using UnityEngine;

namespace RigidBodyPlayer.Jump
{
    public class PlasticJumpStrategy : IJumpStrategy
    {
        public void ExecuteJump(RigidBodyMovement hub)
        {
            if (hub.jump.readyToJump && hub.jump.coyoteTimeCounter > 0f && hub.jump.jumpBufferCounter > 0f &&
                hub.canJump)
            {
                Vector3 localUp = hub.rb.transform.up;
                Vector3 verticalVelocity = Vector3.Project(hub.rb.linearVelocity, localUp);
                hub.rb.linearVelocity -= verticalVelocity;
                hub.rb.AddForce(hub.transform.up * hub.data.jumpForceOnPlastic, ForceMode.Impulse);
                hub.jump.OnJumpExecuted();
            }
        }

        public void ApplyJumpCut(RigidBodyMovement hub)
        {
            float verticalVelocity = Vector3.Dot(hub.rb.linearVelocity, hub.rb.transform.up);
            if (verticalVelocity > 0f)
            {
                hub.rb.linearVelocity -= hub.rb.transform.up * (verticalVelocity * 0.5f);
            }
        }
    }
}