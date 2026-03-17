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
        [SerializeField] private Transform barredDoorHinge;
        [SerializeField] private Transform spawnDoorHinge;
        [SerializeField] private BarredDoorAnimation doorAnimaton;
        private bool hasDoorUnlocked;

        private bool bar1solved = false;
        private bool bar2solved = false;
        private bool bar3solved = false;


        private Dictionary<int, Vector3> spawnPositions = new Dictionary<int, Vector3>();

        private int[] correctPostion = { 0, 0, 0 };

        private Dictionary<String, int> barStates = new Dictionary<String, int>
        {
            { "SW1", -1 },
            { "SW2", -1 },
            { "SW3", -1 },
        };

        void Start()
        {
            // if (barStates["SW1"] == 0)
            // {
            //     
            // }
            //
            // if (barStates["SW2"] == 0)
            // {
            //     doorAnimaton.MoveBar(1, Vector3.forward);
            //     doorAnimaton.MoveBar(1, Vector3.forward);
            //     correctPostion[1] = !correctPostion[1];
            // }
            // if (barStates["SW3"] == 0)
            // {
            //     doorAnimaton.MoveBar(2, Vector3.forward);
            //     doorAnimaton.MoveBar(2, Vector3.forward);
            //     correctPostion[2] = !correctPostion[2];
            // }
            //
            // setup = false;
        }

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
                    doorAnimaton.OpenDoor(spawnRoomDoor.transform, spawnDoorHinge, Vector3.forward, 1f);
                    break;
                case (int)MockButtonValues.BTN2:
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
                    if (value == 1)
                    {
                        doorAnimaton.OpenDoor(spawnRoomDoor.transform, spawnDoorHinge, Vector3.forward, 1f);
                    }
                    break;
                case "BTN2":
                    Debug.Log("Controller Recieved BTN2");

                    if (value == 1) HandleDeathLazer();
                    break;

                case "SW1":
                    if (hasDoorUnlocked) return;
                    if (barStates["SW1"] == -1)
                    {
                        UpdateBarStatus("SW1", value);
                        if (value == 1) return;
                        doorAnimaton.MoveBar(0, value);
                        doorAnimaton.MoveBar(0, value);
                        correctPostion[0] = 1;

                    }
                    else
                    {
                        UpdateBarStatus("SW1", value);
                        if (value == 1) doorAnimaton.MoveBar(0, value);
                    }

                    break;
                case "SW2":
                    if (hasDoorUnlocked) return;
                    if (barStates["SW2"] == -1)
                    {
                        UpdateBarStatus("SW2", value);
                        if (value == 1) return;
                        doorAnimaton.MoveBar(1, value);
                        doorAnimaton.MoveBar(1, value);
                        correctPostion[1] = 1;

                    }
                    else
                    {
                        UpdateBarStatus("SW2", value);
                        if (value == 1) doorAnimaton.MoveBar(1, value);
                    }

                    // UpdateBarStatus("SW2", value);
                    // if (setup) return;
                    // if (value == 1) doorAnimaton.MoveBar(1, Vector3.forward);
                    // else doorAnimaton.MoveBar(1, Vector3.back);
                    break;
                case "SW3":
                    if (hasDoorUnlocked) return;
                    if (barStates["SW3"] == -1)
                    {
                        UpdateBarStatus("SW3", value);
                        if (value == 1) return;
                        doorAnimaton.MoveBar(2, value);
                        doorAnimaton.MoveBar(2, value);
                        correctPostion[2] = 1;

                    }
                    else
                    {
                        UpdateBarStatus("SW3", value);
                        if (value == 1) doorAnimaton.MoveBar(2, value);
                        else doorAnimaton.MoveBar(2, value);
                    }


                    // UpdateBarStatus("SW3", value);
                    // if (setup) return;
                    // if (value == 1) doorAnimaton.MoveBar(2, Vector3.forward);
                    // else doorAnimaton.MoveBar(2, Vector3.back);
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

            Debug.Log("Updating bar status" + header);
            barStates[header] = value;

            if (barStates["SW1"] == correctPostion[0] && barStates["SW2"] == correctPostion[1] && barStates["SW3"] == correctPostion[2])
            {
                
                doorAnimaton.OpenDoor(barredDoor.transform, barredDoorHinge, Vector3.up, 1f);
                hasDoorUnlocked = true;
            }
        }

    }
}
