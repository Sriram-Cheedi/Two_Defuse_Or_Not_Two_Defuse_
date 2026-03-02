using UnityEngine;

public abstract class PlayerBaseState
{
    protected readonly RigidBodyMovement Hub;

    protected PlayerBaseState(RigidBodyMovement hub)
    {
        Hub = hub;
    }
    
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
}
