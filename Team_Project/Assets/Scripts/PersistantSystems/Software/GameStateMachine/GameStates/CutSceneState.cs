using RigidBodyPlayer.PlayerStates;
using UnityEngine;
using UnityEngine.Video;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class CutSceneState : BaseState<GameState>
    {
        private readonly GameManager gameManager;
        private readonly VideoPlayer player;
        private readonly GameObject menuUI;
        private readonly GameObject display;
        private readonly GameObject loadingPage;
        private readonly AudioSource audioSource;   
        private readonly AudioClip cutSceneSoundTrack;

        public CutSceneState(GameManager gameManager, VideoPlayer player, GameObject menuUI, GameObject display, GameObject loadingPage, AudioSource audioSource, AudioClip cutSceneSoundTrack)
            : base(GameState.CutScene)
        {
            this.gameManager = gameManager;
            this.player      = player;
            this.menuUI   = menuUI;
            this.display     = display;
            this.loadingPage     = loadingPage;
            this.audioSource = audioSource;
            this.cutSceneSoundTrack = cutSceneSoundTrack;
        }

        public override void EnterState()
        {
            audioSource.time = 0f;
            audioSource.clip = cutSceneSoundTrack;
            audioSource.loop = true;
            audioSource.Play();
            if (loadingPage != null)
                loadingPage.SetActive(true);

            if (menuUI != null)
                menuUI.SetActive(false);
            
            if (display != null)
                display.SetActive(true);

            if (player != null)
            {
                player.Stop();
                player.time = 0;
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
            
            audioSource.Stop();
        }

        public override void UpdateState() { }
        public override void FixedUpdateState() { }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            player.prepareCompleted -= OnVideoPrepared;
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