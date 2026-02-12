using System;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private RigidBodyMovement rm;
    [SerializeField] public GameObject respawnPoint;

    public void Respawn()
    {
        player.transform.position = respawnPoint.transform.position;
        rm.ResetPlayer();
    }
}
