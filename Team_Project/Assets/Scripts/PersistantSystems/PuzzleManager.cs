using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    
    int[] SSMSeq = new int[6];
    public Dictionary<string, int> _puzzleStates;
    
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
        
        _puzzleStates = new Dictionary<string, int>();
        
        _puzzleStates["SSM"] = 0;
        _puzzleStates["NGM"] = 0;
    }
    
    public int[] GenerateSSMSequence()
    {
        SSMSeq = new int[] { 0, 1, 2, 0, 2, 3 };
        Debug.Log("Inside Generate Sequence");
        return SSMSeq;
    }

    public void Update()
    {
        if (_puzzleStates["SSM"] != 2)
        {
            API_Provider.Instance.ActiveAPIController.RequestStatus("$SSM:status#");
            _puzzleStates["SSM"] = API_Provider.Instance.ActiveAPIController.GetSSMStatus();
        }
    }

    public int checkPuzzleStatus(string puzzle)
    {
        return _puzzleStates[puzzle];
    }
}