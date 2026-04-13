using RigidBodyPlayer.PlayerStates;

namespace PersistantSystems.Software.GameStateMachine
{
    public enum GameState { Menu, CutScene, ReadyUp, Playing, GameOver, GameWin, DashBoard }

    public class GameStateMachine : StateMachine<GameState>
    {
        
    }
}