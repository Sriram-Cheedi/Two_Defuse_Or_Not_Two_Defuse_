using RigidBodyPlayer.Jump;
using RigidBodyPlayer.Movement;
using RigidBodyPlayer.PlayerData;
using UnityEngine;

public class PlayerPlasticState : PlayerGroundState
{
    public PlayerPlasticState(RigidBodyMovement hub) : base(hub)
    {
    }
    public override void EnterState()
    {
        Hub.canJump = true;
        Hub.jump.readyToJump = true;
        Hub.canFlipGravity = true;
    }

    public override void ExitState()
    {
        Hub.sensor.SetPreviousSurface(SurfaceType.Plastic);
    }
}
