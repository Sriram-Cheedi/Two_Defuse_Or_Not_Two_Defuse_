using System;
using PersistantSystems.Software.GameStateMachine.GameStates;
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
        ReadyUpState.TrapDoorOpened += SwitchMaterial;
    }

    private void OnDisable()
    {
        ReadyUpState.TrapDoorOpened -= SwitchMaterial;
    }

    private void SwitchMaterial()
    {
        meshRenderer.material = materialOn;
    }
}
