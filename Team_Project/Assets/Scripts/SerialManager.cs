using UnityEngine;
using System;
using System.IO.Ports;

public class SerialManager : MonoBehaviour
{
    public string port = "COM1";
    public int baudRate = 115200;

    private SerialPort serial;
    private string buffer = "";

    public event Action<string> Received;

    void Start()
    {
        try
        {
            serial = new SerialPort(port, baudRate);
            serial.ReadTimeout = 50;
            serial.Open();
            Debug.Log("Protocol Active on " + port);
        }
        catch (Exception error)
        {
            Debug.LogError("Could not open port: " + error.Message);
            serial = null;
        }
    }

    void Update()
    {
        if (serial == null || !serial.IsOpen){
            return;
        }

        try
        {
            buffer += serial.ReadExisting();

            while (true)
            {
                int start = buffer.IndexOf('$');

                if (start == -1){
                    break;
                }

                int end = buffer.IndexOf('#', start + 1);

                if (end == -1){
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
        serial.Write(message);

        Debug.Log("Sent: " + message);
    }

    void OnApplicationQuit()
    {
        if (serial != null && serial.IsOpen)
            serial.Close();
    }
}

