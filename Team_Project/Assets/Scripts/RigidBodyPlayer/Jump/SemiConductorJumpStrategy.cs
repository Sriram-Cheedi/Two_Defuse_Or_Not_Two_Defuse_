using UnityEngine;

namespace RigidBodyPlayer.Jump
{
    public class SemiConductorJumpStrategy : IJumpStrategy
    {
        public void ExecuteJump(RigidBodyMovement hub)
        {
            if (hub.jump.readyToJump && hub.jump.CoyoteTimeCounter > 0f && hub.jump.JumpBufferCounter > 0f &&
                hub.jump.canJump)
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