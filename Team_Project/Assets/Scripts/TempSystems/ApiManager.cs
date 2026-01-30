using UnityEngine;
using System;
using System.Collections.Generic;

public class ApiManager : MonoBehaviour
{
    public SerialManager serial;

    public event Action<string, int> OnInputChanged;     
    public event Action<string, float> OnGyroChanged;      

    Dictionary<string, int> ComponentStateManager = new Dictionary<string, int>();
    Dictionary<string, float> GyroStates = new Dictionary<string, float>();

    void Awake()
    {
        ComponentStateManager["BTN1"] = 0;
        ComponentStateManager["BTN2"] = 0;
        ComponentStateManager["BTN3"] = 0;
        ComponentStateManager["BTN4"] = 0;
        ComponentStateManager["BTN5"] = 0;

        ComponentStateManager["SWT1"] = 0;
        ComponentStateManager["SWT2"] = 0;
        ComponentStateManager["SWT3"] = 0;

        ComponentStateManager["LED1"] = 0;
        ComponentStateManager["LED2"] = 0;
        ComponentStateManager["LED3"] = 0;

        GyroStates["GYR1"] = 0f;
        GyroStates["GYR2"] = 0f;
        GyroStates["GYR3"] = 0f; 
    }

    void OnEnable()
    {
        if (serial != null){
            serial.Received += HandleMessage;
        }
    }

    void OnDisable()
    {
        if (serial != null){
            serial.Received -= HandleMessage;
        }
    }

 
    private void HandleMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        int colonIndex = message.IndexOf(':');

        if (colonIndex == -1)
        {
            return;
        }

        string header = message.Substring(0, colonIndex);     
        string CommandPart = message.Substring(colonIndex + 1);   

        int spaceIndex = CommandPart.IndexOf(' ');

        if (spaceIndex == -1)
        {
            return;
        }

        string Command = CommandPart.Substring(0, spaceIndex);       
        string Value = CommandPart.Substring(spaceIndex + 1); 

        if (Command != "set" && Command != "status")
        {
            return;
        }

        if (header.StartsWith("GYR"))
        {
            if (!GyroStates.ContainsKey(header))
            {
                return;
            }

            float NewValue = float.Parse(Value);
            float OldValue = GyroStates[header];

            if (OldValue == NewValue)
            {
                return;
            }

            GyroStates[header] = NewValue;
            OnGyroChanged?.Invoke(header, NewValue);
            return;
        }

        if (!ComponentStateManager.ContainsKey(header))
        {
            return;
        }

        int newInt = int.Parse(Value);
        int oldInt = ComponentStateManager[header];

        if (oldInt == newInt)
        {
            return;
        }

        ComponentStateManager[header] = newInt;
        OnInputChanged?.Invoke(header, newInt);
    }

    public int GetDigital(string header)
    {
        return ComponentStateManager[header];
    }

    public float GetGyro(string header)
    {
        return GyroStates[header];
    }

    public int GetButton(string id)
    { 
        return ComponentStateManager["BTN" + id]; 
    }
    public int GetSwitch(string id)
    { 
        return ComponentStateManager["SWT" + id]; 
    }
    public int GetLed(string id)
    { 
        return ComponentStateManager["LED" + id]; 
    }


    public void SendSet(string header, int value)
    {
        string message = "$" + header + ":set " + value + "#";
        serial.SendToArduino(message);
    }

    public void SendSetGyro(string header, float value)
    {
        string message = "$" + header + ":set " + value + "#";
        serial.SendToArduino(message);
    }
    public void TimerSet(int seconds)
    {
        string message = "$TMR:set " + seconds + "#";
        serial.SendToArduino(message);
    }

    public void TimerFlash(int times, int delay)
    {
        string message = "$TMR:flash " + times + " " + delay + "#";
        serial.SendToArduino(message);
    }

    public void TimerOnOff(int OnOff)
    {
        string message = "$TMR:on " + OnOff + "#";
        serial.SendToArduino(message);
    }

    public void RequestStatus(string header)
    {
        string message = "$" + header + ":status 0#";
        serial.SendToArduino(message);
    }

    public void Reset(string header)
    {
        string message = "$" + header + ":reset 0#";
        serial.SendToArduino(message);
    }
}