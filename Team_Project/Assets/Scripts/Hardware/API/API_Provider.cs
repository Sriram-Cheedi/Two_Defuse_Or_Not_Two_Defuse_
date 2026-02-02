using UnityEngine;

public class API_Provider : MonoBehaviour
{

    public bool useMockHardware;
    public API_Controller_Interface ActiveAPIController { get; private set; }
    private void Awake()
    {
        if (useMockHardware)
        {
            Debug.Log("Using Mock API Controller");
        }
        else
        {
            ActiveAPIController = GetComponentInChildren<API_Controller>();
        }

        if (ActiveAPIController != null)
        {
            Debug.Log("Connected to real API Controller");
            ActiveAPIController.Initialize();
        }
        else
        {
            Debug.LogError("API Provider failed to find or create a Controller!");
        }
    }
}
