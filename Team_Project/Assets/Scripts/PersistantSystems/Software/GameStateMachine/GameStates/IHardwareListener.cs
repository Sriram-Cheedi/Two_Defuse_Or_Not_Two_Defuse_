namespace PersistantSystems.Software.GameStateMachine
{
    public interface IHardwareListener
    {
        void OnHardwareInputChanged(string mod, int val);
        void OnHardwareDisconnect();
        void OnHardwareReconnect();
    }
}