namespace RigidBodyPlayer.Movement
{
    public class MovementStrategyFactory
    {
        private static readonly IMovementStrategy Ground = new GroundMovementStrategy();
        private static readonly IMovementStrategy Air = new AirMovementStrategy();
        private static readonly IMovementStrategy Slope  = new SlopeMovementStrategy();
        private static readonly IMovementStrategy MovingPlayform = new ConnectedMovementStrategy();

        public static IMovementStrategy GetStrategy(RigidBodyMovement hub)
        {
            if (!hub.sensor.IsGrounded)
            {
                return Air;
            }else if (hub.sensor.OnSlope)
            {
                return Slope;
            }else if (hub.sensor.OnMovingPlatform)
            {
                return MovingPlayform;
            }
            return Ground;
        }
    }
}