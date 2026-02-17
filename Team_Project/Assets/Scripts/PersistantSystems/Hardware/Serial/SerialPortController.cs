using UnityEngine;
using System;
using System.IO.Ports;


public class SerialPortController : MonoBehaviour, I_SerialPortController
{
    // private readonly string _portID = "COM3";
    private string _portID = "COM4";
    public int _baudRate = 115200;
    private SerialPort _serialPort;
    private string buffer = "";
    public event Action<string> Received;
    public string getPortID => _portID;
    public int getBaudRate => _baudRate;
    public SerialPort getSerialPort => _serialPort;
    
    public void Start() { }
    public void Awake()
    {
        string[] AvailablePorts = SerialPort.GetPortNames();
        foreach (var PortName in AvailablePorts)
        {
            SerialPort CheckPort = null;
            try
            {
                CheckPort = new SerialPort(PortName, _baudRate);
                CheckPort.ReadTimeout = 300;
                Debug.Log("Checking port: " + PortName);
                CheckPort.Open();
                CheckPort.Write("$PING#");
                System.Threading.Thread.Sleep(50);
                string HandshakeMessage = "";
                
                float startTime = Time.realtimeSinceStartup;

                while (Time.realtimeSinceStartup - startTime < 1.0f)
                {
                    HandshakeMessage += CheckPort.ReadExisting();
                    if (HandshakeMessage.Contains("$PONG#"))
                    {
                       _portID = PortName;
                       Debug.Log("Arduino found on port:" + _portID);
                       break;
                    }
                }
                CheckPort.Close();
                if (_portID == PortName)
                {
                    break;
                }
            }
            catch {} 
            finally
            {
                if (CheckPort != null && CheckPort.IsOpen)
                {
                    CheckPort.Close();
                }
            }
        }
        if(_portID == null || _portID == "")
        {
            return;
        }
        try
        {
            Debug.Log("Attempting to Open Port on: " + _portID + " and baud rate: " + _baudRate);
            _serialPort = new SerialPort(_portID, _baudRate);
            _serialPort.ReadTimeout = 50;
            _serialPort.Open();

            Debug.Log("Protocol Active on " + _portID);
        }
        catch (Exception error)
        {
            Debug.LogError("Could not open port: " + error.Message);
            this.enabled = false;
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
    
    // public void SendToArduino(string message)
    // {
    //     if (_serialPort == null || !_serialPort.IsOpen)
    //     {
    //         Debug.LogError("Serial port is not open. Cannot send: " + message);
    //         return;
    //     }
    //     _serialPort.Write(message);
    //     Debug.Log("Sent: " + message);
    // }

    public void SendToArduino(string message)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
        {
            Debug.LogError("Serial port is not open.");
            return;
        }
        try
        {
            _serialPort.Write(message);
            Debug.Log("Sent: " + message);
        }
        catch (Exception e)
        {
            Debug.LogError("Serial write failed (device disconnected?): " + e.Message);

            try { _serialPort.Close(); } catch {}
            _serialPort = null;
        }
    }


    public void OnApplicationQuit()
    {
        if (_serialPort != null && _serialPort.IsOpen)
            _serialPort.Close();
    }
}

