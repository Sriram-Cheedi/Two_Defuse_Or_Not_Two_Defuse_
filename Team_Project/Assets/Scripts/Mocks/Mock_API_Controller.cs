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

    private Dictionary<string, int> _componentStateManager = new Dictionary<string, int>();
    [SerializeField] private Vector3 _gyroValue = Vector3.zero;
    [SerializeField] private Vector3 _accelValue = Vector3.zero;
    private bool _hasAccel = false;
    public Vector3 getGyroValue => _gyroValue;
    public Vector3 getAccelerometerValue => _accelValue;
    public bool getHasAccelerometer => _hasAccel;
    public Dictionary<string, int> getComponentStateManager => _componentStateManager;
    
    [SerializeField] public int mockSSMStatus = 0;

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
        
        _componentStateManager["FGM"] = 0;


    }

    public void InitializeHardware()
    {
        Debug.Log("<color=orange>MOCK: Initializing Mock Hardware...</color>");
    }
    public void UpdateSimulation()
    {
        _gyroValue = new Vector3(Mathf.Sin(Time.time), Mathf.Cos(Time.time), 0);
        OnGyroChanged?.Invoke(_gyroValue);
    }

    public void HandleMessage(string message) => Debug.Log($"<color=orange>MOCK: Handle Message</color>");
    public void SendMazeSeq(string header, string value)
    {
    }

    public void SendSet(string header, int value) => Debug.Log("<color=orange>MOCK: SendSet: </color>" + header + ":set " + value + "#");
    public void SendSetSeq(string header, int[] value) => Debug.Log("<color=orange>MOCK: SendSetSeq: </color>" + header + ":set " + value + "#");
    public void SendSetSeqSTRING(string header, string[] value) =>  Debug.Log("<color=orange>MOCK: SendSetSeqSTRING: </color>" + header + ":set " + value + "#");
    public int GetButton(string id) => _componentStateManager.ContainsKey("BTN" + id) ? _componentStateManager["BTN" + id] : 0;
    public void TimerSet(int seconds) => Debug.Log("<color=orange>MOCK: Timer Set: </color>" + seconds);
    public void ResetHardware(string header) => Debug.Log("<color=orange>MOCK: Reset: </color>" + header);
    public int GetDigital(string h) => 0;
    public int GetSSMStatus() => mockSSMStatus;
    public Vector3 GetAccel() => _accelValue;
    public int GetSwitch(string id) => 0;
    public int GetLed(string id) => 0;
    public void TimerFlash(int t, int d) { return; }
    public void TimerOnOff(int o) { return; }
    public void RequestStatus(string h) { return; }
    public int GetFGMStatus() => _componentStateManager.ContainsKey("FGM") ? _componentStateManager["FGM"] : 0;
}