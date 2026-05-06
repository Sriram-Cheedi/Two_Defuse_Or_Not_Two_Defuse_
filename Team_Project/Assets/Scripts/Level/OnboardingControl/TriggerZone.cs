using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public string zoneName;
    private ControlsManager manager;
    private void Awake()
    {
        manager = FindFirstObjectByType<ControlsManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (zoneName == "Mouse")
        {
            // manager.MouseInstructionCard();
        } else if (zoneName == "Jump")
        {
            // manager.JumpInstructionCard();
        }
        gameObject.SetActive(false);
    }
    
}