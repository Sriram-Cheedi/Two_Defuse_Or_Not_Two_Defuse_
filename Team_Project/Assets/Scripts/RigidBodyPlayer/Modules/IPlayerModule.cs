namespace RigidBodyPlayer.Modules
{
    public interface IPlayerModule
    {
        void Initialize(RigidBodyMovement hub);
        void Tick();
        void FixedTick();
        void LateTick();
    }
}