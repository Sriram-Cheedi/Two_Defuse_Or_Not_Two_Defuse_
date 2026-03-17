using System;
using System.Collections;
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
        [SerializeField] private LevelData levelData;
        [SerializeField] private BarredDoorAnimation doorAnimaton;
        private bool hasDoorUnlocked;
        
        private Dictionary<String, int> barStates = new Dictionary<String, int>
        {
            { "SW1", 1 },
            { "SW2", 0 },
            { "SW3", 1 },
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
                HandleDeathLazer();
                break;
            
            case (int)MockButtonValues.SWT1:
                UpdateBarStatus("SW1", 0);
                break;
            case (int)MockButtonValues.SWT2:
                UpdateBarStatus("SW2", 0);
                break;
            case (int)MockButtonValues.SWT3:
                UpdateBarStatus("SW3", 0);
                break;
            
        }
    }

    private void HandleButtonPress(string header, int value)
    {
        switch (header)
        {
            case "BTN1":
                if (value == 1) SwingDoor(spawnRoomDoor);
                break;
            case "BTN2":
                Debug.Log("Controller Recieved BTN2");

                if (value == 1) HandleDeathLazer();
                break;

            case "SW1":
                UpdateBarStatus("SW1", value);
                break;
            case "SW2":
                UpdateBarStatus("SW2", value);
                break;
            case "SW3":
                UpdateBarStatus("SW3", value);
                break;
        }
    }
    
    // Ingame Effects
    private void HandleDeathLazer()
    {
        StartCoroutine(DisableDeathLazer());
    }

    IEnumerator DisableDeathLazer()
    {
        deathLazer.SetActive(false);
        yield return new WaitForSeconds(levelData.doorDelay);
        deathLazer.SetActive(true);
    }


    private void SwingDoor(GameObject door)
    {
        door.SetActive(false);
    }

    private void UpdateBarStatus(string header, int value)
    {
        if (hasDoorUnlocked) return;

        Debug.Log("Updating bar status" +  header);
        barStates[header] = value;

        // if (header == "SW1")
        // {
        //     if (value ==0)
        //     {
        //         doorAnimaton.MoveBar(0, Vector3.back);
        //     }
        //     else
        //     {
        //         doorAnimaton.MoveBar(0, Vector3.forward);
        //     }
        // } else if (header == "SW2")
        // {
        //     if (value == 1)
        //     {
        //         doorAnimaton.MoveBar(1, Vector3.forward);
        //     }
        //     else
        //     {
        //         doorAnimaton.MoveBar(1, Vector3.back);
        //     }
        // } else if (header == "SW3")
        // {
        //     if (value == 0)
        //     {
        //         doorAnimaton.MoveBar(2, Vector3.back);
        //     }
        //     else
        //     {
        //         doorAnimaton.MoveBar(2, Vector3.forward);
        //     }
        // }
        
        if (barStates["SW1"] == 0 && barStates["SW2"] == 1 && barStates["SW3"] == 0 )
        {
            doorAnimaton.MoveBar(0, Vector3.back);
            doorAnimaton.MoveBar(1, Vector3.forward);
            doorAnimaton.MoveBar(2, Vector3.back);
            doorAnimaton.OpenDoor();
            hasDoorUnlocked = true;
        }
    }
    
    }
}   