using RigidBodyPlayer.Movement;
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
        IMovementStrategy strategy = MovementStrategyFactory.GetStrategy(Hub);
        strategy.ApplyMovement(Hub);
    }

    public override void ExitState()
    {
        float horizontalVelocity = Vector3.Dot(Hub.rb.linearVelocity, Vector3.right);
        if (horizontalVelocity > 0f)
        {
            Hub.rb.linearVelocity -= Hub.rb.transform.right * horizontalVelocity;
        }
    }
}
