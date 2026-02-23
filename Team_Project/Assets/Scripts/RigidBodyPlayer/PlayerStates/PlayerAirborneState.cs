using RigidBodyPlayer.Movement;
using RigidBodyPlayer.PlayerData;
using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(RigidBodyMovement hub) : base(hub)
    {
    }

    public override void EnterState()
    {
        Hub.canJump = false;
    }
    public override void ExitState(){}

    public override void UpdateState()
    {
        if (!Hub.sensor.IsGrounded)
        {
            return;
        }
        if (Hub.sensor.OnMovingPlatform)
        {
            Hub.TransitionToState(Hub.ConnectionState);
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
        IMovementStrategy strategy = MovementStrategyFactory.GetMovementStrategy(Hub);
        strategy.ApplyMovement(Hub);
    }
}
