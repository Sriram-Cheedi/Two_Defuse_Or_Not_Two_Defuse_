using UnityEngine;

namespace RigidBodyPlayer.PlayerData
{
    public enum SurfaceType { Default, Metal, Plastic, Slope, SemiConductor }
    [AddComponentMenu("RigidBodyPlayer/Surface Identity")]
    public class SurfaceIdentity : MonoBehaviour
    {
        public SurfaceType type;
        public bool isMovingPlatform;
    }
}
