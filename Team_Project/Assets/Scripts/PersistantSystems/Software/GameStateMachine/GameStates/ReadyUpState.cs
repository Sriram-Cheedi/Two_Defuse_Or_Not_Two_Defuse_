using System;
using System.Collections;
using RigidBodyPlayer.PlayerStates;
using UnityEngine;


namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class ReadyUpState : BaseState<GameState>, IHardwareListener, IGameEventListener
    {
        private readonly GameManager gameManager;
        private readonly AudioSource audioSource;
        private readonly AudioClip soundTrack;
        private readonly float soundTrackStartPoint = 50.0f;
        private readonly float soundTrackEndPoint = 108.0f;
        private bool isBombActivated;
        private bool isTrapDoorOpened;
        private bool isHudSRead;
        public static event Action BombActivated;
        public static event Action TrapDoorOpened;
        public ReadyUpState(GameManager gameManager, AudioSource audioSource, AudioClip soundTrack) : base(GameState.ReadyUp)
        {
            this.gameManager = gameManager;
            this.audioSource = audioSource;
            this.soundTrack = soundTrack;
        }

        public override void EnterState()
        { 
            isHudSRead  = false;
            isBombActivated = false;
            isTrapDoorOpened = false;
            
            //soundTrack setup
            audioSource.clip = soundTrack;
            audioSource.time = soundTrackStartPoint;
            audioSource.loop = false;
            audioSource.Play();
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            if (audioSource.clip == soundTrack && audioSource.time >= soundTrackEndPoint)
            {
                audioSource.time = soundTrackStartPoint;
                if (!audioSource.isPlaying) audioSource.Play();
            }
            if (isBombActivated && isTrapDoorOpened)
            {
                gameManager.QueueNextState(GameState.Playing);
            }
        }

        public override void FixedUpdateState()
        {
        }

        public void OnHardwareInputChanged(string mod, int val)
        {
            if (mod == "PWR" && val == 1)
            {
                isBombActivated = true;
                BombActivated?.Invoke();
            }

            if (!isHudSRead) return;

            if (mod == "BTN4" && val == 1)
            {
                isTrapDoorOpened = true;
                TrapDoorOpened?.Invoke();
            }
        }

        public void OnHardwareDisconnect()
        {
        }

        public void OnHardwareReconnect()
        {
        }

        public void HandleOnBoardingDone()
        {
            isHudSRead = true;
        }
    }
}