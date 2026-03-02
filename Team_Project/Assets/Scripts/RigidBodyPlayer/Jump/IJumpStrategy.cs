namespace RigidBodyPlayer.Jump
{
    public interface IJumpStrategy
    {
        void ExecuteJump(RigidBodyMovement hub);
        // void ApplyJumpCut(RigidBodyMovement hub);
    }
}