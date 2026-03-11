using UnityEngine;

namespace Puzzles.Minor_Interaction
{
    public class Minor_Interactinon_Controller : MonoBehaviour
    {

        enum MockButtonValues
        {
            BTN1 = 5,
            BTN2 = 6,
        }
        
        [SerializeField] public PlayerInputRelay relay;

        [SerializeField] public GameObject deathLazer;
        [SerializeField] public GameObject spawnRoomDoor;
        
    void OnEnable()
    {
        // For Testing with Keyboard Button Presses
        if (relay != null)
        {
            relay.NumberPressed += HandleMockButtonPress;
        }
        
        API_Provider.Instance.ActiveAPIController.OnInputChanged += HandleButtonPress;
    }

    void OnDisable()
    {
        API_Provider.Instance.ActiveAPIController.OnInputChanged -= HandleButtonPress;
        
        if (relay != null)
        {
            relay.NumberPressed -= HandleMockButtonPress;
        }
    }
    
    // Handle with hardware or corresponding software button presses and activate relevent ingame effect.
    private void HandleMockButtonPress(int digit)
    {
        switch (digit)
        {
            case (int)MockButtonValues.BTN1:
                SwingDoor();
                break;
            case  (int)MockButtonValues.BTN2:
                DisableDeathLazer();
                break;
        }
    }

    private void HandleButtonPress(string header, int value)
    {
        switch (header)
        {
            case "BTN1":
                SwingDoor();
                break;
            case "BTN2":
                DisableDeathLazer();
                break;
        }
    }
    
    // Ingame Effects
    private void DisableDeathLazer()
    {
        deathLazer.SetActive(false);
    }


    private void SwingDoor()
    {
        spawnRoomDoor.SetActive(false);
    }
    
    }
}   