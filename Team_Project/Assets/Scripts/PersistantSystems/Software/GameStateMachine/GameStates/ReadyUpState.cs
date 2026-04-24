using System;
using System.Collections;
using RigidBodyPlayer.PlayerStates;


namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class ReadyUpState : BaseState<GameState>, IHardwareListener, IGameEventListener
    {
        private readonly GameManager gameManager;
        private bool isBombActivated;
        private bool isTrapDoorOpened;
        private bool isHudSRead;
        public static event Action BombActivated;
        public static event Action TrapDoorOpened;
        public ReadyUpState(GameManager gameManager) : base(GameState.ReadyUp)
        {
            this.gameManager = gameManager;
        }

        public override void EnterState()
        { 
            isHudSRead  = false;
            isBombActivated = false;
            isTrapDoorOpened = false;
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
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