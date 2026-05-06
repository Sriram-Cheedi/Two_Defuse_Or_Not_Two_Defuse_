using RigidBodyPlayer.PlayerStates;
using UnityEngine;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class MenuState : BaseState<GameState>   {
        private readonly GameManager gameManager;
        private readonly AudioSource audioSource;   
        private readonly AudioClip menuSoundTrack;
        public MenuState(GameManager gameManager, AudioSource audioSource, AudioClip menuSoundTrack) : base(GameState.Menu)
        {
            this.gameManager = gameManager;
            this.audioSource = audioSource;
            this.menuSoundTrack = menuSoundTrack;
        }

        public override void EnterState()
        {
            gameManager.CacheRefs();
            audioSource.time = 0f;
            audioSource.clip = menuSoundTrack;
            audioSource.loop = true;
            audioSource.Play();
        }

        public override void ExitState()
        {
            audioSource.Stop();
        }

        public override void UpdateState()
        {
        }

        public override void FixedUpdateState()
        {
        }
    }
}