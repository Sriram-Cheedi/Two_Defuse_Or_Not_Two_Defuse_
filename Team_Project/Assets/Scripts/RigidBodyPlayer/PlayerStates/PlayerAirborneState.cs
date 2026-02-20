using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(RigidBodyMovement hub) : base(hub)
    {
    }
    public override void EnterState(){}
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
        else if (Hub.sensor.IsGrounded)
        {
            Hub.TransitionToState(Hub.GroundState);
        }else if (Hub.gravity.invertRequested && Hub.sensor.InGravityZone)
        {
            Hub.TransitionToState(Hub.FlipState);
        }
    }

    public override void FixedUpdateState()
    {
        Hub.ApplyMovement(Hub.data.walkSpeed, Hub.data.groundAcceleration, false);
    }
}
