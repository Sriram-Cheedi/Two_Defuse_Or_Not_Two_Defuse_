using UnityEngine;

public class Systems : MonoBehaviour
{
    public static Systems Instance;

    void Awake()
    {
        // If an instance already exists, destroy this one (prevents duplicates when returning to main menu)
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance and tell Unity not to kill this object on scene load
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
