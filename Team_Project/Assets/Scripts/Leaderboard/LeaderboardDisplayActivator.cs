using UnityEngine;

public class LeaderboardDisplayActivator : MonoBehaviour
{
    private void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Debug.Log("Leaderboard display activated on Display 2.");
        }
        else
        {
            Debug.LogWarning("No second display detected.");
        }
    }
}