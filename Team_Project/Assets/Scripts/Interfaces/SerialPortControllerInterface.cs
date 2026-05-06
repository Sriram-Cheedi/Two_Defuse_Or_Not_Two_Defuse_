using UnityEngine;
using System;
using System.IO.Ports;

public interface I_SerialPortController
{
    public event Action<string> Received;

    // Getters
    public string getPortID { get; }
    public int getBaudRate { get; }
    public SerialPort getSerialPort { get; }
    void Start();
    void Update();
    public void SendToArduino(string message);
    void OnApplicationQuit();
}

