using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class VerletInsulatedWire : MonoBehaviour
{
    private static readonly List<VerletInsulatedWire> allwires = new List<VerletInsulatedWire>();

    void OnEnable()
    {
        if (!allwires.Contains(this))
        {
            allwires.Add(this);
        }
    }

    void OnDisable()
    {
        allwires.Remove(this);
    }
    
    public enum  WireColour
    {
       Red,
       Blue,
       Grey,
       Brown,
       Yellow
    }
    public WireColour colour;
    
    [Header("Anchors")]
    public Transform anchor1;
    public Transform anchor2;
}