namespace RigidBodyPlayer.Movement
{
    public interface IMovementStrategy
    {
        void ApplyMovement(RigidBodyMovement hub);
    }
}