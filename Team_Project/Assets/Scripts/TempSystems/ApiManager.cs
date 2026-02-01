using UnityEngine;
using System;
using System.Collections.Generic;

public class ApiManager : MonoBehaviour
{
    public SerialManager serial;

    public event Action<string, int> OnInputChanged;
    public event Action<Vector3> OnGyroChanged;
    public event Action<Vector3, Vector3> OnImuChanged;
    private Dictionary<string, int> ComponentStateManager = new Dictionary<string, int>();

    private Vector3 GyroValue = Vector3.zero;
    private Vector3 AccelerometerValue = Vector3.zero;
    private bool HasAccelerometer = false;

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
        if (string.IsNullOrWhiteSpace(message)){
            return;
        }

        message = message.Trim();
        if (message.StartsWith("$")){
            message = message.Substring(1);
        }
        if (message.EndsWith("#")){
            message = message.Substring(0, message.Length - 1);
        }

        int colonIndex = message.IndexOf(':');
        if (colonIndex == -1){
            return;
        }

        string header = message.Substring(0, colonIndex);
        string commandPart = message.Substring(colonIndex + 1);

        int spaceIndex = commandPart.IndexOf(' ');
        if (spaceIndex == -1){
            return;
        }

        string command = commandPart.Substring(0, spaceIndex);
        string value = commandPart.Substring(spaceIndex + 1);

        if (command != "set" && command != "status"){
            return;
        }

        if (header == "GYR" && command == "status")
        {
            string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3){
                return;
            }

            float gx = float.Parse(parts[0]);
            float gy = float.Parse(parts[1]);
            float gz = float.Parse(parts[2]);

            Vector3 newGyro = new Vector3(gx, gy, gz);

            if (GyroValue == newGyro && HasAccelerometer == false){
                return;
            }

            GyroValue = newGyro;
            HasAccelerometer = false;

            OnGyroChanged?.Invoke(GyroValue);{
                return;
            }
        }


        if (header == "IMU" && command == "status")
        {
            string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 6){
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

            if (GyroValue == newGyro && AccelerometerValue == newAccel && HasAccelerometer == true){
                return;
            }

            GyroValue = newGyro;
            AccelerometerValue = newAccel;
            HasAccelerometer = true;

            OnImuChanged?.Invoke(GyroValue, AccelerometerValue);{
                return;
            }
        }

        if (!ComponentStateManager.ContainsKey(header)){
            return;
        }

        int newInt = int.Parse(value);
        int oldInt = ComponentStateManager[header];

        if (oldInt == newInt){
            return;
        }

        ComponentStateManager[header] = newInt;
        OnInputChanged?.Invoke(header, newInt);
    }

    public int GetDigital(string header){
        return ComponentStateManager[header];
    }
    public Vector3 GetGyro(){
        return GyroValue;
    }
    public bool GetHasAccelerometer(){
        return HasAccelerometer;
    }
    public Vector3 GetAccel(){
        return AccelerometerValue;
    }

    public int GetButton(string id){
        return ComponentStateManager["BTN" + id];
    }
    public int GetSwitch(string id){
        return ComponentStateManager["SWT" + id];
    }
    public int GetLed(string id){
        return ComponentStateManager["LED" + id];
    }

    public void SendSet(string header, int value)
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

    public void TimerOnOff(int onOff)
    {
        string message = "$TMR:on " + onOff + "#";
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