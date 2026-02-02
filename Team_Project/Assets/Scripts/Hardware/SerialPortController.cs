using UnityEngine;
using System;
using System.IO.Ports;

public class SerialPortController : MonoBehaviour, I_SerialPortController
{
    private readonly string _portID = "COM1";
    public int _baudRate = 115200;

    private SerialPort _serialPort;
    private string buffer = "";

    public event Action<string> Received;

    public string getPortID => _portID;
    public int getBaudRate => _baudRate;
    public SerialPort getSerialPort => _serialPort;

    public void Start()
    {
        try
        {
            _serialPort = new SerialPort(_portID, _baudRate);
            _serialPort.ReadTimeout = 50;
            _serialPort.Open();
            Debug.Log("Protocol Active on " + _portID);
        }
        catch (Exception error)
        {
            Debug.LogError("Could not open port: " + error.Message);
            _serialPort = null;
        }
    }

    public void Update()
    {
        if (_serialPort == null || !_serialPort.IsOpen)
        {
            return;
        }

        try
        {
            buffer += _serialPort.ReadExisting();

            while (true)
            {
                int start = buffer.IndexOf('$');

                if (start == -1)
                {
                    break;
                }

                int end = buffer.IndexOf('#', start + 1);

                if (end == -1)
                {
                    break;
                }

                string message = buffer.Substring(start + 1, end - start - 1);
                buffer = buffer.Substring(end + 1);
                Debug.Log("Message Received: " + message);
                Received?.Invoke(message);

            }
        }
        catch (TimeoutException)
        {
        }
        catch (Exception error)
        {
            Debug.LogError("Serial error: " + error.Message);
        }
    }

    public void SendToArduino(string message)
    {
        _serialPort.Write(message);

        Debug.Log("Sent: " + message);
    }

    public void OnApplicationQuit()
    {
        if (_serialPort != null && _serialPort.IsOpen)
            _serialPort.Close();
    }
}

