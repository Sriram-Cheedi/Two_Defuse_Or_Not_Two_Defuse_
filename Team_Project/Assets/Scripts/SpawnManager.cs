using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] public GameObject respawnPoint;

    public void Respawn()
    {
        player.transform.position = respawnPoint.transform.position;
    }
}
