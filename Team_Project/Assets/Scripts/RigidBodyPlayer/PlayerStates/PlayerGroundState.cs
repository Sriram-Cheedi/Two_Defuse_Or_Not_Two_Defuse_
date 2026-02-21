using UnityEngine;

public class PlayerGroundState : PlayerBaseState
{
    public PlayerGroundState(RigidBodyMovement hub) : base(hub)
    {
    }

    public override void EnterState()
    {
        Hub.CanJump = true;
        Hub.jump.readyToJump = true;
    }

    public override void UpdateState()
    {
        if (Hub.sensor.OnPlasticSurface)
        {
            Hub.TransitionToState(Hub.PlasticState);
        }

        if (!Hub.sensor.IsGrounded)
        {
            Hub.TransitionToState(Hub.AirborneState);
        }
        
        if (Hub.sensor.OnMovingPlatform)
        {
            Hub.TransitionToState(Hub.ConnectionState);
        }

        if (Hub.gravity.invertRequested && Hub.CanFlipGravity)
        {
            Hub.TransitionToState(Hub.FlipState);   
        }
    }

    public override void FixedUpdateState()
    {
        float speed = Hub.isCrouchPressed ? Hub.data.crouchSpeed : Hub.data.walkSpeed;
        Hub.ApplyMovement(speed, Hub.data.groundAcceleration, false);
        
    }
    public override void ExitState(){}
}
