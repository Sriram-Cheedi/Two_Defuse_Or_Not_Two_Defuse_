using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class PuzzleData
{
    public bool completed = false;
    
    public int[] guessSequence;
    public int[] correctSequence;
    
    public int currentSequenceIndex = 0;
    
    public Action OnSuccess;
    public Action OnFailure;
}

// Global Room State Manager
public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    
    public Puzzles.Controllers.SSM_Controller ssm_controller;
    public Puzzles.Controllers.NGM_Controller ngm_controller;
    public Puzzles.Controllers.MazeController maze_controller;
    
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
        API_Provider.Instance.ActiveAPIController.SendSetSeq("SSM", ssm_controller.data.correctSequence);
        API_Provider.Instance.ActiveAPIController.SendSetSeq("NGM", ngm_controller.data.correctSequence);
        API_Provider.Instance.ActiveAPIController.SendSet("SSM", 1);
        API_Provider.Instance.ActiveAPIController.SendSet("FGM", 1);
        
        // This is subscriptions for the software side of each puzzle. Triggers off hardware will be handled through the API not Actions such as this
        ssm_controller.data.OnSuccess += S_HandleSSMVictory;
        ngm_controller.data.OnSuccess += S_HandleSSMVictory;

        API_Provider.Instance.ActiveAPIController.OnInputChanged += H_HandleSSMVictory;
        
        Debug.Log("Subscriptions Successful!");

    }
    
    // Global Changes if correct
    private void S_HandleSSMVictory()
    {
        Debug.Log("Software SSM Solved!");
        // Link Lighting and Alarms and stuff to this 
        SSM_Completed?.Invoke(); 
        API_Provider.Instance.ActiveAPIController.SendSet("SSM", 2); // Alert Hardware that puzzle has been completed (Edge case where software guesses before Hardware complete)
        
        
        // Activate NGM Puzzle after SSM Completed
        API_Provider.Instance.ActiveAPIController.SendSet("NGM", 1);
    }
    
    private void H_HandleSSMVictory(string header, int value)
    {
        if (header == "SSM" && value == 2)
        {
            Debug.Log("Hardware SSM Solved!");

            // Activate NGM Puzzle after SSM Completed - Not invoking SSM_Completed as this is software event when correct seq is inputted
            API_Provider.Instance.ActiveAPIController.SendSet("NGM", 1);
        }
    }
    
    
    // Global Changes if correct
    private void HandleNGMVictory()
    {
        Debug.Log("NGM Solved!");
        // Link Lighting and Alarms and stuff to this 
        NGM_Completed?.Invoke(); 
    }
}