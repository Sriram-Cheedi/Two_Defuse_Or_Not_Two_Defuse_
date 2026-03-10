using UnityEngine;

namespace RigidBodyPlayer.PlayerStates
{
    public class PlayerDeathState  : PlayerBaseState
    {
        private float targetRotation = 90f;
        private float remaining;
        public PlayerDeathState(RigidBodyMovement hub) : base(hub, PlayerState.Death)
        {
        }

        public override void EnterState()
        {
            
        }

        public override void ExitState()
        {
            
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
                
            }
        }
    }
}