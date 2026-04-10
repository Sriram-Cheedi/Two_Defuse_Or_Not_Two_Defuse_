using System.Collections;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class GameOverState : BaseState<GameState>

    {
        private readonly GameManager gameManager;
        private readonly AudioSource audioSource;
        private readonly AudioClip loseSound;
        public GameOverState(GameManager gameManager, AudioSource audioSource, AudioClip loseSound) : base(GameState.GameOver)
        {
            this.gameManager = gameManager;
            this.audioSource = audioSource;
            this.loseSound = loseSound;
        }

        public override void EnterState()
        {
            gameManager.FireOnGameEnd();
            API_Provider.Instance?.ActiveAPIController?.SendSet("PWR", 4);
            SceneManager.LoadScene("Explode");
            gameManager.StartCoroutine(LoadGameOverScene());
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
        }

        public override void FixedUpdateState()
        {
        }
        private IEnumerator LoadGameOverScene()
        {
            yield return new WaitForSeconds(1f);
            audioSource.PlayOneShot(loseSound);
            SceneManager.LoadScene("GameOver");
        }
    }
}