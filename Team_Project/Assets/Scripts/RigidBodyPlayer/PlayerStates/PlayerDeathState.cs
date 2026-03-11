using System;
using UnityEngine;

namespace RigidBodyPlayer.PlayerStates
{
    public class PlayerDeathState  : PlayerBaseState
    {
        public static event Action OnDeathAnimationFinished;
        private float targetRotation = 90f;
        private float remaining;
        public PlayerDeathState(RigidBodyMovement hub) : base(hub, PlayerState.Death)
        {
        }

        public override void EnterState()
        {
            Hub.rb.linearVelocity = Vector3.zero;
            Hub.jump.canJump = false;
            remaining = targetRotation;
        }

        public override void ExitState()
        {
            
        }

        public override void UpdateState()
        {
            
        }

        public override void FixedUpdateState()
        {
            Hub.rb.isKinematic = true;
            float step = Hub.data.deathRotationSpeed * Time.fixedDeltaTime;
            step = Mathf.Min(step, remaining);

            Quaternion deltaRotation = Quaternion.Euler(0f, 0f, step);

            if (Hub.cameraHandler.cameraPosition != null)
                Hub.cameraHandler.cameraPosition.Rotate(Vector3.forward, -step, Space.Self);

            if (Hub.cameraHandler.playerCam != null)
                Hub.cameraHandler.playerCam.rollZ += step;

            Hub.rb.MoveRotation(Hub.rb.rotation * deltaRotation);

            remaining -= step;
            if (remaining <= 0)
            {
                Hub.rb.isKinematic = false;
                OnDeathAnimationFinished?.Invoke();
            }
        }
    }
}