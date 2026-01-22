using System;
using UnityEngine;

public class HardwareHandler : MonoBehaviour
{
    public static event Action<float> OnGyroRotationUpdated;

    void Update()
    {
        float currentRotation = 0.05f; // This will connect to Hardware API instead of raw float

        OnGyroRotationUpdated?.Invoke(currentRotation);
    }
}
