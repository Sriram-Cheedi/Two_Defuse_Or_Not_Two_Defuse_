using System;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;
using Object = UnityEngine.Object;

public struct RespawnData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public bool IsUpsideDown;
}

public class SpawnManager : MonoBehaviour
{
    [SerializeField] public CheckPointScript respawnPoint;
    [SerializeField] private RigidBodyMovement hub;
    public static event Action<RespawnData> RespawnRequested;
    public static event Action OnDeath;

    private void Awake()
    {
        hub = FindAnyObjectByType<RigidBodyMovement>();
    }

    private void OnEnable()
    {
        PlayerDeathState.OnDeathAnimationFinished += CallRepawnReset;
    }

    private void OnDisable()
    {
        PlayerDeathState.OnDeathAnimationFinished -= CallRepawnReset;
    }

    public void CallRepawnReset()
    {
        if (hub == null && respawnPoint == null) return; 
        RespawnData data = new RespawnData
        {
            Position = respawnPoint.transform.position,
            Rotation = respawnPoint.transform.rotation,
            IsUpsideDown = respawnPoint.isUpsideDown,
        };
        RespawnRequested?.Invoke(data);
        hub.stateMachine.QueueNextState(PlayerState.Airborne);
    }

    public void OnRespawn()
    {
        if (hub == null && respawnPoint == null) return; 
        OnDeath?.Invoke();
        hub.stateMachine.QueueNextState(PlayerState.Death);
    }
}
