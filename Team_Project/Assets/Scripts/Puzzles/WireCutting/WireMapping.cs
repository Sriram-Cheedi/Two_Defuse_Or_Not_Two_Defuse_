using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
using UnityEngine;

public class WireMapping : MonoBehaviour
{
    public Puzzles.Controllers.SSM_Controller ssm_controller;
    public int finalSSMColour;
    public string cutWire;
    
    
    private void Start()
    {
        finalSSMColour = ssm_controller.data.correctSequence[ssm_controller.data.correctSequence.Length - 1];
    }

    private void Update()
    {
        if(finalSSMColour == 0)
        {
            cutWire = "red";
        }
        else if(finalSSMColour == 1)
        {
            cutWire = "green";
        }
        else if(finalSSMColour == 2)
        {
            cutWire = "grey";
        }
        else if(finalSSMColour == 3)
        {
            cutWire = "brown";
        }
    }
}