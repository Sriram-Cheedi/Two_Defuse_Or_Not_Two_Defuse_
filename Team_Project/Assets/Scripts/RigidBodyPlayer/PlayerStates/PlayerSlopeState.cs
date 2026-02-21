using RigidBodyPlayer.Movement;
using UnityEngine;

public class PlayerSlopeState : PlayerBaseState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PlayerSlopeState(RigidBodyMovement hub) : base(hub)
    {
    }

    public override void EnterState()
    {
        Hub.CanJump = false;
    }

    public override void UpdateState()
    {
        if (!Hub.sensor.IsGrounded)
        {
            Hub.TransitionToState(Hub.AirborneState);
        }
    }

    public override void FixedUpdateState()
    {
        IMovementStrategy strategy = MovementStrategyFactory.GetMovementStrategy(Hub);
        strategy.ApplyMovement(Hub);
    }

    public override void ExitState()
    {
        float sideVelocity = Vector3.Dot(Hub.rb.linearVelocity, Hub.slopeSideWayDir);
        Hub.rb.linearVelocity -= Hub.slopeSideWayDir * sideVelocity;
    }
}
