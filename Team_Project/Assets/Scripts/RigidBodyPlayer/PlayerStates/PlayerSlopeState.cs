using UnityEngine;

public class PlayerSlopeState : PlayerBaseState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PlayerSlopeState(RigidBodyMovement hub) : base(hub)
    {
    }

    public override void EnterState()
    {
        
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
        Hub.ApplySlopeMovement(Hub.sensor.GroundHit.normal, Hub.horizontalInput);
    }

    public override void ExitState()
    {
    }
}
