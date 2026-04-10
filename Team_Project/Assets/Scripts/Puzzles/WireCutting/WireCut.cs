using System;
using System.Collections;
using System.Collections.Generic;
using PersistantSystems.Software.GameStateMachine;
using UnityEngine;

public class WireCut : MonoBehaviour
{
    public WireMapping wireMapping;
    private Dictionary<string, VerletWire.WireColor> wireDict = new Dictionary<string, VerletWire.WireColor>();
    VerletWire.WireColor hardwareWireColor = VerletWire.WireColor.None;
    VerletWire.WireColor softwareWireColor = VerletWire.WireColor.None;
    private float timeToCut = 3f;
    private string cutColor = null;
    public static event Action HasCutSoftwareWire;
    public static event Action HasCutAnyWire;

    private void Awake()
    {
        InitializeWireDict();
    }

    private void InitializeWireDict()
    {
        wireDict["WR1"] = VerletWire.WireColor.Brown;
        wireDict["WR2"] = VerletWire.WireColor.Grey;
        wireDict["WR3"] = VerletWire.WireColor.Blue;
        wireDict["WR4"] = VerletWire.WireColor.Red;
        wireDict["grey"] = VerletWire.WireColor.Grey;
        wireDict["brown"] = VerletWire.WireColor.Brown;
        wireDict["blue"] = VerletWire.WireColor.Blue;
        wireDict["red"] = VerletWire.WireColor.Red;
    }

    private void Start()
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

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null.");
            return;
        }

        if (VerletWire.CutWireColor.HasValue)
        {
            cutColor = VerletWire.CutWireColor.Value.ToString().ToLower();
        }
        

        Debug.Log("Cut wire: " + cutColor);
        Debug.Log("Expected wire: " + wireMapping.cutWire);
        if (VerletWire.CutWireColor.HasValue)
        {
            softwareWireColor = VerletWire.CutWireColor.Value;
        }
        

        if (softwareWireColor != VerletWire.WireColor.None) // software wire is cut first
        {
            // CheckWireCutColor(softwareWireColor, hardwareWireColor);
            HasCutSoftwareWire?.Invoke();
            HasCutAnyWire?.Invoke();
            // //if software wire cut is wrong in the first place
            if (softwareWireColor != wireDict[wireMapping.cutWire])
            {
                //starts the timer
                timeToCut -= Time.deltaTime;
                if (timeToCut <= 0f || hardwareWireColor != VerletWire.WireColor.None)
                {//game lost if timer hits 0 or any physical color is cut
                    GameManager.Instance.EndGame();
                }
            }
            else//software wire cut is correct wait for physical wire color
            {
                //starts the timer
                timeToCut -= Time.deltaTime;
                if (timeToCut <= 0f)
                {
                    //game lost if timer hits 0
                    GameManager.Instance.EndGame();
                }
                else
                {
                    //if it's the same we are good
                    if (hardwareWireColor == softwareWireColor)
                    {
                        GameManager.Instance.WinGame();
                    }
                }
            }
        }else if (hardwareWireColor != VerletWire.WireColor.None) // hardware wire is cut first
        {
            // CheckWireCutColor(hardwareWireColor, softwareWireColor);
            HasCutAnyWire?.Invoke();
            if (hardwareWireColor != wireDict[wireMapping.cutWire])
            {
                //starts the timer
                timeToCut -= Time.deltaTime;
                if (timeToCut <= 0f || softwareWireColor != VerletWire.WireColor.None)
                {//game lost if timer hits 0 or any software color is cut
                    GameManager.Instance.EndGame();
                }
            }
            else//hardware wire cut is correct wait for software wire color
            {
                //starts the timer
                timeToCut -= Time.deltaTime;
                if (timeToCut <= 0f)
                {
                    //game lost if timer hits 0
                    GameManager.Instance.EndGame();
                }
                else
                {
                    //if it's the same we are good
                    if (softwareWireColor == hardwareWireColor)
                    {
                        GameManager.Instance.WinGame();
                    }
                }
            }
        }
        VerletWire.CutWireColor = null;
    }

    private void CheckWireCutColor(VerletWire.WireColor wireCutFirst, VerletWire.WireColor wireCutLater)
    {
        if (wireCutFirst != wireDict[wireMapping.cutWire])
        {
            //starts the timer
            timeToCut -= Time.deltaTime;
            if (timeToCut <= 0f || wireCutLater != VerletWire.WireColor.None)
            {//game lost if timer hits 0 or any physical color is cut
                GameManager.Instance.EndGame();
            }
        }
        else//software wire cut is correct wait for physical wire color
        {
            //starts the timer
            timeToCut -= Time.deltaTime;
            if (timeToCut <= 0f)
            {
                //game lost if timer hits 0
                GameManager.Instance.EndGame();
            }
            else
            {
                //if it's the same we are good
                if (wireCutFirst == wireCutLater)
                {
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
            if (hardwareWireColor == VerletWire.CutWireColor)
            {
                GameManager.Instance.WinGame();
            }
        }
    }

    private void HandleHardwareWire(string header, int value)
    {
        if(value == 1) return;
        hardwareWireColor = wireDict[header];
    }
}