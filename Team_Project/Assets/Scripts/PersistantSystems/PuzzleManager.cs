using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleEvents
{
    public Action OnSuccess;
    public Action OnFailure;
}

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    
    int[] SSMSeq = new int[6];
    
    public Dictionary<string, PuzzleEvents> _puzzleTriggers;
    // public Dictionary<string, int> _puzzleStates;
    
    
    private int currentSequenceIndex = 0;
    
    private void Awake()
    {
        Debug.Log("Puzzle Manager Initialised.");
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        _puzzleTriggers = new Dictionary<string, PuzzleEvents>();
        _puzzleTriggers["SSM"] = new PuzzleEvents();
        _puzzleTriggers["NGM"] = new PuzzleEvents();
        
        GenerateSSMSequence();
    }
    
    // =============  SSM FUNCTIONS ===================
    
    public void CheckSSMButtonPress(int pressedValue)
    {
        if (pressedValue == SSMSeq[currentSequenceIndex])
        {
            Debug.Log("Correct!" + pressedValue);
            currentSequenceIndex++;

            if (currentSequenceIndex >= SSMSeq.Length)
            {
                Debug.Log("SSM Complate" + pressedValue);
                PuzzleSolved("SSM");
            }
        }
        else
        {
            Debug.Log("Wrong button! Resetting sequence.");
            _puzzleTriggers["SSM"].OnFailure?.Invoke();

            ResetPuzzle("SSM");
        }
    }
    
    public int[] GenerateSSMSequence()
    {
        SSMSeq = new int[] { 0, 1 };
        Debug.Log("Inside Generate Sequence");
        return SSMSeq;
    }
    
    private void ResetSSMSequence()
    {
        currentSequenceIndex = 0;
    }
    
    // ============ General Functions ================
    // When we implement multiple SSM Doors we will need to reset SSM Puzzle state and generate new sequence
    private void PuzzleSolved(string name)
    {
        _puzzleTriggers[name].OnSuccess?.Invoke();
    }
    
    private void ResetPuzzle(string name)
    {
        if (name == "SSM") currentSequenceIndex = 0;
    }
}