using System;
using UnityEngine;

public class TrackRoom : MonoBehaviour
{
    public SmokeEmitter currentSmokeEmitter { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        SmokeEmitter emitter = other.GetComponent<SmokeEmitter>();
        if (emitter != null)
        {
            currentSmokeEmitter = emitter;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SmokeEmitter emitter = other.GetComponent<SmokeEmitter>();
        if (emitter != null && currentSmokeEmitter == emitter)
        {
            currentSmokeEmitter = null;
        }
    }
}