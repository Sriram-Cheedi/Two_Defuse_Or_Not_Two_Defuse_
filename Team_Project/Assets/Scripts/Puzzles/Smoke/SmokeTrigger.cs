using UnityEngine;

public class Smoke_Trigger : MonoBehaviour
{
    [SerializeField] SmokeManager smokeManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            smokeManager.isActive = true;
            API_Provider.Instance.ActiveAPIController.SendSet("FGM", 1);
        }
    }
}