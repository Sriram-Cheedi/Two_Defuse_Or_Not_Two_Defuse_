using System;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;

namespace Puzzles.Controllers
{
    public class MazeCell : MonoBehaviour
    {
        public int y;
        public int x;
        private bool isSafeTile;
        private SpawnManager spawnManager;
        private Transform electricity;
        private RigidBodyMovement hub;
        [SerializeField] public Material matCorrect;
        [SerializeField] public Material matDefault;


        public void SetSafeZone(bool safeTile)
        {
            isSafeTile = safeTile;
        }

        private void Awake() 
        {
            spawnManager = GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>();
            electricity = transform.Find("Electricity");
            hub = FindAnyObjectByType<RigidBodyMovement>();
            if (electricity != null)
            {
                electricity.gameObject.SetActive(false);
            }
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        private bool IsPlayerRespawning()
        {
            if (hub == null) return false;
            return hub.stateMachine.CurrentStateKey == PlayerState.Death;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !IsPlayerRespawning())
            {
                if (!isSafeTile)
                {
                    spawnManager.OnRespawn();
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (electricity != null)
                {
                    electricity.gameObject.SetActive(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (electricity != null)
                {
                    electricity.gameObject.SetActive(false);
                }
            }
        }
    }
}