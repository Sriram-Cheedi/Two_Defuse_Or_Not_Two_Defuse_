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
    private void Awake()
    {
        _activeHardware = _API_Provider.ActiveAPIController;
        buttonsPressed = new bool[3];
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

    public void OnInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (context.control is not KeyControl key) return;
            if (key.keyCode == Key.Digit1 || key.keyCode == Key.Numpad1)
            {
                if (doors[0] != null)
                {
                    DisableDoorForSeconds(doors[0], 1f);
                    doors[0].SetActive(false);
                }
            }
            if (key.keyCode == Key.Digit2 || key.keyCode == Key.Numpad2)
            {
                if (doors[1] != null)
                {
                    DisableDoorForSeconds(doors[1], 1f);
                    doors[1].SetActive(false);
                }
            }
            if (key.keyCode == Key.Digit3 || key.keyCode == Key.Numpad3)
            {
                if (doors[2] != null)
                {
                    DisableDoorForSeconds(doors[2], 1f);
                    doors[2].SetActive(false);
                }
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
        // for (int i = 0; i < buttonsPressed.Length; i++)
        // {
        //     Debug.Log(buttonsPressed[i]);
        //     if (buttonsPressed[i])
        //     {
        //         Destroy(doors[i]);
        //     }
        // }

    }
}
