using RigidBodyPlayer.PlayerStates;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public CheckPointScript respawnPoint;
    private RigidBodyMovement hub;
    
    private PlayerInputRelay relay;
    private void Awake()
    {
        relay = Object.FindAnyObjectByType<PlayerInputRelay>();
        hub = FindAnyObjectByType<RigidBodyMovement>();
    }
    
    private void OnEnable()
    {
        if (relay != null) 
        {
            relay.RespawnRequested += ResetPlayer; 
        }
    }

    private void OnDisable()
    {
        if (relay != null) 
        {
            relay.RespawnRequested -= ResetPlayer; 
        }
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
