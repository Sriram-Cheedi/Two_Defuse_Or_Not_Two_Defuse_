using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class KeypadInteraction : MonoBehaviour, Interactible_Interface
{
    private Camera cam;
    private void Awake() => cam = Camera.main;

    public void Interact(RaycastHit hit)
    {
        Debug.Log("IN KeypadInteraction!");
        if (hit.collider.TryGetComponent(out NavKeypad.KeypadButton keypadButton))
        {
            keypadButton.PressButton();
        }
    }
}
