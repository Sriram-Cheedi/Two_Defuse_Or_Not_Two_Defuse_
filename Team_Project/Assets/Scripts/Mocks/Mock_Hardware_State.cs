using System;
using UnityEngine;

public class Mock_Hardware_State : MonoBehaviour
{
    public int activateNGM;
    
    public int activateSSM;
    public int[] SSMSequence = new int[4];
    public int[] NGMNumbers = new int[4];
    public int timer;
    public int SSMStatus;


    private void HandleMessageReceived(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        message = message.Trim();
        if (message.StartsWith("$"))
        {
            message = message.Substring(1);
        }
        if (message.EndsWith("#"))
        {
            message = message.Substring(0, message.Length - 1);
        }

        int colonIndex = message.IndexOf(':');
        if (colonIndex == -1)
        {
            return;
        }

        string header = message.Substring(0, colonIndex);
        string commandPart = message.Substring(colonIndex + 1);

        int spaceIndex = commandPart.IndexOf(' ');
        if (spaceIndex == -1)
        {
            return;
        }

        string command = commandPart.Substring(0, spaceIndex);
        string value = commandPart.Substring(spaceIndex + 1);
        if (command == "set")
        {
            if (header == "SSM")
            {
                if (value.StartsWith('['))
                {
                    value = value.Trim('[', ']');
                    string[] numbers = value.Split(',');
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        SSMSequence[i] = int.Parse(numbers[i]);
                    }
                }
                else
                {
                    activateSSM  = int.Parse(value);
                }
            }
            
            if (header == "NGM")
            {
                if (value.StartsWith('['))
                {
                    value = value.Trim('[', ']');
                    string[] numbers = value.Split(',');
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        NGMNumbers[i] = int.Parse(numbers[i]);
                    }
                }
                else
                {
                    activateNGM  = int.Parse(value);
                }
            }
        }
    }
}


