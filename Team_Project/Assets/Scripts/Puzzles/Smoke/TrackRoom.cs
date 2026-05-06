using System.Collections.Generic;
using UnityEngine;

public class TrackRoom : MonoBehaviour
{
    private List<SmokeEmitter> rooms = new List<SmokeEmitter>();
    public SmokeEmitter currentSmokeEmitter {get; private set;}
    private void OnTriggerEnter(Collider other)
    {
        SmokeEmitter emitter = other.GetComponent<SmokeEmitter>();
        if (emitter == null)
        { 
            return;
        }
        if (!rooms.Contains(emitter))
        {
            rooms.Add(emitter);
        }
        currentSmokeEmitter = emitter;
    }

    private void OnTriggerExit(Collider other)
    {
        SmokeEmitter emitter = other.GetComponent<SmokeEmitter>();
        if (emitter == null)
        {
            return;
        }
        if (rooms.Contains(emitter))
        {
            rooms.Remove(emitter);
        }
        if (rooms.Count > 0)
        {
            currentSmokeEmitter = rooms[rooms.Count - 1];
        }
        else
        {
            currentSmokeEmitter = null;
        }
    }
}