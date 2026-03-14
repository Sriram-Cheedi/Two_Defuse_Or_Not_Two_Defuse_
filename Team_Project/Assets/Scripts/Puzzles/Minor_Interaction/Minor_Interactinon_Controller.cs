using System;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzles.Minor_Interaction
{
    public class Minor_Interactinon_Controller : MonoBehaviour
    {

        enum MockButtonValues
        {
            BTN1 = 5,
            BTN2 = 6,
            
            SWT1 = 8,
            SWT2 = 9,
            SWT3 = 0,
        }
        
        [SerializeField] public PlayerInputRelay relay;

        [SerializeField] public GameObject deathLazer;
        [SerializeField] public GameObject spawnRoomDoor;
        
        [SerializeField] public GameObject[] bars;
        [SerializeField] public GameObject barredDoor;

        private Dictionary<String, int> barStates = new Dictionary<String, int>
        {
            { "SWT1", 1 },
            { "SWT2", 1 },
            { "SWT3", 1 },
        };
        
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
                SwingDoor(spawnRoomDoor);
                break;
            case  (int)MockButtonValues.BTN2:
                DisableDeathLazer();
                break;
            
            case (int)MockButtonValues.SWT1:
                UpdateBarStatus("SWT1", 0);
                break;
            case (int)MockButtonValues.SWT2:
                UpdateBarStatus("SWT2", 0);
                break;
            case (int)MockButtonValues.SWT3:
                UpdateBarStatus("SWT3", 0);
                break;
            
        }
    }

    private void HandleButtonPress(string header, int value)
    {
        switch (header)
        {
            case "BTN1":
                SwingDoor(spawnRoomDoor);
                break;
            case "BTN2":
                DisableDeathLazer();
                break;
            
            
            case "SWT1":
                UpdateBarStatus("SWT1", value);

                break;
            case "SWT2":
                UpdateBarStatus("SWT2", value);

                break;
            case "SWT3":
                UpdateBarStatus("SWT3", value);

                break;
        }
    }
    
    // Ingame Effects
    private void DisableDeathLazer()
    {
        deathLazer.SetActive(false);
    }


    private void SwingDoor(GameObject door)
    {
        door.SetActive(false);
    }

    private void UpdateBarStatus(string header, int value)
    {
        Debug.Log("Updating bar status" +  header);
        barStates[header] = value;

        if (header == "SWT1")
        {
            if (value == 1) bars[0].SetActive(true);
            else bars[0].SetActive(false);
        } else if (header == "SWT2")
        {
            if (value == 1) bars[1].SetActive(true);
            else bars[1].SetActive(false);
        } else if (header == "SWT3")
        {
            if (value == 1) bars[2].SetActive(true);
            else bars[2].SetActive(false);
        }

        bool blocked = false;
        foreach (int state in barStates.Values)
        {
            if (state == 1)
            {
                blocked = true;
                break;
            }
        }
    
        if (!blocked) 
        {
            SwingDoor(barredDoor);
        }
    }
    
    }
}   