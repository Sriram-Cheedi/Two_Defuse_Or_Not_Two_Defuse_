using UnityEngine;

public class KillZone : MonoBehaviour
{
    private SpawnManager spawnManager;

    void Awake()
    {
        spawnManager = GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            spawnManager.Respawn();
        }
    }
}
