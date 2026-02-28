using RigidBodyPlayer.PlayerData;
using UnityEngine;

public class PlayerFlipState : PlayerBaseState
{
    private float targetRotation = 180f;
    private float remaining;
    public PlayerFlipState(RigidBodyMovement hub) : base(hub)
    {
    }

    public override void EnterState()
    {
        Hub.rb.linearVelocity = Vector3.zero;
        Hub.gravity.InvertGravityDirection();
        Hub.canJump = false;
        remaining = targetRotation;
    }

    public override void UpdateState()
    {
        
    }

    public override void FixedUpdateState()
    {
        float step = Hub.data.rotationSpeed * Time.fixedDeltaTime;
        step = Mathf.Min(step, remaining);

        Quaternion deltaRotation = Quaternion.Euler(0f, 0f, step);

        if (Hub.cameraHandler.cameraPosition != null)
            Hub.cameraHandler.cameraPosition.Rotate(Vector3.forward, step, Space.Self);

        if (Hub.cameraHandler.playerCam != null)
            Hub.cameraHandler.playerCam.rollZ += step;

        Hub.rb.MoveRotation(Hub.rb.rotation * deltaRotation);

        remaining -= step;
        if (remaining <= 0)
        {
            ExitLogicState();
        }
    }

    private void ExitLogicState()
    {
        if (!Hub.sensor.IsGrounded)
        {
            Hub.TransitionToState(Hub.AirborneState);
            return;
        }
        if (Hub.sensor.OnMovingPlatform)
        {
            Hub.TransitionToState(Hub.ConnectionState);
            return;
        }

        switch (Hub.sensor.CurrentSurface)
        {
            case SurfaceType.Metal:
                Hub.TransitionToState(Hub.MetalState);
                break;
            case SurfaceType.Plastic:
                Hub.TransitionToState(Hub.PlasticState);
                break;
            case SurfaceType.Slope:
                Hub.TransitionToState(Hub.SlopeState);
                break;
            default:
                break;
        }
    }

    public override void ExitState()
    {
        Hub.gravity.invertRequested = false;
    }
}
