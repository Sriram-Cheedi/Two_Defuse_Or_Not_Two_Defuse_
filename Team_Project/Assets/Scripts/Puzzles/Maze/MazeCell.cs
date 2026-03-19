using System;
using UnityEngine;

namespace Puzzles.Controllers
{
    public class MazeCell : MonoBehaviour
    {
        public int y;
        public int x;
        private bool isSafeTile;
        private SpawnManager spawnManager;

        public void SetSafeZone(bool safeTile)
        {
            isSafeTile = safeTile;
        }

        private void Awake()
        {
            spawnManager = GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerInputRelay relay))
            {
                if (!isSafeTile)
                {
                    spawnManager.OnRespawn();
                }
                Debug.Log("Hit tile" + isSafeTile);
            }
        }
    }
}