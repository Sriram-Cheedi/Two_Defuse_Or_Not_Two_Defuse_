using RigidBodyPlayer.PlayerData;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;

public class PlayerFlipState : PlayerBaseState
{
    private float targetRotation = 180f;
    private float remaining;
    [SerializeField] private EmojiReactionManager emojiReactionManager;
    public PlayerFlipState(RigidBodyMovement hub) : base(hub, PlayerState.Flip)
    {
    }

    public override void EnterState()
    {
        Hub.rb.linearVelocity = Vector3.zero;
        Hub.gravity.InvertGravityDirection();
        Hub.jump.canJump = false;
        // emojiReactionManager.PlayGravityBurst();
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
            Hub.stateMachine.QueueNextState(PlayerState.Airborne);
            return;
        }
        if (Hub.sensor.IsGrounded)
        {
            Hub.stateMachine.QueueNextState(PlayerState.Grounded);
            return;
        }
    }

    public override void ExitState()
    {
        Hub.gravity.invertRequested = false;
    }
}
