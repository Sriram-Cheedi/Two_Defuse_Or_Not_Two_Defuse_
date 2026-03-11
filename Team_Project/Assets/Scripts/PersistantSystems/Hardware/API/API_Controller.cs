using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework;

struct HardwareState
{
    public Dictionary<string, int> componentStates;
    public Vector3 gyroValue;
    public Vector3 accelerometerValue;
    public bool hasAccelerometer;
    
    public int[] currentHardwareNGMValues;
    public int[] LastNGMSubmissionValues;
}

public class API_Controller : MonoBehaviour, API_Controller_Interface
{
    public I_SerialPortController ActivePortController { get; private set; }
    
    public event Action<string, int> OnInputChanged;
    public event Action<Vector3> OnGyroChanged;
    public event Action<Vector3, Vector3> OnImuChanged;
    public event Action<int[]> OnNGMSubmit;
    public event Action<int[]> OnNGMChanged;
    public event Action<int> OnFGMChanged; 
    
    // Local saved hardware values
    private HardwareState hardwareState;

    public I_SerialPortController serialController;
    
    // Getters for Local saved hardware values
    public Vector3 getGyroValue => hardwareState.gyroValue;
    public Vector3 getAccelerometerValue => hardwareState.accelerometerValue;
    public bool getHasAccelerometer => hardwareState.hasAccelerometer;
    public Dictionary<string, int> getComponentStateManager => hardwareState.componentStates;
    public int[] GetcurrentHardwareNGMValues => hardwareState.currentHardwareNGMValues;
    public int[] GetLastNGMSubmissionValues => hardwareState.LastNGMSubmissionValues;

    
    
    private void OnDestroy() => Exit();

    public void Awake()
    {
        hardwareState = new HardwareState();
        hardwareState.componentStates = new Dictionary<string, int>();
        hardwareState.gyroValue = Vector3.zero; ;
        hardwareState.hasAccelerometer = false;

        hardwareState.currentHardwareNGMValues = new int[4];
        hardwareState.LastNGMSubmissionValues = new int[4];
    }
    /// <summary>
    /// Initializes Local dictionary of all connected hardware components to 0 (off).
    /// Passes in the serialController and connects its 'Received' action to the HandleMessage function.
    /// </summary>
    public void InitializeSelf()
    {
        Debug.Log("<color=green>API Provider: Initializing REAL mode.</color>");

        hardwareState.componentStates["BTN1"] = 0;
        hardwareState.componentStates["BTN2"] = 0;
        hardwareState.componentStates["BTN3"] = 0;
        hardwareState.componentStates["BTN4"] = 0;
        hardwareState.componentStates["BTN5"] = 0;

        hardwareState.componentStates["SWT1"] = 0;
        hardwareState.componentStates["SWT2"] = 0;
        hardwareState.componentStates["SWT3"] = 0;

        hardwareState.componentStates["LED1"] = 0;
        hardwareState.componentStates["LED2"] = 0;
        hardwareState.componentStates["LED3"] = 0;
        hardwareState.componentStates["SSM"] = 0;

        hardwareState.componentStates["FGM"] = 0;

        GameObject child = new GameObject("Serial_Port_Controller");
        child.transform.SetParent(this.transform);
        ActivePortController = child.AddComponent<SerialPortController>();
        Debug.Log("Setting Serial Controller");

        if (ActivePortController != null)
        {
            Debug.Log("Setting Handle Message for Real Serial Controller");
            ActivePortController.Received += HandleMessage;
            var serialPortController = ActivePortController as SerialPortController;
            if (serialPortController != null)
            {
                serialPortController.bombDisconnected += HandleDisconnect;
                serialPortController.bombReconnected += HandleReconnect;
            }
        }
        else
        {
            Debug.LogError("Error: Unable to set serial Controller for REAL API_Controller");
        }
    }


    public void InitializeHardware()
    {
        Debug.Log("<color=green>Initializing REAL Hardware...</color>");
    }

    void Exit()
    {
        if (ActivePortController != null)
        {
            ActivePortController.Received -= HandleMessage;
            var serialPortController = ActivePortController as SerialPortController;
            if (serialPortController != null)
            {
                serialPortController.bombDisconnected -= HandleDisconnect;
                serialPortController.bombReconnected -= HandleReconnect;
            }
            ActivePortController.OnApplicationQuit();
        }
    }
    
