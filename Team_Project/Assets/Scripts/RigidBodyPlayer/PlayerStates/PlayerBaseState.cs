using RigidBodyPlayer.PlayerStates;
using UnityEngine;

public abstract class PlayerBaseState : BaseState<PlayerState>
{
    protected readonly RigidBodyMovement Hub;

    protected PlayerBaseState(RigidBodyMovement hub, PlayerState key) : base(key)
    {
        Hub = hub;
    }
}
