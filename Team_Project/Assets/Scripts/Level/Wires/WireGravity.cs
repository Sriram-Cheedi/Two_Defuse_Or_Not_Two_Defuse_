using UnityEngine;

public class WireGravity : MonoBehaviour
{
    public float gravityMagnitude = 9.81f;
    public Vector3 Gravity {get; private set;} = new Vector3(0, -9.81f, 0);
    private API_Controller controller;

    void Update()
    { 
        if (API_Provider.Instance != null && API_Provider.Instance.ActiveAPIController != null)
        { 
            API_Provider.Instance.ActiveAPIController.OnImuChanged += OnImuChanged;
        }

    }

    private void OnImuChanged(Vector3 gyro, Vector3 accel)
    {
        Vector3 gravity = new Vector3(accel.x, accel.z, accel.y);
        Vector3 down = -gravity.normalized;
        Gravity = down * gravityMagnitude;
    }

    void OnDestroy()
    {
        if (API_Provider.Instance.ActiveAPIController != null)
        {
            API_Provider.Instance.ActiveAPIController.OnImuChanged -= OnImuChanged;
            
        }
    }
}