using System;
using UnityEngine;

public class LevelHandler : MonoBehaviour
{
    private void OnEnable() { HardwareHandler.OnGyroRotationUpdated += RotateLevel; }
    private void OnDisable() { HardwareHandler.OnGyroRotationUpdated -= RotateLevel; }
    private void RotateLevel(float rotationValue)
    {
        Debug.Log("Rotating: " + rotationValue);
        // transform.localRotation = Quaternion.Euler(0, rotationValue, 0);
    }
}
