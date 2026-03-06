using UnityEngine;
using System;
using System.IO.Ports;


public class SerialPortController : MonoBehaviour, I_SerialPortController
{
    // private readonly string _portID = "COM3";
    private string _portID = "COM4";
    [SerializeField] private int _baudRate = 115200;
    private SerialPort _serialPort;
    private string buffer = "";
    public event Action<string> Received;
    public event Action bombDisconnected;
    public event Action bombReconnected;
    public string getPortID => _portID;
    public int getBaudRate => _baudRate;
    public SerialPort getSerialPort => _serialPort;
    
    private float reconnectTimer = 1.0f;
    private float reconnectAgainTimer = 0f;
    private bool isConnected = false;
    
    private float pingBombTimer = 0.5f;
    private float pongTimeoutSeconds = 2.0f;
    private float lastPingTime;
    private float lastPongTime;
    
    public void Start() { }
    public void Awake()
    {
        Debug.Log("SerialPortController Awake on: " + gameObject.name);
        Connect();

    }

    public void Update()
    {
        if (_serialPort == null || !_serialPort.IsOpen)
        {
            if (Time.realtimeSinceStartup >= reconnectAgainTimer)
            {
                reconnectAgainTimer = Time.realtimeSinceStartup + reconnectTimer;
                Connect();
            }
            return;
        }

        if (Time.realtimeSinceStartup - lastPingTime >= pingBombTimer)
        {
            lastPingTime = Time.realtimeSinceStartup;
            try
            {
                _serialPort.Write("$PING#");
            }
            catch (Exception e)
            {
                Debug.LogError("Serial write failed (device disconnected?): " + e.Message);
                Disconnect();
                return;
            }
        }

        if (Time.realtimeSinceStartup - lastPongTime >= pongTimeoutSeconds)
        {
            Debug.Log("Pong timeout");
            Disconnect();
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
                if (message == "PONG")
                {
                    lastPongTime = Time.realtimeSinceStartup;
                    continue;
                }
                Debug.Log("<color=green>Message Received: </color>" + message);
                Received?.Invoke(message);

            }
        }
        catch (TimeoutException)
        {
        }
        catch (Exception error)
        {
            Debug.LogError("Serial error: " + error.Message);
            Disconnect();
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
            Debug.Log("<color=orange>Message Sent: </color>" + message);
        }
        catch (Exception e)
        {
            Debug.LogError("Serial write failed (device disconnected?): " + e.Message);
            Disconnect();
        }
    }

    private void Disconnect()
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        catch { }
        _serialPort = null;
        buffer = "";
        if (isConnected)
        {
            isConnected = false;
            bombDisconnected?.Invoke();
        }
    }
    public bool Connect()
    {
        if(_serialPort != null && _serialPort.IsOpen)
        {
            if (!isConnected)
            {
                isConnected = true;
                lastPingTime = Time.realtimeSinceStartup;
                lastPongTime = Time.realtimeSinceStartup;
                bombReconnected?.Invoke();
            }
            return true;
        }
        
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
            return false;
        }
        try
        {
            Debug.Log("Attempting to Open Port on: " + _portID + " and baud rate: " + _baudRate);
            _serialPort = new SerialPort(_portID, _baudRate);
            _serialPort.ReadTimeout = 50;
            Debug.Log("Opening serial port: " + _portID + "\n" + Environment.StackTrace);
            _serialPort.Open();

            Debug.Log("Protocol Active on " + _portID);
            if (!isConnected)
            {
                isConnected = true;
                lastPingTime = Time.realtimeSinceStartup;
                lastPongTime = Time.realtimeSinceStartup;
                bombReconnected?.Invoke();
            }
            return true;
        }
        catch (Exception error)
        {
            Debug.LogError("Could not open port: " + error.Message);
            Disconnect();
            return false;
        }
        
    }

    public void OnApplicationQuit()
    {
        if (_serialPort != null && _serialPort.IsOpen)
            _serialPort.Close();
    }
}

