using RigidBodyPlayer.Jump;
using RigidBodyPlayer.Movement;
using RigidBodyPlayer.PlayerData;
using UnityEngine;

public class PlayerConnectionState : PlayerBaseState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PlayerConnectionState(RigidBodyMovement hub) : base(hub)
    {
    }

    public override void EnterState()
    {
        Hub.canJump = true;
    }

    public override void UpdateState()
    {
        if (!Hub.sensor.IsGrounded)
        {
            Hub.TransitionToState(Hub.AirborneState);
            return;
        }
        
        if (Hub.gravity.invertRequested && Hub.canFlipGravity)
        {
            Hub.TransitionToState(Hub.FlipState);   
            return;
        }
        
        if (Hub.sensor.OnMovingPlatform)
        {
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
                Hub.TransitionToState(Hub.GroundState);
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
