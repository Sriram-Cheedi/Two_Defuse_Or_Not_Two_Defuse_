using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework;

public class API_Controller : MonoBehaviour, API_Controller_Interface
{
    public I_SerialPortController ActivePortController { get; private set; }
    public event Action<string, int> OnInputChanged;
    public event Action<Vector3> OnGyroChanged;
    public event Action<Vector3, Vector3> OnImuChanged;
    private Dictionary<string, int> _componentStateManager = new Dictionary<string, int>();
    private Vector3 _gyroValue = Vector3.zero;
    private Vector3 _accelerometerValue = Vector3.zero;
    private bool _hasAccelerometer = false;

    // Getters
    public Vector3 getGyroValue => _gyroValue;
    public Vector3 getAccelerometerValue => _accelerometerValue;
    public bool getHasAccelerometer => _hasAccelerometer;
    public Dictionary<string, int> getComponentStateManager => _componentStateManager;
    private void OnDestroy() => Exit();

    public void Initialize(I_SerialPortController manualController = null)
    {
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

        if (manualController != null)
        {
            ActivePortController = manualController;
            Debug.Log("Setting Serial Controller");
        }
        else
            ActivePortController = GetComponentInChildren<SerialPortController>();


        if (ActivePortController != null)
        {
            Debug.Log("Setting Handle Message for Real Serial Controller");
            ActivePortController.Received += HandleMessage;
        }
    }

    void Exit()
    {
        if (ActivePortController != null)
        {
            ActivePortController.Received -= HandleMessage;
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

            if (_gyroValue == newGyro && _hasAccelerometer == false)
            {
                return;
            }

            _gyroValue = newGyro;
            _hasAccelerometer = false;

            OnGyroChanged?.Invoke(_gyroValue);
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

            if (_gyroValue == newGyro && _accelerometerValue == newAccel && _hasAccelerometer == true)
            {
                return;
            }

            _gyroValue = newGyro;
            _accelerometerValue = newAccel;
            _hasAccelerometer = true;

            OnImuChanged?.Invoke(_gyroValue, _accelerometerValue);
            {
                return;
            }
        }

        if (!_componentStateManager.ContainsKey(header))
        {
            return;
        }

        int newInt = int.Parse(value);
        int oldInt = _componentStateManager[header];

        if (oldInt == newInt)
        {
            return;
        }

        _componentStateManager[header] = newInt;
        OnInputChanged?.Invoke(header, newInt);
    }

    public int GetDigital(string header)
    {
        return _componentStateManager[header];
    }

    public int GetButton(string id)
    {
        return _componentStateManager["BTN" + id];
    }
    public int GetSwitch(string id)
    {
        return _componentStateManager["SWT" + id];
    }
    public int GetLed(string id)
    {
        return _componentStateManager["LED" + id];
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