    private void HandleDisconnect()
    {
        Debug.LogWarning("Hardware disconnected");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHardwareDisconnect();
        }
    }

    private void HandleReconnect()
    {
        Debug.LogWarning("Hardware reconnected");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHardwareReconnect();
        }
    }

    public void HandleMessage(string message)
    {
        
        
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        message = message.Trim();
        if (message.StartsWith("$"))
        {
            message = message.Substring(1);
        }
        if (message.EndsWith("#"))
        {
            message = message.Substring(0, message.Length - 1);
        }

        int colonIndex = message.IndexOf(':');
        if (colonIndex == -1)
        {
            return;
        }

        string header = message.Substring(0, colonIndex);
        string commandPart = message.Substring(colonIndex + 1);

        int spaceIndex = commandPart.IndexOf(' ');
        if (spaceIndex == -1)
        {
            return;
        }

        string command = commandPart.Substring(0, spaceIndex);
        string value = commandPart.Substring(spaceIndex + 1);

        if (command != "set" && command != "status")
        {
            return;
        }

        if (header == "GYR" && command == "status")
        {
            string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                return;
            }

            float gx = float.Parse(parts[0]);
            float gy = float.Parse(parts[1]);
            float gz = float.Parse(parts[2]);

            Vector3 newGyro = new Vector3(gx, gy, gz);

            if (hardwareState.gyroValue == newGyro && hardwareState.hasAccelerometer == false)
            {
                return;
            }

            hardwareState.gyroValue = newGyro;
            hardwareState.hasAccelerometer = false;

            OnGyroChanged?.Invoke(hardwareState.gyroValue);
            {
                return;
            }
        }


        if (header == "IMU" && command == "status")
        {
            string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 6)
            {
                return;
            }

            float gx = float.Parse(parts[0]);
            float gy = float.Parse(parts[1]);
            float gz = float.Parse(parts[2]);

            float ax = float.Parse(parts[3]);
            float ay = float.Parse(parts[4]);
            float az = float.Parse(parts[5]);

            Vector3 newGyro = new Vector3(gx, gy, gz);
            Vector3 newAccel = new Vector3(ax, ay, az);

            if (hardwareState.gyroValue == newGyro && hardwareState.accelerometerValue == newAccel && hardwareState.hasAccelerometer == true)
            {
                return;
            }

            hardwareState.gyroValue = newGyro;
            hardwareState.accelerometerValue = newAccel;
            hardwareState.hasAccelerometer = true;

            OnImuChanged?.Invoke(hardwareState.gyroValue, hardwareState.accelerometerValue);
            {
                return;
            }
        }

        if (header == "NGM" && command == "status")
        {
            string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5)
            {
                return;
            }

            if (parts[0] == "0")
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    hardwareState.currentHardwareNGMValues[i - 1] = int.Parse(parts[i]);
                }
                OnNGMChanged?.Invoke(hardwareState.currentHardwareNGMValues);
            }
            else
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    hardwareState.LastNGMSubmissionValues[i - 1] = int.Parse(parts[i]);
                }
                OnNGMSubmit?.Invoke(hardwareState.LastNGMSubmissionValues);
            }
        }

        if (header == "SSM" && command == "status")
        {
            hardwareState.componentStates["SSM"] = int.Parse(value);
        }

        if (!hardwareState.componentStates.ContainsKey(header))
        {
            return;
        }

        int newInt = int.Parse(value);
        int oldInt = hardwareState.componentStates[header];

        if (oldInt == newInt)
        {
            return;
        }

        hardwareState.componentStates[header] = newInt;
        if (header == "FGM")
        {
            OnFGMChanged?.Invoke(newInt);
        }
        OnInputChanged?.Invoke(header, newInt);
    }

    public int GetDigital(string header)
    {
        return hardwareState.componentStates[header];
    }

    public int GetButton(string id)
    {
        return hardwareState.componentStates["BTN" + id];
    }
    public int GetSwitch(string id)
    {
        return hardwareState.componentStates["SWT" + id];
    }
    public int GetLed(string id)
    {
        return hardwareState.componentStates["LED" + id];
    }

    public int GetSSMStatus()
    {
        return hardwareState.componentStates["SSM"];
    }

    public int GetFGMStatus()
    {
        return hardwareState.componentStates["FGM"];
    }

    public void SendSetSeq(string header, int[] seq)
    {
        string strseq = "";
        for (int i = 0; i < seq.Length; i++)
        {
            strseq += seq[i] + " ";
        }
        strseq = strseq.Substring(0, strseq.Length - 1);

        string message = "$" + header + ":set " + strseq + "#";
        ActivePortController.SendToArduino(message);
    }

    public void SendSet(string header, int value)
    {
        string message = "$" + header + ":set " + value + "#";
        ActivePortController.SendToArduino(message);
    }

    public void TimerSet(int seconds)
    {
        string message = "$TMR:set " + seconds + "#";
        ActivePortController.SendToArduino(message);
    }

    public void TimerFlash(int times, int delay)
    {
        string message = "$TMR:flash " + times + " " + delay + "#";
        ActivePortController.SendToArduino(message);
    }

    public void TimerOnOff(int onOff)
    {
        string message = "$TMR:on " + onOff + "#";
        ActivePortController.SendToArduino(message);
    }

    public void RequestStatus(string header)
    {
        string message = "$" + header + ":status 0#";
        ActivePortController.SendToArduino(message);
    }

    public void ResetHardware(string header)
    {
        string message = "$" + header + ":reset 0#";
        ActivePortController.SendToArduino(message);
    }
}