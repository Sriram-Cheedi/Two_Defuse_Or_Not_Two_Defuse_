using RigidBodyPlayer.Jump;
using RigidBodyPlayer.Movement;
using UnityEngine;

public class PlayerPlasticState : PlayerGroundState
{
    public PlayerPlasticState(RigidBodyMovement hub) : base(hub)
    {
    }
    public override void EnterState()
    {
        Hub.CanJump = true;
        Hub.jump.readyToJump = true;
        Hub.CanFlipGravity = true;
    }
}
