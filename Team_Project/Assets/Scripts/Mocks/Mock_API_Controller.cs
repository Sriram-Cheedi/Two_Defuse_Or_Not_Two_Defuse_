using UnityEngine;
using System;
using System.Collections.Generic;


public class Mock_API_Controller : MonoBehaviour, API_Controller_Interface
{
    public event Action<string, int> OnInputChanged;
    public event Action<Vector3> OnGyroChanged;
    public event Action<Vector3, Vector3> OnImuChanged;

    public event Action<int[]> OnNGMSubmit;
    public event Action<int[]> OnNGMChanged;

    public event Action<int> OnFGMChanged;
    public event Action<string> OnMessageSent;


    private Dictionary<string, int> _componentStateManager = new Dictionary<string, int>();
    [SerializeField] private Vector3 _gyroValue = Vector3.zero;
    [SerializeField] private Vector3 _accelValue = Vector3.zero;
    public int[] currentHardwareNGMValues = new int[4];
    public int[] LastNGMSubmissionValues = new int[4];

    private bool _hasAccel = false;
    public Vector3 getGyroValue => _gyroValue;
    public Vector3 getAccelerometerValue => _accelValue;
    public bool getHasAccelerometer => _hasAccel;
    public Dictionary<string, int> getComponentStateManager => _componentStateManager;

    [SerializeField] public int mockSSMStatus = 0;
    public int[] GetLastNGMSubmissionValues() => LastNGMSubmissionValues;
    public int[] GetcurrentHardwareNGMValues() => currentHardwareNGMValues;



    public void Awake()
    {
        Debug.Log("<color=orange>MOCK API Controller: Awake</color>");
    }

    public void InitializeSelf()
    {
        // Debug.Log("<color=orange>MOCK: Initializing Simulated Controlller...</color>");

        _componentStateManager["BTN1"] = 0;
        _componentStateManager["BTN2"] = 0;
        _componentStateManager["BTN3"] = 0;
        _componentStateManager["BTN4"] = 0;
        _componentStateManager["BTN5"] = 0;

        _componentStateManager["SWT1"] = 0;
        _componentStateManager["SWT2"] = 0;
        _componentStateManager["SWT3"] = 0;

        _componentStateManager["LED1"] = 0;
        _componentStateManager["LED2"] = 0;
        _componentStateManager["LED3"] = 0;

        _componentStateManager["SSM"] = 0;

        _componentStateManager["FGM"] = 0;

        _componentStateManager["PWR"] = 0;


    }

    public void InitializeHardware()
    {
        Debug.Log("<color=orange>MOCK: Initializing Mock Hardware...</color>");
    }
    // public void UpdateSimulation()
    // {
    //     _gyroValue = new Vector3(Mathf.Sin(Time.time), Mathf.Cos(Time.time), 0);
    //     OnGyroChanged?.Invoke(_gyroValue);
    // }

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

        // GYRO Checking
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

            if (_gyroValue == newGyro && _hasAccel == false)
            {
                return;
            }

            _gyroValue = newGyro;
            _hasAccel = false;

            OnGyroChanged?.Invoke(_gyroValue);
            {
                return;
            }
        }

        // IMU Checking
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

            if (_gyroValue == newGyro && _accelValue == newAccel && _hasAccel == true)
            {
                return;
            }

            _gyroValue = newGyro;
            _accelValue = newAccel;
            _hasAccel = true;

            OnImuChanged?.Invoke(_gyroValue, _accelValue);
            {
                return;
            }
        }

        // NGM Checking
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
                    currentHardwareNGMValues[i - 1] = int.Parse(parts[i]);
                }

                OnNGMChanged?.Invoke(currentHardwareNGMValues);
            }
            else
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    LastNGMSubmissionValues[i - 1] = int.Parse(parts[i]);
                }

                OnNGMSubmit?.Invoke(LastNGMSubmissionValues);
            }
        }

        // SSM Checking
        if (header == "SSM" && command == "status")
        {
            _componentStateManager["SSM"] = int.Parse(value);
        }

        // General Button Checking 
        if (!_componentStateManager.ContainsKey(header))
        {
            return;
        }

        int newInt = int.Parse(value);
        int oldInt = _componentStateManager[header];

        _componentStateManager[header] = newInt;

        // FGM Checking
        if (header == "FGM")
        {
            OnFGMChanged?.Invoke(newInt);
        }

        OnInputChanged?.Invoke(header, newInt);
    }

    public void SendSet(string header, int value)
    {
        string message = "$" + header + ":set " + value + "#";
        OnMessageSent?.Invoke(message);
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
        OnMessageSent?.Invoke(message);

    }

    public void SendSetSeqSTRING(string header, string[] seq)
    {
        string strseq = "";
        for (int i = 0; i < seq.Length; i++)
        {
            strseq += seq[i] + " ";
        }
        strseq = strseq.Substring(0, strseq.Length - 1);

        string message = "$" + header + ":set " + strseq + "#";
        OnMessageSent?.Invoke(message);
    }
    
    public int GetButton(string id) => _componentStateManager.ContainsKey("BTN" + id) ? _componentStateManager["BTN" + id] : 0;

    public void TimerSet(int seconds)
    {
        string message = "$TMR:set " + seconds + "#";
        OnMessageSent?.Invoke(message);
    }

    public void ResetHardware(string header)
    {
        string message = "$" + header + ":reset 0#";
        OnMessageSent?.Invoke(message);
    }
    public int GetDigital(string header) => _componentStateManager[header];
    public int GetSSMStatus() => mockSSMStatus;
    public Vector3 GetAccel() => _accelValue;
    public int GetSwitch(string id) => _componentStateManager["SWT" + id];
    public int GetLed(string id) =>  _componentStateManager["LED" + id];
    public void TimerFlash(int times, int delay) {         
        string message = "$TMR:flash " + times + " " + delay + "#";
        OnMessageSent?.Invoke(message);
    }
    public void TimerOnOff(int onOff) 
    {         
        string message = "$TMR:on " + onOff + "#";
        OnMessageSent?.Invoke(message);
    }

    public void RequestStatus(string header)
    {
        string message = "$" + header + ":status 0#";
        OnMessageSent?.Invoke(message);
    }
}