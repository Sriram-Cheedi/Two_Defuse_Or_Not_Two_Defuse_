using RigidBodyPlayer.Jump;
using RigidBodyPlayer.Movement;
using UnityEngine;

public class PlayerMetalState : PlayerGroundState
{
    public PlayerMetalState(RigidBodyMovement hub) : base(hub)
    {
    }
    public override void EnterState()
    {
        Hub.canJump = true;
        Hub.jump.readyToJump = true;
        Hub.canFlipGravity = false;
    }
}
