using UnityEngine;

public class CheckPointScript : MonoBehaviour
{
    [SerializeField]private SpawnManager spawnManager;
    private BoxCollider respawnCollider;
    public bool isUpsideDown;

    void Awake()
    {
        respawnCollider = GetComponent<BoxCollider>();
        spawnManager = GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            spawnManager.respawnPoint = this;
            // respawnCollider.enabled = false;
        }
    }

}
