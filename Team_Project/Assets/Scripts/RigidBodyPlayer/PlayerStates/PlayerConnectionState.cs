using RigidBodyPlayer.Jump;
using RigidBodyPlayer.Movement;
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
        }

        if (Hub.sensor.OnMetalSurface)
        {
            Hub.TransitionToState(Hub.MetalState);
        }

        if (Hub.sensor.OnPlasticSurface)
        {
            Hub.TransitionToState(Hub.PlasticState);
        }
    }

    public override void FixedUpdateState()
    {
        IMovementStrategy moveStrategy = MovementStrategyFactory.GetMovementStrategy(Hub);
        moveStrategy.ApplyMovement(Hub);
        IJumpStrategy jumpStrategy = MovementStrategyFactory.GetJumpStrategy(Hub);
        jumpStrategy.ExecuteJump(Hub);
    }

    public override void ExitState()
    {
    }
}
