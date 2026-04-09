using RigidBodyPlayer.PlayerStates;
using UnityEngine;
using UnityEngine.Video;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class CutSceneState : BaseState<GameState>
    {
        private readonly GameManager gameManager;
        private readonly VideoPlayer player;
        private readonly GameObject display;
        private readonly GameObject menuUI;

        public CutSceneState(GameManager gameManager, VideoPlayer player, GameObject display, GameObject menuUI)
            : base(GameState.CutScene)
        {
            this.gameManager = gameManager;
            this.player      = player;
            this.display     = display;
            this.menuUI      = menuUI;
        }

        public override void EnterState()
        {
            if (menuUI != null)
                menuUI.SetActive(false);

            if (player != null)
            {
                player.prepareCompleted -= OnVideoPrepared;
                player.prepareCompleted += OnVideoPrepared;
                player.Prepare();
            }
            else
            {
                Debug.LogWarning("No cutscene player assigned. Skipping to ReadyUp.");
                gameManager.SkipCutScene();
            }
        }

        public override void ExitState()
        {
            if (player != null)
            {
                player.prepareCompleted -= OnVideoPrepared;
                player.loopPointReached -= OnVideoFinished;
            }

            if (display != null)
                display.SetActive(false);
        }

        public override void UpdateState() { }
        public override void FixedUpdateState() { }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            player.prepareCompleted -= OnVideoPrepared;

            if (display != null)
                display.SetActive(true);

            player.loopPointReached -= OnVideoFinished;
            player.loopPointReached += OnVideoFinished;
            player.Play();
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            gameManager.OnCutSceneFinished();
        }
    }
}