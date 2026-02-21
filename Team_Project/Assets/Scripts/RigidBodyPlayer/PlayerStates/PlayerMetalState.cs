using UnityEngine;

public class PlayerMetalState : PlayerGroundState
{
    public PlayerMetalState(RigidBodyMovement hub) : base(hub)
    {
    }
    public override void EnterState()
    {
        Hub.CanJump = false;
        Hub.jump.readyToJump = true;
        Hub.CanFlipGravity = false;
    }
}
