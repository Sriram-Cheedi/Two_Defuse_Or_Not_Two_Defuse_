using System;
using UnityEngine;

public class API_Provider : MonoBehaviour
{
    public static API_Provider Instance { get; private set; }

    public bool useMockHardware;
    public API_Controller_Interface ActiveAPIController { get; private set; }

    public bool hasAPIController = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (useMockHardware)
        {
            GameObject mockChild = new GameObject("Mock_API_Controller");
            mockChild.transform.SetParent(transform);

            ActiveAPIController = mockChild.AddComponent<Mock_API_Controller>();
            ActiveAPIController.InitializeSelf();
            ActiveAPIController.InitializeHardware();
        }

        else
        {
            GameObject child = new GameObject("API_Controller");
            child.transform.SetParent(transform);

            ActiveAPIController = child.AddComponent<API_Controller>();
            
            // Try init real hardware
            try
            {
                ActiveAPIController.InitializeSelf();
                ActiveAPIController.InitializeHardware();
            }
            // If fail default to mock hardware again
            catch (Exception e)
            {
                Debug.LogError("ERROR Failed connect to hardware: " + e.Message + " | Defaulting to Mock_API_Controller");
                Destroy(child);
                GameObject mockChild = new GameObject("Mock_API_Controller");
                mockChild.transform.SetParent(transform);

                ActiveAPIController = mockChild.AddComponent<Mock_API_Controller>();
                ActiveAPIController.InitializeSelf();      
                ActiveAPIController.InitializeHardware(); 
            }
        }
        hasAPIController = true;
    }

    public void Update()
    {
        // if (useMockHardware && ActiveAPIController is Mock_API_Controller mock)
        // {
        //     mock.UpdateSimulation();
        // }
    }
}