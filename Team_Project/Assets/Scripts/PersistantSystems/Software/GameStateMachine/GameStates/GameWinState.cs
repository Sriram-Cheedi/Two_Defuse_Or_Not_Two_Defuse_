using System.Collections;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class GameWinState : BaseState<GameState>
    {
        private readonly GameManager gameManager;
        private readonly AudioSource audioSource;   
        private readonly AudioClip winClip;
        public GameWinState(GameManager gameManager, AudioSource audioSource, AudioClip winClip) : base(GameState.GameWin)
        {
            this.gameManager = gameManager;
            this.audioSource = audioSource;
            this.winClip = winClip;
        }

        public override void EnterState()
        {
            gameManager.FireOnGameEnd();
            API_Provider.Instance?.ActiveAPIController?.SendSet("PWR", 4);
            gameManager.StartCoroutine(LoadGameWinScene());
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

        private IEnumerator LoadGameWinScene()
        {
            audioSource.PlayOneShot(winClip);
            SceneManager.LoadScene("GameWin");
            yield return new WaitForSeconds(2f);
            gameManager.OnLoadingScoreBoard();
        }
    }
}