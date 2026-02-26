using RigidBodyPlayer.Jump;
using RigidBodyPlayer.Movement;
using RigidBodyPlayer.PlayerData;
using UnityEngine;

public class PlayerGroundState : PlayerBaseState
{
    public PlayerGroundState(RigidBodyMovement hub) : base(hub)
    {
    }

    public override void EnterState()
    {
        Hub.canJump = true;
        Hub.jump.readyToJump = true;
    }

    public override void UpdateState()
    {
        if (!Hub.sensor.IsGrounded)
        {
            Hub.TransitionToState(Hub.AirborneState);
            return;
        }
        
        if (Hub.sensor.OnMovingPlatform)
        {
            Hub.TransitionToState(Hub.ConnectionState);
            return;
        }
        
        if (Hub.gravity.invertRequested && Hub.canFlipGravity)
        {
            Hub.TransitionToState(Hub.FlipState);   
            return;
        }
        
        switch (Hub.sensor.CurrentSurface)
        {
            case SurfaceType.Metal:
                Hub.TransitionToState(Hub.MetalState);
                break;
            case SurfaceType.Plastic:
                Hub.TransitionToState(Hub.PlasticState);
                break;
            case SurfaceType.Slope:
                Hub.TransitionToState(Hub.SlopeState);
                break;
            default:
                break;
        }
    }

    public override void FixedUpdateState()
    {
    }

    public override void ExitState()
    {
    }
}
