using RigidBodyPlayer.PlayerStates;
using UnityEngine.SceneManagement;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class GameWinState : BaseState<GameState>
    {
        private readonly GameManager gameManager;
        public GameWinState(GameManager gameManager) : base(GameState.GameWin)
        {
            this.gameManager = gameManager;
        }

        public override void EnterState()
        {
            gameManager.FireOnGameEnd();
            API_Provider.Instance?.ActiveAPIController?.SendSet("PWR", 4);
            SceneManager.LoadScene("GameWin");
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
    }
}