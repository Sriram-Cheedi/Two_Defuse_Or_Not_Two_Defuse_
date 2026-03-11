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

    private void Awake()
    {
        hub = FindAnyObjectByType<RigidBodyMovement>();
    }
    
    public void CallRepawn()
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
    

    public void ResetPlayer()
    {
        if (hub == null && respawnPoint == null) return; 
            hub.rb.position = respawnPoint.transform.position;
            hub.rb.linearVelocity = Vector3.zero;
            if (!respawnPoint.isUpsideDown)
            {
                ResetPlayerRotation();
                ResetPlayerGravity();
            }
            else
            {
                ResetInversePlayerRotation();
                ResetInversePlayerGravity();
            }
            hub.stateMachine.QueueNextState(PlayerState.Airborne);
    }

    private void ResetPlayerRotation()
    {
        var playerCam = hub.cameraHandler.playerCam;
        var orientation = hub.orientation;
        if (playerCam != null) playerCam.rollZ = 0f;
        if (orientation != null) hub.orientation.transform.rotation = respawnPoint.transform.rotation;
    }

    private void ResetPlayerGravity()
    {
        hub.gravity.invertRequested = false;
        hub.gravity.SetGravityDirection(Vector3.down);
        hub.rb.rotation = Quaternion.identity;

        if (hub.cameraHandler.cameraPosition != null)
            hub.cameraHandler.cameraPosition.localRotation = Quaternion.identity;
    }
    
    private void ResetInversePlayerRotation()
    {
        var playerCam = hub.cameraHandler.playerCam;
        var orientation = hub.orientation;
        if (playerCam != null) playerCam.rollZ = 180f;
        if (orientation != null) orientation.rotation = Quaternion.Euler(0, 0, 180);
    }

    private void ResetInversePlayerGravity()
    {
        hub.gravity.invertRequested = false;
        hub.gravity.SetGravityDirection(Vector3.up);
        hub.rb.rotation = Quaternion.Euler(0, 0, 180);

        if (hub.cameraHandler.cameraPosition != null)
            hub.cameraHandler.cameraPosition.localRotation = Quaternion.Euler(0, 0, 180);
    }

}
