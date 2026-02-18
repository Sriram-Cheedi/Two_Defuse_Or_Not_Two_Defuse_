using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DestroyDoor : MonoBehaviour
{
    [SerializeField] private GameObject[] doors;
    [SerializeField] private float delay = 1f;
    private API_Provider _API_Provider = API_Provider.Instance;
    private API_Controller_Interface _activeHardware;
    private bool[] buttonsPressed;
    private PlayerInputRelay relay;
    private void Awake()
    {
        _activeHardware = _API_Provider.ActiveAPIController;
        buttonsPressed = new bool[3];
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
        for (int i = 0; i < buttonsPressed.Length; i++)
        {
            int buttonId = i + 1;
            int buttonValue = API_Provider.Instance.ActiveAPIController.GetButton(buttonId.ToString());
            if (buttonValue == 1)
            {
                DisableDoorForSeconds(doors[i], 1f);
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
                DisableDoorForSeconds(targetDoor, 1f);
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
        // GetButtonsValues();
    }
}