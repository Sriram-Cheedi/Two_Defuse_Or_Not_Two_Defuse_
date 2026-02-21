using RigidBodyPlayer.Movement;
using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(RigidBodyMovement hub) : base(hub)
    {
    }

    public override void EnterState()
    {
        Hub.CanJump = false;
    }
    public override void ExitState(){}

    public override void UpdateState()
    {
        if (Hub.sensor.OnSlope)
        {
            Hub.TransitionToState(Hub.SlopeState);
        }else if (Hub.sensor.OnMovingPlatform)
        {
            Hub.TransitionToState(Hub.ConnectionState);
        }
        else if (Hub.sensor.OnMetalSurface)
        {
            Hub.TransitionToState(Hub.MetalState);
        }else if (Hub.sensor.OnPlasticSurface)
        {
            Hub.TransitionToState(Hub.PlasticState);
        } else if (Hub.sensor.IsGrounded)
        {
            Hub.TransitionToState(Hub.GroundState);
        }
    }

    public override void FixedUpdateState()
    {
        IMovementStrategy strategy = MovementStrategyFactory.GetStrategy(Hub);
        strategy.ApplyMovement(Hub);
    }
}
