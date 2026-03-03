using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DestroyDoor : MonoBehaviour
{
    [SerializeField] private GameObject[] doors;
    [SerializeField] private LevelData levelData;
    private API_Controller_Interface _activeHardware;
    private bool[] buttonsPressed;
    private PlayerInputRelay relay;
    private void Awake()
    {
        buttonsPressed = new bool[doors.Length];
    }

    private void Start()
    {
        _activeHardware = API_Provider.Instance.ActiveAPIController;
    }

    private void OnEnable()
    {
        relay = Object.FindAnyObjectByType<PlayerInputRelay>();
        if (relay != null)
        {
            relay.NumberPressed += HandleDoorInput;
        }
    }
    
    private void OnDisable()
    {
        if (relay != null)
        {
            relay.NumberPressed -= HandleDoorInput;
        }
    }

    private void GetButtonsValues()
    {
        if (_activeHardware == null) return;
        for (int i = 0; i < buttonsPressed.Length; i++)
        {
            int buttonId = i + 1;
            int buttonValue = API_Provider.Instance.ActiveAPIController.GetButton(buttonId.ToString());
            if (buttonValue == 1)
            {
                DisableDoorForSeconds(doors[i], levelData.doorDelay);
                doors[i].SetActive(false);
            }
        }
    }

    private void HandleDoorInput(int number)
    {
        int index = number - 1;
        if (index >= 0 && index < doors.Length)
        {
            GameObject targetDoor = doors[index];
            if (targetDoor != null && targetDoor.activeSelf)
            {
                DisableDoorForSeconds(targetDoor, levelData.doorDelay);
                targetDoor.SetActive(false);
            }
        }
    }

    private void DisableDoorForSeconds(GameObject door, float delay)
    {
        StartCoroutine(HandlesDoorSwitch(door, delay));
    }

    IEnumerator HandlesDoorSwitch(GameObject door, float delay)
    {
        yield return new WaitForSeconds(delay);
        door.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        GetButtonsValues();
    }
}