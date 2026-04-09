using System;
using System.Collections.Generic;
using UnityEngine;

public interface API_Controller_Interface
{
    public static event Action<string, int> OnInputChanged;
    public event Action<Vector3> OnGyroChanged;
    public event Action<Vector3, Vector3> OnImuChanged;
    
    public event Action<int[]> OnNGMSubmit;
    public event Action<int[]> OnNGMChanged;
// Getters
    Dictionary<string, int> getComponentStateManager { get; }
    Vector3 getAccelerometerValue { get; }
    bool getHasAccelerometer { get; }
    Vector3 getGyroValue { get; }

    // Methods
    public void InitializeSelf();
    public void InitializeHardware();
    public void HandleMessage(string message);
    public int GetDigital(string header);
    public int GetButton(string id);
    public int GetSwitch(string id);
    public int GetLed(string id);
    public int GetSSMStatus();
    public int GetMazeStatus();
    public void SendSetSeq(string header, int[] value);
    public void SetMazeStatus(string header, int value);
    public void SendSetSeqSTRING(string header, string[] value);
    public void SendMazeSeq(string header, string value);
    public void SendSet(string header, int value);
    public void TimerSet(int seconds);
    public void TimerFlash(int times, int delay);
    public void TimerOnOff(int onOff);
    public void RequestStatus(string header);
    public void ResetHardware(string header);
}