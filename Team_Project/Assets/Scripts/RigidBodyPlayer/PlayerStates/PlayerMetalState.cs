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
        Hub.CanJump = true;
        Hub.jump.readyToJump = true;
        Hub.CanFlipGravity = false;
    }
}
