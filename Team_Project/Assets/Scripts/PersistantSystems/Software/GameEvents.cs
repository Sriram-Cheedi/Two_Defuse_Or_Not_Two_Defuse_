using System;

public static class GameEvents {
    public static event Action OnReplay;
    public static event Action OnQuit;
    public static void TriggerReplay() => OnReplay?.Invoke();
    public static void TriggerQuit() => OnQuit?.Invoke();
}