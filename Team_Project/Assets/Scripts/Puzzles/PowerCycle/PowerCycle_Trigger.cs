using System;
using System.Collections;
using UnityEngine;

public class PowerCycle_Trigger : MonoBehaviour
{
    private bool hasTriggered;

    private void Awake()
    {
        hasTriggered = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (other.CompareTag("Player"))
        {
            API_Provider.Instance.ActiveAPIController.SendSet("PWR", 2);
            hasTriggered = true;
        }
    }
}