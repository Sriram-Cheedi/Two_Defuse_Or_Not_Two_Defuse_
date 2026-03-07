using System;

namespace RigidBodyPlayer.PlayerStates
{
    public abstract class BaseState<EState> where EState : Enum
    {
        public EState StateKey { get; private set; }

        public BaseState(EState state)
        {
            StateKey = state;
        }
        
        public abstract void EnterState();
        public abstract void ExitState();
        public abstract void UpdateState();
        public abstract void FixedUpdateState();
    }
}