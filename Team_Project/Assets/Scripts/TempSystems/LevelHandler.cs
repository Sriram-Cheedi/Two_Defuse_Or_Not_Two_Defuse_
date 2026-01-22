using System;
using UnityEngine;

public class LevelHandler : MonoBehaviour
{
    public GameObject level;

    private void OnEnable() { HardwareHandler.OnGyroRotationUpdated += RotateLevel; }
    private void OnDisable() { HardwareHandler.OnGyroRotationUpdated -= RotateLevel; }
    private void RotateLevel(float rotationValue)
    {
        Debug.Log("Rotating: " + rotationValue);
        level.transform.Rotate(0, 0, rotationValue);
    }
}
