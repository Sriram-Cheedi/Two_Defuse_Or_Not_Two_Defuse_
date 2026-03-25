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
        [SerializeField] public Material matIncorrect;
        private Material matDefault;
        private MeshRenderer tileRenderer;


        public void SetSafeZone(bool safeTile)
        {
            isSafeTile = safeTile;
        }

        private void Awake() 
        {
            spawnManager = GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>();
            electricity = transform.Find("Electricity");
            hub = FindAnyObjectByType<RigidBodyMovement>();
            tileRenderer = GetComponent<MeshRenderer>();
            matDefault = tileRenderer.material;
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
            if (matCorrect == null || matDefault == null || matIncorrect == null) return;
            if (other.CompareTag("Player") && !IsPlayerRespawning())
            {
                if (!isSafeTile)
                {
                    spawnManager.OnRespawn();
                    tileRenderer.material = matIncorrect;
                }
                else
                {
                    tileRenderer.material = matCorrect;
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (matCorrect == null || matDefault == null || matIncorrect == null) return;
            if (other.CompareTag("Player"))
            {
                if (!isSafeTile && electricity != null)
                {
                    electricity.gameObject.SetActive(true);
                    tileRenderer.material = matIncorrect;
                }

                if (isSafeTile)
                {
                    tileRenderer.material = matCorrect;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (matCorrect == null || matDefault == null || matIncorrect == null) return;
            if (other.CompareTag("Player"))
            {
                if (electricity != null)
                {
                    electricity.gameObject.SetActive(false);
                }
                tileRenderer.material = matDefault;
            }
        }
    }
}