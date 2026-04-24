using RigidBodyPlayer.PlayerStates;

namespace PersistantSystems.Software.GameStateMachine
{
    public enum GameState { Menu, CutScene, ReadyUp, Playing, GameOver, GameWin, DashBoardWin, DashBoardLose }

    public class GameStateMachine : StateMachine<GameState>
    {
        
    }
}