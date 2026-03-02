using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;

    void Awake()
    {
        spawnManager = GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerInputRelay relay))
        {
            relay.CallRepawn();
        }
    }
}
