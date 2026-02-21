using UnityEngine;

namespace RigidBodyPlayer
{
    public interface IJumpStrategy
    {
        void Execute(Rigidbody rb, Vector3 upDirection, MovementData data);
    }
}