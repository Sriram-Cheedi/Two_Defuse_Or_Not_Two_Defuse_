using RigidBodyPlayer.PlayerStates;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class ScoreDashBoardState : BaseState<GameState>
    {
        private GameManager gameManager;
        private AudioSource audioSource;
        private AudioClip scoreBoardSound;
        public ScoreDashBoardState(GameManager gameManager, AudioSource audioSource, AudioClip scoreBoardSound) : base(GameState.DashBoard)
        {
            this.gameManager = gameManager;
            this.audioSource = audioSource;
            this.scoreBoardSound = scoreBoardSound;
        }

        public override void EnterState()
        {
            audioSource.time = 0f;
            audioSource.clip = scoreBoardSound;
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