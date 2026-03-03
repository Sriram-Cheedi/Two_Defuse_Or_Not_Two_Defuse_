using RigidBodyPlayer.Jump;
using RigidBodyPlayer.PlayerData;

namespace RigidBodyPlayer.Movement
{
    public class MovementStrategyFactory
    {
        private static readonly IMovementStrategy Ground = new GroundMovementStrategy();
        private static readonly IMovementStrategy Air = new AirMovementStrategy();
        private static readonly IMovementStrategy Slope  = new SlopeMovementStrategy();
        private static readonly IMovementStrategy MovingPlayform = new ConnectedMovementStrategy();
        private static readonly IMovementStrategy Grid = new SemiConductorStrategy();
        
        private static readonly IJumpStrategy METAL = new MetalJumpStrategy();
        private static readonly IJumpStrategy PLASTIC = new PlasticJumpStrategy();
        private static readonly IJumpStrategy SEMI_CONDUCTOR = new SemiConductorJumpStrategy();
        
        public static IJumpStrategy GetJumpStrategy(RigidBodyMovement hub)
        {
            if (hub.sensor.CurrentSurface == SurfaceType.Metal)
            {
                return METAL;
            }
            if (hub.sensor.CurrentSurface == SurfaceType.SemiConductor)
            {
                return SEMI_CONDUCTOR;
            }
            return PLASTIC;
        }

        public static IMovementStrategy GetMovementStrategy(RigidBodyMovement hub)
        {
            if (!hub.sensor.IsGrounded)
            {
                return Air;
            }else if (hub.sensor.CurrentSurface == SurfaceType.Slope)
            {
                return Slope;
            }else if (hub.sensor.OnMovingPlatform)
            {
                return MovingPlayform;
            }

            if (hub.sensor.CurrentSurface == SurfaceType.SemiConductor)
            {
                return Grid;
            }
            return Ground;
        }
    }
}