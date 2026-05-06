using Puzzles.Minor_Interaction;
using UnityEngine;

public class DoorClose : MonoBehaviour
{
    [SerializeField] private DoorID doorID;
    [SerializeField] private Minor_Interactinon_Controller interactinonController;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        interactinonController.RequestCloseDoor(doorID);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
