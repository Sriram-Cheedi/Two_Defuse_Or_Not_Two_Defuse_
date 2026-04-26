using System;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class PlayingState : BaseState<GameState>, IHardwareListener
    {
        private readonly GameManager gameManager;
        private readonly AudioSource audioSourceSfx;
        private readonly AudioSource audioSourceMusic;
        private readonly AudioClip warningAudio;
        private readonly AudioClip soundTrack;
        public static Action ExplosionWarning;
        private bool hasWarningFired;
        private readonly float soundTrackStartTime = 108.0f;
        
        private bool isPaused;
        private float previousTimeScale = 1f;
        public PlayingState(GameManager gameManager, AudioSource audioSourceSfx, AudioSource audioSourceMusic, AudioClip warningAudio, AudioClip soundTrack) : base(GameState.Playing)
        {
            this.gameManager = gameManager;
            this.audioSourceSfx = audioSourceSfx;
            this.audioSourceMusic =  audioSourceMusic;
            this.warningAudio = warningAudio;
            this.soundTrack = soundTrack;
        }

        public override void EnterState()
        {
            hasWarningFired = false;
            isPaused = false;
            previousTimeScale = 1f;
            
    
            if (soundTrack != null)
                Debug.Log($"PlayingState: clip length={soundTrack.length}, seeking to {soundTrackStartTime}");
    
            audioSourceMusic.clip = soundTrack;
            audioSourceMusic.loop = false;
            audioSourceMusic.Play();
            audioSourceMusic.timeSamples = (int)(soundTrackStartTime * soundTrack.frequency);

            string currentUser = gameManager.GetCurrentUsername();
            DatabaseAPI.Instance?.UpdateStationStatus(GameplayStationConfig.StationId, currentUser, "Playing");
        }

        public override void ExitState()
        {
            audioSourceSfx.Stop();
            audioSourceMusic.Stop();
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

            audioSourceMusic.Stop();
            audioSourceSfx.Stop();
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

            if (gameManager.BombTimer <= 10f && gameManager.BombTimer > 0f)
            {
                if (!hasWarningFired)
                {
                    ExplosionWarning?.Invoke();
                    hasWarningFired = true;
                    
                    audioSourceSfx.clip = warningAudio;
                    audioSourceSfx.loop = true;
                    audioSourceSfx.Play();
                }
                
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