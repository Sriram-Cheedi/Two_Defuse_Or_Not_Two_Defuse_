using System;
using System.Collections;
using System.Collections.Generic;
using Puzzles.Controllers;
using UnityEngine;

public class Maze_Trigger : MonoBehaviour
{
    [SerializeField] Puzzles.Controllers.MazeController maze_controller;
    private Dictionary<int, string> directionMap;
    
    private bool gameActive = false;
    
    private string[] lines;
    private int lineCount = 0;

    private string sequence;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         gameActive = true;
    //         StartCoroutine(startScreen());
    //     }
    // }

    private void Awake()
    {
        directionMap = new Dictionary<int, string>();
        lines = new[]{"","","",""};
        
        directionMap.Add(0, "U");
        directionMap.Add(1, "D");
        directionMap.Add(2, "L");
        directionMap.Add(3, "R");
    }

    private void Start()
    {
        for (int i = 0; i < maze_controller.data.correctSequence.Length; i++)
        {
            sequence += directionMap[maze_controller.data.correctSequence[i]];
            if (i !=  maze_controller.data.correctSequence.Length - 1) sequence += " ";
        }

        Debug.Log("Maze Sequence: " + sequence);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            API_Provider.Instance.ActiveAPIController.SendMazeSeq("MAZE", sequence);
            
            SendSet();
        }
    }
    public void SendMazeONHardwareResync()
    {
        if (API_Provider.Instance == null || API_Provider.Instance.ActiveAPIController == null)
        {
            return;
        }

        API_Provider.Instance.ActiveAPIController.SendMazeSeq("MAZE", sequence);
        SendSet();
    }
    private void SendSet()
    {
        
        API_Provider.Instance.ActiveAPIController.SetMazeStatus("MAZE", 1);
        API_Provider.Instance.ActiveAPIController.SendSet("MAZE", 1);

    }
    
    IEnumerator StartScreen()
    {
        yield return new WaitForSeconds(1.0f);
        while (gameActive)
        {
            // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"--MAZE-SEQ--\"", "\"------------\"" });
            // yield return new WaitForSeconds(3f);
            // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { $"\"U-U-U-L-L-U\"", $"\"U-R-R-R-D-R\"" });
            // yield return new WaitForSeconds(5f);
            // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { $"\"R-U-U-U-L-L\"", $"\"L\"" });
            // yield return new WaitForSeconds(5f);
            // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"--MAZE-SEQ--\"", "\"------------\"" });
            yield return new WaitForSeconds(3f);
            //
            // for (int i = 0; i < lines.Length; i++)
            // {
                API_Provider.Instance.ActiveAPIController.SendMazeSeq("MAZE", "U U U U U U U U U");
                // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { lines[i], lines[i + 1] });
                yield return new WaitForSeconds(5f);
            // }
        }
    }
    //
    public string GetDirectionString(int dirInt)
    {
        return dirInt switch
        {
            0 => "UP",
            1 => "DOWN",
            2 => "LEFT",
            3 => "RIGHT",
            _ => "WTF IS THIS"
        };
    }
}