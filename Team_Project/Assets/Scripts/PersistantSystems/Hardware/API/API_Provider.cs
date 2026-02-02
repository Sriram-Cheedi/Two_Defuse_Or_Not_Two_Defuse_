using UnityEngine;

public class API_Provider : MonoBehaviour
{
    public static API_Provider Instance { get; private set; }

    public bool useMockHardware;
    public API_Controller_Interface ActiveAPIController { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (useMockHardware)
        {
            ActiveAPIController = new Mock_API_Controller();
            Debug.Log("<color=cyan>API Provider: Initializing MOCK mode.</color>");

            ActiveAPIController.Initialize(null);

            // Disabling Serial Port Controller so that ports dont open during mocks
            var realSerial = GetComponentInChildren<SerialPortController>(true);
            if (realSerial != null && realSerial is MonoBehaviour mb) mb.enabled = false;
        }
        else
        {
            ActiveAPIController = GetComponentInChildren<API_Controller>();

            if (ActiveAPIController != null)
            {
                Debug.Log("<color=green>API Provider: Initializing REAL mode.</color>");

                I_SerialPortController serial = GetComponentInChildren<SerialPortController>();

                ActiveAPIController.Initialize(serial);
            }
            else
            {
                Debug.LogError("API Provider: Could not find API_Controller in children!");
            }
        }
    }

    public void Update()
    {
        if (useMockHardware && ActiveAPIController is Mock_API_Controller mock)
        {
            mock.UpdateSimulation();
        }
    }
}