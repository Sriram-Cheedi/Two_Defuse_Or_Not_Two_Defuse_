using System;
using RigidBodyPlayer.Modules;
using UnityEngine;

namespace RigidBodyPlayer.SoundEffect
{
    public class PlayerDeathSound : MonoBehaviour, IPlayerModule
    {
        private RigidBodyMovement hub;
        public AudioSource audioSource;
    
        public AudioClip deathClip;

        public void Initialize(RigidBodyMovement Hub)
        {
            hub = Hub;
        }

        private void OnEnable()
        {
            SpawnManager.RespawnRequested += PlayDeathSound;
        }

        private void OnDisable()
        {
            SpawnManager.RespawnRequested -= PlayDeathSound;
        }

        private void PlayDeathSound(RespawnData spawnData)
        {
            if (deathClip != null)
            {
                audioSource.PlayOneShot(deathClip, hub.data.volumnBoost);
            }
        }

    public void Tick()
        {
            
        }

        public void FixedTick()
        {
            
        }

        public void LateTick()
        {
            
        }
    }
}