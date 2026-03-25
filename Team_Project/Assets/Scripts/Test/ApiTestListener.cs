
// Example usage of the API 

using UnityEngine;
public class ApiTestListener : MonoBehaviour
{
    public API_Controller api;

    void OnEnable()
    {
        API_Controller.OnInputChanged += DigitalChanged;
    }

    void OnDisable()
    {
        API_Controller.OnInputChanged -= DigitalChanged;

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

        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            api.TimerSet(200);
        }
    }
}