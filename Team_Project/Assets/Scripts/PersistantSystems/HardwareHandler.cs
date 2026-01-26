using System;
using UnityEngine;

public class HardwareHandler : MonoBehaviour
{
    public static event Action<float> OnGyroRotationUpdated;

    float last_reported_rotation = 0;
    float acc_rotation = 0;
    const float step_size = 5f;

    void FixedUpdate()
    {

        // Gyro Logic 
        float hardware_rotation = 45.123f; // This will be a current gyro rotation from hardware API 

        float delta = Mathf.DeltaAngle(last_reported_rotation, hardware_rotation);

        if (Mathf.Abs(delta + acc_rotation) >= step_size && GameManager.currentState == GameManager.GameState.Playing)
        {
            Debug.Log("TE");
            float combinedMovement = delta + acc_rotation;
            int steps = (int)(combinedMovement / step_size);
            float totalStepMovement = steps * step_size;

            OnGyroRotationUpdated?.Invoke(totalStepMovement);

            last_reported_rotation = (last_reported_rotation + totalStepMovement) % 360;
            acc_rotation = combinedMovement % step_size;
        }


        // Other Hardware Logic
    }
}
