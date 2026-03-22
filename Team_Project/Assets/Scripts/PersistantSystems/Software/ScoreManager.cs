using System;
using Puzzles;
using Puzzles.Controllers;
using UnityEngine;

namespace PersistantSystems.Software
{
    public class ScoreManager : MonoBehaviour
    {
        private int deathCount = 0;
        public string username = "Unknown";
        private DatabaseAPI dbAPI;
        
        private int puzzleScores = 0;
        
        private void Awake()
        {
            dbAPI = FindFirstObjectByType<DatabaseAPI>();
            if (dbAPI == null) Debug.LogError("Database Manager not found");
            
            deathCount = 0;
        }
        
        public void OnUsernameEntered(string newUsername)
        {
            if (string.IsNullOrEmpty(newUsername)) return;
            username = newUsername;
            Debug.Log("Username entered: " + username);
        }

        private void OnEnable()
        {
            SpawnManager.RespawnRequested += AddDeath;
            GameManager.onGameEnd += CalculateFinalScore;
            Puzzle.OnSuccess += CompletePuzzle;
            Puzzle.OnFailure += FailPuzzle;

        }

        private void OnDisable()
        {
            SpawnManager.RespawnRequested -= AddDeath;
            GameManager.onGameEnd -= CalculateFinalScore;
            Puzzle.OnSuccess -= CompletePuzzle;
            Puzzle.OnFailure -= FailPuzzle;
        }

        private void AddDeath(RespawnData data)
        {
            deathCount++;
        }

        private void FailPuzzle(GameObject puzzle)
        {
            int subtractVal = 0;
            if (puzzle.GetComponent<SSM_Controller>() != null) subtractVal = 20;
            else if (puzzle.GetComponent<NGM_Controller>() != null) subtractVal = 10;
            
            Debug.Log($"Subtracting {subtractVal}");
            puzzleScores -= subtractVal;
        }
        
        private void CompletePuzzle(GameObject puzzle)
        {
            Debug.Log("Adding Score");
            puzzleScores += 100;
        }
        

        private void CalculateFinalScore(double bombTime)
        {
            var finalScore = puzzleScores + (int)bombTime + deathCount * -50;
            if (finalScore < 0) finalScore = 0;
            
            Debug.Log($"Final Score {finalScore}");
            dbAPI.SubmitScore(username, finalScore);
        }
    }
}