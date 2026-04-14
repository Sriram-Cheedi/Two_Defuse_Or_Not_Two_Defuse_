using System;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class PlayingState : BaseState<GameState>, IHardwareListener
    {
        private readonly GameManager gameManager;
        private readonly AudioSource warningAudio;
        public static Action ExplosionWarning;
        private bool hasWarningFired;
        
        private bool isPaused;
        private float previousTimeScale = 1f;
        public PlayingState(GameManager gameManager, AudioSource warningAudio) : base(GameState.Playing)
        {
            this.gameManager = gameManager;
            this.warningAudio = warningAudio;
        }

        public override void EnterState()
        {
            isPaused = false;
            previousTimeScale = 1f;
            ScoreManager scoreManager = UnityEngine.Object.FindFirstObjectByType<ScoreManager>();
            string currentUser = scoreManager != null ? scoreManager.GetCurrentUsername() : "---";

            DatabaseAPI.Instance?.UpdateStationStatus(GameplayStationConfig.StationId, currentUser, "Playing");
        }

        public override void ExitState()
        {
            if (warningAudio != null && warningAudio.isPlaying)
                warningAudio.Stop();
        }

        public override void UpdateState()
        {
            if (isPaused)
                return;

            gameManager.BombTimer -= Time.deltaTime;
            gameManager.BombTimer  = Math.Max(gameManager.BombTimer, 0.0);

            HandleWarningAudio();
            SyncHardwareTimer();

            if (gameManager.BombTimer <= 0.0)
                gameManager.EndGame();
        }

        public override void FixedUpdateState()
        {
        }

        public void OnHardwareInputChanged(string mod, int val)
        {
        }

        public void OnHardwareDisconnect()
        {
            if (isPaused)
                return;

            isPaused          = true;
            previousTimeScale = Time.timeScale;
            Time.timeScale    = 0f;

            if (warningAudio != null && warningAudio.isPlaying)
                warningAudio.Stop();
        }

        public void OnHardwareReconnect()
        {
            if (!isPaused)
                return;

            isPaused       = false;
            Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;

            try
            {
                API_Provider.Instance?.ActiveAPIController?.TimerSet((int)gameManager.BombTimer);
                API_Provider.Instance?.ActiveAPIController?.TimerOnOff(1);
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to reset bomb timer on reconnect: {e.Message}");
            }
        }
        private void HandleWarningAudio()
        {
            if (warningAudio == null)
                return;

            if (gameManager.BombTimer <= 5f && gameManager.BombTimer > 0f)
            {
                if (!hasWarningFired)
                {
                    ExplosionWarning?.Invoke();
                    hasWarningFired = true;
                }
                
                if (!warningAudio.isPlaying)
                    warningAudio.Play();
            }
            else
            {
                if (warningAudio.isPlaying)
                    warningAudio.Stop();
            }
        }

        private void SyncHardwareTimer()
        {
            if (!gameManager.IsMatSceneLoaded)
                return;

            API_Provider.Instance?.ActiveAPIController?.TimerSet((int)gameManager.BombTimer);
        }
    }
}