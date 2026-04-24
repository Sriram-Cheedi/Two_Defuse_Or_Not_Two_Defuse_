using System;
using System.Collections;
using System.Collections.Generic;
using PersistantSystems.Software.GameStateMachine;
using Puzzles;
using UnityEngine;

public class WireCut : Puzzle
{
    public WireMapping wireMapping;
    private Dictionary<string, VerletInsulatedWire.WireColour> wireDict = new Dictionary<string, VerletInsulatedWire.WireColour>();
    VerletInsulatedWire.WireColour hardwareWireColor =  VerletInsulatedWire.WireColour.None;
    VerletInsulatedWire.WireColour softwareWireColor =  VerletInsulatedWire.WireColour.None;
    private float timeToCut = 3f;
    private string cutColor = null;
    public static event Action HasCutSoftwareWire;
    public static event Action HasCutAnyWire;
    public static event Action<float> HasCutWireWithinTimeWindow;
    private bool hasCut = false;

    private void Awake()
    {
        InitializeWireDict();
    }

    private void InitializeWireDict()
    {
        wireDict["WR1"] = VerletInsulatedWire.WireColour.Red;
        wireDict["WR2"] = VerletInsulatedWire.WireColour.Blue;
        wireDict["WR3"] = VerletInsulatedWire.WireColour.Grey;
        wireDict["WR4"] = VerletInsulatedWire.WireColour.Brown;
        wireDict["grey"] = VerletInsulatedWire.WireColour.Grey;
        wireDict["brown"] = VerletInsulatedWire.WireColour.Brown;
        wireDict["blue"] = VerletInsulatedWire.WireColour.Blue;
        wireDict["red"] = VerletInsulatedWire.WireColour.Red;
    }

    private void OnEnable()
    {
        API_Controller.OnInputChanged += HandleHardwareWire;
    }

    private void OnDisable()
    {
        API_Controller.OnInputChanged -= HandleHardwareWire;
    }

    private void Update()
    {
        // if (hasCut)
        //     return;

        if (wireMapping == null)
        {
            Debug.LogError("WireMapping is null.");
            return;
        }

        if ( VerletInsulatedWire.CutWireColor.HasValue)
        {
            cutColor = VerletInsulatedWire.CutWireColor.Value.ToString().ToLower();
        }
        
        // Debug.Log("Cut wire: " + cutColor);
        // Debug.Log("Expected wire: " + wireMapping.cutWire);
        if (VerletInsulatedWire.CutWireColor.HasValue)
        {
            softwareWireColor = VerletInsulatedWire.CutWireColor.Value;
        }

        if (softwareWireColor != VerletInsulatedWire.WireColour.None) // software wire is cut first
        {
            CheckWireCutColor(softwareWireColor, hardwareWireColor);
            if (!hasCut)
            {
                HasCutSoftwareWire?.Invoke();
                HasCutAnyWire?.Invoke();
                hasCut = true;
            }
            
        }
        else if (hardwareWireColor != VerletInsulatedWire.WireColour.None) // hardware wire is cut first
        {
            CheckWireCutColor(hardwareWireColor, softwareWireColor);
            if (!hasCut)
            {
                HasCutAnyWire?.Invoke();
                hasCut = true;
            }
        }
        VerletInsulatedWire.CutWireColor = null;
    }

    private void CheckWireCutColor(VerletInsulatedWire.WireColour wireCutFirst, VerletInsulatedWire.WireColour wireCutLater)
    {
        if (wireCutFirst != wireDict[wireMapping.cutWire])
        {
            //starts the timer
            timeToCut -= Time.deltaTime;
            if (timeToCut <= 0f || wireCutLater != VerletInsulatedWire.WireColour.None)
            {//game lost if timer hits 0 or any physical color is cut
                TriggerFailure(PuzzleGame.WCM);
                GameManager.Instance.EndGame();
            }
        }
        else//software wire cut is correct wait for physical wire color
        {
            //starts the timer
            timeToCut -= Time.deltaTime;
            if (timeToCut <= 0f)
            {
                TriggerFailure(PuzzleGame.WCM);
                //game lost if timer hits 0
                GameManager.Instance.EndGame();
            }
            else
            {
                //if it's the same we are good
                if (wireCutFirst == wireCutLater)
                {
                    HasCutWireWithinTimeWindow?.Invoke(3.0f - (float)GameManager.Instance.BombTimer);
                    TriggerSuccess(PuzzleGame.WCM);
                    GameManager.Instance.WinGame();
                }
            }
        }
    }

    private void CutWithinTimeWindow()
    {
        timeToCut -= Time.deltaTime;
        if (timeToCut <= 0f)
        {
            GameManager.Instance.EndGame();
        }
        else
        {
            if (hardwareWireColor == VerletInsulatedWire.CutWireColor)
            {
                GameManager.Instance.WinGame();
            }
        }
    }

    private void HandleHardwareWire(string header, int value)
    {
        if(value == 1) return;
        if(header != "WR1" && header != "WR2" &&  header != "WR3" && header != "WR4") return;
        hardwareWireColor = wireDict[header];
    }
}