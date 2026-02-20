using UnityEngine;

public class PlayerConnectionState : PlayerBaseState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PlayerConnectionState(RigidBodyMovement hub) : base(hub)
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
        float speed = Hub.isCrouchPressed ? Hub.data.crouchSpeed : Hub.data.walkSpeed;
        Hub.ApplyMovement(speed, Hub.data.groundAcceleration, true);
    }

    public override void ExitState()
    {
    }
}
