using PlasticGui.WorkspaceWindow;
using UnityEngine;

public class API_Provider : MonoBehaviour
{
    public static API_Provider Instance { get; private set; }

    public bool useMockHardware;
    public API_Controller_Interface ActiveAPIController { get; private set; }
    public I_SerialPortController ActiveSerialController { get; private set; }

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
            GameObject mockChild = new GameObject("Mock_API_Controller");
            mockChild.transform.SetParent(this.transform);

            ActiveAPIController = mockChild.AddComponent<Mock_API_Controller>();
        }
        else
        {
            GameObject child = new GameObject("API_Controller");
            child.transform.SetParent(this.transform);

            ActiveAPIController = gameObject.AddComponent<API_Controller>();

            Debug.Log("<color=green>API Provider: Initializing REAL mode.</color>");

            GameObject serialChild = new GameObject("Serial_Controller");
            serialChild.transform.SetParent(this.transform);

            ActiveSerialController = serialChild.AddComponent<SerialPortController>();
            ActiveAPIController.Initialize(ActiveSerialController);
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