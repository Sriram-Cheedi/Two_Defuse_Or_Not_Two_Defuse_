using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private RigidBodyMovement rm;
    [SerializeField] public GameObject respawnPoint;
    private PlayerInputRelay relay;
    private void Awake()
    {
        relay = Object.FindAnyObjectByType<PlayerInputRelay>();
    }
    private void OnEnable()
    {
        if (relay != null) 
        {
            // Subscribe to the "ThingHappened" event
            relay.RespawnRequested += MovePlayerToRespawn; 
        }
    }

    private void OnDisable()
    {
        if (relay != null) 
        {
            // Unsubscribe to prevent memory leaks or "ghost" calls
            relay.RespawnRequested -= MovePlayerToRespawn; 
        }
    }

    public void MovePlayerToRespawn()
    {
        if (rm != null)
        {
            rm.rb.position = respawnPoint.transform.position;
        }
    }

}
