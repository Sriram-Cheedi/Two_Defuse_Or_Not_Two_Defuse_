using System;
using RigidBodyPlayer.PlayerStates;

namespace PersistantSystems.Software.GameStateMachine.GameStates
{
    public class ReadyUpState : BaseState<GameState>, IHardwareListener
    {
        private readonly GameManager gameManager;
        private bool isBombActivated;
        private bool isTrapDoorOpened;
        public static event Action BombActivated;
        public static event Action TrapDoorOpened;
        public ReadyUpState(GameManager gameManager) : base(GameState.ReadyUp)
        {
            this.gameManager = gameManager;
        }

        public override void EnterState()
        {
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
    }
}