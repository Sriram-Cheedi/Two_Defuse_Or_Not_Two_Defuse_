using System;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private RigidBodyMovement rm;
    [SerializeField] public GameObject respawnPoint;
    [SerializeField] private GridManager gridManager;

    private void Awake()
    {
        if (gridManager == null)
        {
            return;
        }
        gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
    }


    public void Respawn()
    {
        rm.rb.position = respawnPoint.transform.position;
        
        rm.ResetPlayer();
        // gridManager.ResetPath();
    }

}
