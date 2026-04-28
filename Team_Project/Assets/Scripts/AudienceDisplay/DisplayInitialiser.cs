using UnityEngine;

public class DisplayInitialiser : MonoBehaviour
{
    private void Awake()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Debug.Log("Display 2 activated");
        }
        else
        {
            Debug.LogWarning("Display 2 not found");
        }
    }
}