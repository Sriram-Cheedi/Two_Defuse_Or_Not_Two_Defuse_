using UnityEngine;

public class PowerCycle_Trigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            API_Provider.Instance.ActiveAPIController.SendSet("PWR", 3);
        }
    }
}