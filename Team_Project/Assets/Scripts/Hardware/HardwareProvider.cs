using System.Diagnostics;
using UnityEngine;

public class HardwareProvider : MonoBehaviour
{

    public bool useMockHardware;

    public I_HardwareController ActiveHardware { get; private set; }

    private void Awake()
    {
        if (useMockHardware)
        {
            UnityEngine.Debug.Log("Implement Mock Hardware Listener");
        }
        else
        {
            // If the physical script is on the same object
            ActiveHardware = GetComponent<HardwareController>();
            UnityEngine.Debug.Log("Connected to real Hardware Controller");
        }

        ActiveHardware.Initialize();
    }
}
