using System.Collections;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class GameOverState : BaseState<GameState>

    {
        private readonly GameManager gameManager;
        public GameOverState(GameManager gameManager) : base(GameState.GameOver)
        {
            this.gameManager = gameManager;
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
            SceneManager.LoadScene("GameOver");
        }
    }
}