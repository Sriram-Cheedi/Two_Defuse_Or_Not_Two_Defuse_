using System;
using System.Collections;
using System.Collections.Generic;
using Puzzles;
using Puzzles.Controllers;
using UnityEngine;
using UnityEngine.Serialization;

// Global Room State Manager
public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    
    public Puzzles.Controllers.SSM_Controller ssm_controller;
    public Puzzles.Controllers.NGM_Controller ngm_controller;
    public Puzzles.Controllers.MazeController maze_controller;
    [SerializeField] private Maze_Trigger maze_Trigger;
    
    public Action SSM_Completed;
    public Action NGM_Completed;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => 
                API_Provider.Instance != null && 
                API_Provider.Instance.ActiveAPIController != null && 
                API_Provider.Instance.hasAPIController
        );
        
        // Send all Puzzle info AFTER 
        API_Provider.Instance.ActiveAPIController.SendSet("MAZE", 0);
        // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[]{"\"Module Activated: \"", "\"SSM\""});
        API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"Module Activated\"", "\"------SSM------\"" });
        API_Provider.Instance.ActiveAPIController.SendSetSeq("SSM", ssm_controller.data.correctSequence);
        API_Provider.Instance.ActiveAPIController.SendSetSeq("NGM", ngm_controller.data.correctSequence);
        API_Provider.Instance.ActiveAPIController.SendSet("SSM", 1);
        // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[]{"module activated SSM"});
        // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[]{"\"Module Activated: \"", "\"\""});

        API_Provider.Instance.ActiveAPIController.SendSet("FGM", 0);
        // API_Provider.Instance.ActiveAPIController.SendSet("MAZE", 0);
        // API_Provider.Instance.ActiveAPIController.SendSet("MAZE", 1);
        
        // API_Provider.Instance.ActiveAPIController.SendMazeSeq("LCD", "\"module activated SSM\"");

        
        // This is subscriptions for the software side of each puzzle. Triggers off hardware will be handled through the API not Actions such as this
        
        
        Debug.Log("Subscriptions Successful!");

    }

    private void OnEnable()
    {
        Puzzle.OnSuccess += S_HandleSSMVictory;
        Puzzle.OnSuccess += HandleNGMVictory;
        API_Controller.OnInputChanged += H_HandleSSMVictory;
        API_Controller.OnHardwareReconnected += H_Resync;

    }

    private void OnDisable()
    {
        Puzzle.OnSuccess -= S_HandleSSMVictory;
        Puzzle.OnSuccess -= HandleNGMVictory;
        API_Controller.OnInputChanged -= H_HandleSSMVictory;
        API_Controller.OnHardwareReconnected -= H_Resync;

    }

    // Global Changes if correct
    private void S_HandleSSMVictory(PuzzleGame puzzleGame)
    {
        if (puzzleGame != PuzzleGame.SSM) return;
        
        Debug.Log("Software SSM Solved!");
        // Link Lighting and Alarms and stuff to this 
        SSM_Completed?.Invoke(); 
        API_Provider.Instance.ActiveAPIController.SendSet("SSM", 2); // Alert Hardware that puzzle has been completed (Edge case where software guesses before Hardware complete)
        
        // Activate NGM Puzzle after SSM Completed
        API_Provider.Instance.ActiveAPIController.SendSet("NGM", 1);
        API_Provider.Instance.ActiveAPIController.SendSet("PWR", 2);
    }
    
    private void H_HandleSSMVictory(string header, int value)
    {
        if (header == "SSM" && value == 2)
        {
            Debug.Log("Hardware SSM Solved!");

            // Activate NGM Puzzle after SSM Completed - Not invoking SSM_Completed as this is software event when correct seq is inputted
            // API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"Module Activated\"", "\"--Number Game--\"" });
            API_Provider.Instance.ActiveAPIController.SendSet("NGM", 1);
        }
    }
    
    
    // Global Changes if correct
    private void HandleNGMVictory(PuzzleGame puzzleGame)
    {
        if (puzzleGame != PuzzleGame.NGM) return;
        
        Debug.Log("NGM Solved!");
        // Link Lighting and Alarms and stuff to this 
        NGM_Completed?.Invoke(); 
    }

    public void H_Resync()
    {
        if (API_Provider.Instance.ActiveAPIController == null)
        {
            return;
        }
        Debug.Log("Resync Activated");

        if (ssm_controller != null && ssm_controller.data != null)
        {
            API_Provider.Instance.ActiveAPIController.SendSetSeq("SSM", ssm_controller.data.correctSequence);
        }

        if (ngm_controller != null && ngm_controller.data != null)
        {
            API_Provider.Instance.ActiveAPIController.SendSetSeq("NGM", ngm_controller.data.correctSequence);
        }

        bool H_SSM_Puzzle_Finished = API_Provider.Instance.ActiveAPIController.GetSSMStatus() == 2;
        bool S_SSM_Puzzle_Finished = ssm_controller != null && ssm_controller.data.completed;
        bool NGM_Puzzle_Finished = ngm_controller != null && ngm_controller.data.completed;

        if (!S_SSM_Puzzle_Finished)
        {
            if (!H_SSM_Puzzle_Finished)
            {
                API_Provider.Instance.ActiveAPIController.SendSetSeqSTRING("LCD", new string[] { "\"Module Activated\"", "\"------SSM------\"" } );
                API_Provider.Instance.ActiveAPIController.SendSet("SSM", 1);
            }
            else
            {
                API_Provider.Instance.ActiveAPIController.SendSet("SSM", 2);
            }
            API_Provider.Instance.ActiveAPIController.SendSet("FGM", 0);
            API_Provider.Instance.ActiveAPIController.SendSet("NGM", 0);
        }
        else
        {
            API_Provider.Instance.ActiveAPIController.SendSet("SSM", 2);
            if (NGM_Puzzle_Finished)
            {
                API_Provider.Instance.ActiveAPIController.SendSet("NGM", 2);
            }
            else
            {
                API_Provider.Instance.ActiveAPIController.SendSet("NGM", 1);
            }
        }
        
        if (API_Provider.Instance.ActiveAPIController.GetMazeStatus() == 1 && maze_controller != null && maze_controller.data != null)
        {
            // maze_Trigger.SendMazeONHardwareResync();
            StartCoroutine(DelayMazeResync());
        }
        

        int FGM_Status = API_Provider.Instance.ActiveAPIController.GetDigital("FGM");
        if (FGM_Status != 0)
        {
            API_Provider.Instance.ActiveAPIController.SendSet("FGM", 1);
        }
        
        int PWR_Status = API_Provider.Instance.ActiveAPIController.GetDigital("PWR");
        if (PWR_Status != 0)
        {
            API_Provider.Instance.ActiveAPIController.SendSet("PWR", PWR_Status);
        }
    }
    private IEnumerator DelayMazeResync()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        if (maze_Trigger == null)
        {
            yield break;
        }
        maze_Trigger.SendMazeONHardwareResync();
    }
    
}