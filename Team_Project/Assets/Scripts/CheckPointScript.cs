using UnityEngine;

public class CheckPointScript : MonoBehaviour
{
    private SpawnManager spawnManager;
    private BoxCollider respawnCollider;

    void Awake()
    {
        respawnCollider = GetComponent<BoxCollider>();
        spawnManager = GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            spawnManager.respawnPoint = this.gameObject;
            respawnCollider.enabled = false;
        }
    }

}
