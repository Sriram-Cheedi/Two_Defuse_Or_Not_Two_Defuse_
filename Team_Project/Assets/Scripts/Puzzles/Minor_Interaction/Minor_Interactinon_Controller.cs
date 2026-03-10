using UnityEngine;

namespace Puzzles.Minor_Interaction
{
    public class Minor_Interactinon_Controller : MonoBehaviour
    {

        enum MockButtonValues
        {
            BTN1 = 5,
        }
        
        [SerializeField] public PlayerInputRelay relay;

        [SerializeField] public GameObject deathLazer;
        
    void Awake()
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

    private void HandleMockButtonPress(int digit)
    {
        if (digit <= 4) return;

        if (digit == (int)MockButtonValues.BTN1)
        {
            Debug.Log("Software BTN1 Pressed");
            DisableDeathLazer();
        }
    }

    private void HandleButtonPress(string header, int value)
    {
        if (header == "BTN1")
        {
            Debug.Log("Hardware BTN1 Pressed");
            DisableDeathLazer();
        }
    }
    
    // Button Effects

    private void DisableDeathLazer()
    {
        deathLazer.SetActive(false);
    }
    
    }
}   