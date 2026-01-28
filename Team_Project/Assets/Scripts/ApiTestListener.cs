
// Example usage of the API 

using UnityEngine;
public class ApiTestListener : MonoBehaviour
{
    public ApiManager api;

    void OnEnable()
    {
        api.OnInputChanged += DigitalChanged;
        api.OnGyroChanged += GyroChanged;
    }

    void OnDisable()
    {
        api.OnInputChanged -= DigitalChanged;
        api.OnGyroChanged -= GyroChanged;
    }

    void DigitalChanged(string header, int value)
    {
        Debug.Log("DIGITAL -> " + header + " = " + value);
    }

    void GyroChanged(string header, float value)
    {
        Debug.Log("GYRO -> " + header + " = " + value);
    }
     void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("GET BTN1 -> " + api.GetButton("1"));
            Debug.Log("GET SWT2 -> " + api.GetSwitch("1"));
            Debug.Log("GET GYR0 -> " + api.GetGyro("GYR1"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            api.TimerSet(200);
        }
    }
}