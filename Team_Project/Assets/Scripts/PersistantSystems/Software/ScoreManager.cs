using System;
using Puzzles;
using UnityEngine;

namespace PersistantSystems.Software
{
    public class ScoreManager : MonoBehaviour
    {
        private int deathCount = 0;
        public string username;
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
            Debug.Log("Subtracting Score");

            puzzleScores -= 20;
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