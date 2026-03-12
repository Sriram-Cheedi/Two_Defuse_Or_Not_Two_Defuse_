using UnityEngine;

namespace RigidBodyPlayer.PlayerStates
{
    public enum PlayerState
    {
        Grounded,
        Airborne,
        Flip,
        Death,
    }
    public class PlayerStateMachine : StateMachine<PlayerState>
    {
        public void Initialize(RigidBodyMovement hub)
        {
            States.Add(PlayerState.Grounded, new PlayerGroundState(hub));
            States.Add(PlayerState.Airborne, new PlayerAirborneState(hub));
            States.Add(PlayerState.Flip, new PlayerFlipState(hub));
            States.Add(PlayerState.Death, new PlayerDeathState(hub));
            QueueNextState(PlayerState.Airborne);
        }
    }
}