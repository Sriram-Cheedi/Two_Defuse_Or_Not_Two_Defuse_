using UnityEngine;

namespace RigidBodyPlayer.Jump
{
    public class SemiConductorJumpStrategy : IJumpStrategy
    {
        public void ExecuteJump(RigidBodyMovement hub)
        {
            if (hub.jump.readyToJump && hub.jump.coyoteTimeCounter > 0f && hub.jump.jumpBufferCounter > 0f &&
                hub.canJump)
            {
                Vector3 localUp = hub.rb.transform.up;
                Vector3 verticalVelocity = Vector3.Project(hub.rb.linearVelocity, localUp);
                hub.rb.linearVelocity -= verticalVelocity;
                hub.rb.AddForce(hub.transform.up * hub.data.jumpForceOnSemiConductor, ForceMode.Impulse);
                hub.jump.OnJumpExecuted();
            }
        }
    }
}