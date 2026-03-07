using RigidBodyPlayer.Jump;
using RigidBodyPlayer.Movement;
using RigidBodyPlayer.PlayerData;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;

public class PlayerGroundState : PlayerBaseState
{
    public PlayerGroundState(RigidBodyMovement hub) : base(hub, PlayerState.Grounded)
    {
    }
    
    public enum GroundSubState
    {
        Normal, Metal, Plastic, Slope
    }
    private GroundSubState currentSubstate;

    public override void EnterState()
    {   
        Hub.jump.canJump = true;
        Hub.jump.readyToJump = true;
        UpdateSubState(force: true);
    }

    public override void UpdateState()
    {
        if (!Hub.sensor.IsGrounded)
        {
            Hub.stateMachine.QueueNextState(PlayerState.Airborne);
            return;
        }
        
        if (Hub.gravity.invertRequested && Hub.gravity.canFlipGravity)
        {
            Hub.stateMachine.QueueNextState(PlayerState.Flip);
            return;
        }
        UpdateSubState(force: false);
    }

    public override void FixedUpdateState()
    {
        var movementStrategy = MovementStrategyFactory.GetMovementStrategy(Hub);
        movementStrategy.ApplyMovement(Hub);
        var jumpStrategy = MovementStrategyFactory.GetJumpStrategy(Hub);
        jumpStrategy.ExecuteJump(Hub);
    }

    public override void ExitState()
    {
        switch (currentSubstate)
        {
            case GroundSubState.Metal:
                Hub.sensor.SetPreviousSurface(SurfaceType.Metal);
                break;
            case GroundSubState.Plastic:
                Hub.sensor.SetPreviousSurface(SurfaceType.Plastic);
                break;
            default:
                break;
        }
    }

    private void UpdateSubState(bool force)
    {
        GroundSubState newSubState = DetermineSubState();
        if (!force &&  newSubState == currentSubstate) return;
        currentSubstate = newSubState;
        switch (currentSubstate)
        {
            case GroundSubState.Metal:
                Hub.jump.canJump = true;
                Hub.jump.readyToJump = true;
                Hub.gravity.canFlipGravity = false;
                break;
            case GroundSubState.Plastic:
                Hub.jump.canJump = true;
                Hub.jump.readyToJump = true;
                Hub.gravity.canFlipGravity = true;
                break;
            case GroundSubState.Slope:
                Hub.jump.canJump = false;
                Hub.jump.readyToJump = false;
                Hub.gravity.canFlipGravity = false;
                break;
            default:
                Hub.jump.canJump = true;
                Hub.jump.readyToJump = true;
                break;
        }
    }
    
    private GroundSubState DetermineSubState()
    {
        if (Hub.sensor.CurrentSurface == SurfaceType.Metal)
            return GroundSubState.Metal;

        if (Hub.sensor.CurrentSurface == SurfaceType.Plastic)
            return GroundSubState.Plastic;
        
        if (Hub.sensor.CurrentSurface == SurfaceType.Slope)
            return GroundSubState.Slope;

        return GroundSubState.Normal;
    }
}
