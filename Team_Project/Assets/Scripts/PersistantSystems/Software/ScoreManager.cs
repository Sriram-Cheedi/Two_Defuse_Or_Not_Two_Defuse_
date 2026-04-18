using System;
using PersistantSystems.Software.GameStateMachine;
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
            deathCount = 0;
            puzzleScores = 0;

            Debug.Log("Username entered: " + username);
            GameManager.Instance?.SetCurrentUsername(username);
            Debug.Log($"OnUsernameEntered station={GameplayStationConfig.StationId} user={username}");

            if (dbAPI != null)
            {
                dbAPI.UpdateStationStatus(GameplayStationConfig.StationId, username, "Waiting");
            }
        }
        
        
        
        private void OnEnable()
        {
            SpawnManager.RespawnRequested += AddDeath;
            GameManager.OnGameEnd += CalculateFinalScore;
            Puzzle.OnSuccess += CompletePuzzle;
            Puzzle.OnFailure += FailPuzzle;

        }

        private void OnDisable()
        {
            SpawnManager.RespawnRequested -= AddDeath;
            GameManager.OnGameEnd -= CalculateFinalScore;
            Puzzle.OnSuccess -= CompletePuzzle;
            Puzzle.OnFailure -= FailPuzzle;
        }

        private void AddDeath(RespawnData data)
        {
            deathCount++;
        }

        private void FailPuzzle(PuzzleGame puzzle)
        {
            int subtractVal = 0;
            if (puzzle == PuzzleGame.SSM) subtractVal = 20;
            else if (puzzle == PuzzleGame.NGM) subtractVal = 10;
            
            Debug.Log($"Subtracting {subtractVal}");
            puzzleScores -= subtractVal;
        }
        
        private void CompletePuzzle(PuzzleGame puzzle)
        {
            Debug.Log("Adding Score");
            puzzleScores += 200;
        }
        
        public void ResetPlayerSession()
        {
            username = "Unknown";
            deathCount = 0;
            puzzleScores = 0;

            if (dbAPI != null)
            {
                dbAPI.UpdateStationStatus(GameplayStationConfig.StationId, "---", "Waiting");
            }
        }

        private void CalculateFinalScore(double bombTime)
        {
            var finalScore = puzzleScores + (int)bombTime + deathCount * -10;
            if (finalScore < 0) finalScore = 0;
            
            bool defused = GameManager.Instance != null &&
                           GameManager.Instance.CurrentStateKey == GameState.GameWin;

            Debug.Log($"Final Score {finalScore}");

            if (dbAPI != null)
            {
                dbAPI.SubmitScore(username, finalScore);

                dbAPI.SubmitLeaderboardEntry(username, finalScore, defused, GameplayStationConfig.StationId);

                dbAPI.UpdateStationStatus(GameplayStationConfig.StationId, username, defused ? "Defused" : "Exploded");
            }
        }
    }
}