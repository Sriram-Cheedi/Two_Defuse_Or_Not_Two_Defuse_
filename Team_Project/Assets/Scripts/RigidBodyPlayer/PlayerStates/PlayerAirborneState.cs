using RigidBodyPlayer.Movement;
using RigidBodyPlayer.PlayerData;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(RigidBodyMovement hub) : base(hub, PlayerState.Airborne)
    {
    }

    public override void EnterState()
    {
        Hub.jump.canJump = false;
        Hub.gravity.canFlipGravity = Hub.sensor.GetPreviousSurface() == SurfaceType.Plastic;
    }
    public override void ExitState(){}

    public override void UpdateState()
    {
        if (Hub.gravity.invertRequested && Hub.gravity.canFlipGravity)
        {
            Hub.stateMachine.QueueNextState(PlayerState.Flip);
            return;
        }

        if (!Hub.sensor.IsGrounded)
        {
            return;
        }

        if (Hub.sensor.IsGrounded)
        {
            Hub.stateMachine.QueueNextState(PlayerState.Grounded);
        }
    }

    public override void FixedUpdateState()
    {
        IMovementStrategy strategy = MovementStrategyFactory.GetMovementStrategy(Hub);
        strategy.ApplyMovement(Hub);
    }
}
