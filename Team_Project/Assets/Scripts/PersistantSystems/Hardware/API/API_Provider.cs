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
            GameObject mockChild = new GameObject("Mock_API_Controller");
            mockChild.transform.SetParent(this.transform);

            ActiveAPIController = mockChild.AddComponent<Mock_API_Controller>();
        }
        else
        {
            GameObject child = new GameObject("API_Controller");
            child.transform.SetParent(this.transform);

            ActiveAPIController = child.AddComponent<API_Controller>();
        }
        ActiveAPIController.InitializeSelf();
    }

    public void Update()
    {
        if (useMockHardware && ActiveAPIController is Mock_API_Controller mock)
        {
            mock.UpdateSimulation();
        }
    }
}