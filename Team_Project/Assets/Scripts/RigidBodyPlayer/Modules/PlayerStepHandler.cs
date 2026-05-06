using UnityEngine;
using RigidBodyPlayer.PlayerData;

namespace RigidBodyPlayer.Modules
{
    public class PlayerStepHandler : MonoBehaviour, IPlayerModule
    {
        [SerializeField] private CapsuleCollider controller;
        [SerializeField] private LayerMask groundLayer;
        private RigidBodyMovement Hub;
        
        public void Initialize(RigidBodyMovement hub)
        {
            if (controller == null) controller = GetComponent<CapsuleCollider>();
            Hub = hub;
        }

        public void Tick()
        {
            
        }

        public void FixedTick()
        {
            if (Hub.relay.MoveValue.sqrMagnitude < 0.01f) return;
            Vector2 input = Hub.relay.MoveValue.normalized;
            Vector3 localDir = new Vector3(input.x, 0f, input.y);
            Vector3 worldDir = transform.TransformDirection(localDir);
            HandleStep(worldDir);
            
        }

        public void LateTick()
        {
            
        }

        private void HandleStep(Vector3 dir)
        {
            Vector3 up = transform.up;
            Vector3 down = -up;
            float radius = controller.radius * 0.9f;
            float forwardCast = radius + Hub.data.forwardCast;
            
            Vector3 capsuleCenter = transform.TransformPoint(controller.center);
            float halfHeight = controller.height * 0.5f;
            Vector3 footPos = capsuleCenter + down * halfHeight;
            Vector3 footOrigin = footPos + up * Hub.data.groundLift;
            Vector3 topOrigin = footOrigin + up * Hub.data.stepOffset;

            if (Physics.Raycast(footOrigin, dir, out _, forwardCast, groundLayer))
            {
                if (!Physics.Raycast(topOrigin, dir, forwardCast, groundLayer))
                {
                    Hub.rb.position += up * Hub.data.stepSmooth;
                }
            }
        }
    }
}