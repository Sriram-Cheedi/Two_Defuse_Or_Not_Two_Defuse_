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
            SpawnManager.OnDeath += PlayDeathSound;
        }

        private void OnDisable()
        {
            SpawnManager.OnDeath -= PlayDeathSound;
        }

        private void PlayDeathSound()
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