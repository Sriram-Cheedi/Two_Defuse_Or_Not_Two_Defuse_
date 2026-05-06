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
        [SerializeField] public GameObject lightCorrect;
        [SerializeField] public GameObject lightIncorrect;


        public void SetSafeZone(bool safeTile)
        {
            isSafeTile = safeTile;
        }

        private void Awake() 
        {
            spawnManager = GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>();
            electricity = transform.Find("Electricity");
            lightCorrect  = transform.Find("Point_Light_Correct").gameObject;
            lightIncorrect = transform.Find("Point_Light_Wrong").gameObject;
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

            ResetLight();
         }

        private void ResetLight()
        {
            lightCorrect.SetActive(false);
            lightIncorrect.SetActive(false);
        }

        private bool IsPlayerRespawning()
        {
            if (hub == null) return false;
            return hub.stateMachine.CurrentStateKey == PlayerState.Death;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (lightCorrect == null || lightIncorrect == null) return;
            if (other.CompareTag("Player") && !IsPlayerRespawning())
            {
                if (!isSafeTile)
                {
                    spawnManager.OnRespawn();
                    lightIncorrect.SetActive(true);
                }
                else
                {
                    lightCorrect.SetActive(true);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (lightCorrect == null || lightIncorrect == null) return;
            if (other.CompareTag("Player"))
            {
                if (!isSafeTile && electricity != null)
                {
                    electricity.gameObject.SetActive(true);
                    lightIncorrect.SetActive(true);
                }

                if (isSafeTile)
                {
                    lightCorrect.SetActive(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (lightCorrect == null || lightIncorrect == null) return;
            if (other.CompareTag("Player"))
            {
                if (electricity != null)
                {
                    electricity.gameObject.SetActive(false);
                }
                ResetLight();
            }
        }
    }
}