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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
