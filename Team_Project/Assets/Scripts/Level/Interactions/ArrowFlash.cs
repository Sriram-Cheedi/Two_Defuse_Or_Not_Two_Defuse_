using System;
using UnityEngine;

public class ArrowFlash : MonoBehaviour
{
    public Material materialOn;
    public Material materialOff;
    private MeshRenderer meshRenderer;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = materialOff;
        }
    }

    private void OnEnable()
    {
        ActivationTip.IsBombActive += SwitchMaterial;
    }

    private void OnDisable()
    {
        ActivationTip.IsBombActive -= SwitchMaterial;
    }

    private void SwitchMaterial()
    {
        meshRenderer.material = materialOn;
    }
}
