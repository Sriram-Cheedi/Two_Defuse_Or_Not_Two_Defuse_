using UnityEngine;

public class WireGravity : MonoBehaviour
{
    public float gravityMagnitude = 9.81f;
    public Vector3 Gravity { get; private set; } = new Vector3(0, -9.81f, 0);
    private API_Controller controller;

    void Update()
    {
        if (controller == null)
        {
            if (API_Provider.Instance != null && API_Provider.Instance.ActiveAPIController is API_Controller real)
            {
                controller = real;
                controller.OnImuChanged += OnImuChanged;
                Debug.Log("WireGravity connected to API_Controller.");
            }
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
        if (controller != null)
        {
            controller.OnImuChanged -= OnImuChanged;
        }
    }
    
     // public float gravityStrength = 9.81f;
     //
     // public Vector3 Gravity { get; private set; } = new Vector3(0f, -9.81f, 0f);
     //
     // void Update()
     // {
     //     Vector3 dir = Vector3.zero;
     //
     //     if (Input.GetKeyDown(KeyCode.H))
     //         dir = Vector3.left;
     //     else if (Input.GetKeyDown(KeyCode.L))
     //         dir = Vector3.right;
     //     else if (Input.GetKeyDown(KeyCode.J))
     //         dir = Vector3.down;
     //     else if (Input.GetKeyDown(KeyCode.K))
     //         dir = Vector3.up;
     //
     //     if (dir != Vector3.zero)
     //     {
     //         Gravity = dir * gravityStrength;
     //         Debug.Log("Gravity changed to: " + Gravity);
     //     }
     // }

}