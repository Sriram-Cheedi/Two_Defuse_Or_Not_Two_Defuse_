using UnityEngine;
using System;
using System.Collections.Generic;

public class Mock_API_Controller : API_Controller_Interface
{
    public event Action<string, int> OnInputChanged;
    public event Action<Vector3> OnGyroChanged;
    public event Action<Vector3, Vector3> OnImuChanged;

    private Dictionary<string, int> _componentStateManager = new Dictionary<string, int>();
    private Vector3 _gyroValue = Vector3.zero;
    private Vector3 _accelValue = Vector3.zero;
    private bool _hasAccel = false;
    public Vector3 getGyroValue => _gyroValue;
    public Vector3 getAccelerometerValue => _accelValue;
    public bool getHasAccelerometer => _hasAccel;
    public Dictionary<string, int> getComponentStateManager => _componentStateManager;

    public void Initialize(I_SerialPortController manualController = null)
    {
        Debug.Log("<color=orange>MOCK: Initializing Simulated Hardware...</color>");

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

    }
    public void UpdateSimulation()
    {
        _gyroValue = new Vector3(Mathf.Sin(Time.time), Mathf.Cos(Time.time), 0);
        OnGyroChanged?.Invoke(_gyroValue);
    }

    public void HandleMessage(string message) => Debug.Log($"MOCK HANDLE MESSAGE");

    public void SendSet(string header, int value) => Debug.Log("MOCK SEND SET: $" + header + ":set " + value + "#");

    public int GetButton(string id) => _componentStateManager.ContainsKey("BTN" + id) ? _componentStateManager["BTN" + id] : 0;
    public void TimerSet(int seconds) => Debug.Log($"MOCK TIMER: Set to {seconds}s");
    public void ResetHardware(string header) => Debug.Log($"MOCK RESET: {header}");
    public int GetDigital(string h) => 0;
    public Vector3 GetAccel() => _accelValue;
    public int GetSwitch(string id) => 0;
    public int GetLed(string id) => 0;
    public void TimerFlash(int t, int d) { return; }
    public void TimerOnOff(int o) { return; }
    public void RequestStatus(string h) { return; }
}