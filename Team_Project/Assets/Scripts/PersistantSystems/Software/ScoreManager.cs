using System;
using System.Collections;
using System.Collections.Generic;
using PersistantSystems.Software.GameStateMachine;
using Puzzles;
using Puzzles.Controllers;
using UnityEngine;

namespace PersistantSystems.Software
{
    public readonly struct PuzzleConfig
    {
        public readonly float Weight;
        public readonly float OptimalTime;
        
        
        public PuzzleConfig(float weight, float optimalTime)
        {
            Weight = weight;
            OptimalTime = optimalTime;
        }
    }

    public class PuzzleResults
    {
        public int Attempts;
        public bool Solved;

        public PuzzleResults(int attempts, bool solved)
        {
            Attempts = attempts;
            Solved = solved;
        }
    }
    public static class FinalRunResults
    {
        public static int yourScore;
        public static double bombTimeRemaining;
    }
    
    public class ScoreManager : MonoBehaviour
    {
        // CONSTANTS
        private const int BASE_SCORE = 1000;
        private const int TIME_BONUS_WEIGHT = 10;
        private const int DEATH_PENALTY_BASE = 50;
        private const int PERFECT_RUN_BONUS = 2000;
        private const int SYNC_MAX_BONUS = 500;
        private const float SYNC_WINDOW = 3.0f;
        private const float SYNC_PERFECT_THRESHOLD = 0.5f;
        private const float LOSS_MULTIPLIER = 0.25f;

        private Dictionary<PuzzleGame, PuzzleResults> puzzleResultsMap = 
                new Dictionary<PuzzleGame, PuzzleResults>()
                {
                    { PuzzleGame.SWT, new PuzzleResults(0, false) },
                    { PuzzleGame.SSM, new PuzzleResults(0, false) },
                    { PuzzleGame.NGM, new PuzzleResults(0, false) },
                    { PuzzleGame.WCM, new PuzzleResults(0, false) },
                };
        
        public static readonly IReadOnlyDictionary<PuzzleGame, PuzzleConfig> Puzzles =
            new Dictionary<PuzzleGame, PuzzleConfig>
            {
                { PuzzleGame.SWT, new PuzzleConfig(1.6f, 60f) },
                { PuzzleGame.SSM, new PuzzleConfig(1.3f, 45f) },
                { PuzzleGame.NGM, new PuzzleConfig(1.0f, 40f) },
                { PuzzleGame.WCM, new PuzzleConfig(0.8f, 10f) },
            };

        private float wireCutDelta;
        private int deathCount = 0;
        public string username = "Unknown";
        private DatabaseAPI dbAPI;
        
        private int puzzleScores = 0;
        
        private void Awake()
        {
            dbAPI = FindFirstObjectByType<DatabaseAPI>();
            if (dbAPI == null) Debug.LogError("Database Manager not found");
            ResetPuzzleResults(); 
            deathCount = 0;
        }
        
        public void OnUsernameEntered(string newUsername)
        {
            if (string.IsNullOrEmpty(newUsername)) return;
            username = newUsername;
            deathCount = 0;
            puzzleScores = 0;
            ResetPuzzleResults();
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
            WireCut.HasCutWireWithinTimeWindow += UpdateCutTime;
        }

        private void OnDisable()
        {
            SpawnManager.RespawnRequested -= AddDeath;
            GameManager.OnGameEnd -= CalculateFinalScore;
            Puzzle.OnSuccess -= CompletePuzzle;
            Puzzle.OnFailure -= FailPuzzle;
            WireCut.HasCutWireWithinTimeWindow -= UpdateCutTime;
        }

        private void AddDeath(RespawnData data)
        {
            deathCount++;
        }
        
        private float GetAttemptMultiplier(PuzzleGame puzzleGame, int attempts)
        {
            if (puzzleGame == PuzzleGame.SWT)
            {
                return 1.0f;
            }

            if (puzzleGame == PuzzleGame.WCM)
            {
                return 1.0f;
            }

            if (puzzleGame == PuzzleGame.NGM)
            {
                if (attempts <= 6)
                {
                    return 1.0f;
                }else if (attempts <= 8)
                {
                    return 0.8f;
                }
                else
                {
                    return 0.5f;
                }
            }

            if (puzzleGame == PuzzleGame.SSM)
            {
                if (attempts == 1) return 1.0f;
                if (attempts == 2) return 0.6f;
                return 0.3f;
            }

            return 1.0f;
        }

        private float SyncBonus(float delta, bool defused)
        {
            if (!defused) return 0f;
            if (delta <= SYNC_PERFECT_THRESHOLD) return SYNC_MAX_BONUS;
            if (delta >= SYNC_WINDOW) return 0f;
            return SYNC_MAX_BONUS * (SYNC_WINDOW - delta) / (SYNC_WINDOW - SYNC_PERFECT_THRESHOLD);
        }

        private void FailPuzzle(PuzzleGame puzzleGame)
        {
            // int subtractVal = 0;
            // if (puzzle == PuzzleGame.SSM) subtractVal = 20;
            // else if (puzzle == PuzzleGame.NGM) subtractVal = 10;
            //
            // Debug.Log($"Subtracting {subtractVal}");
            // puzzleScores -= subtractVal;
            if (puzzleResultsMap[puzzleGame].Solved) return;
            puzzleResultsMap[puzzleGame].Attempts++;
        }
        
        private void CompletePuzzle(PuzzleGame puzzleGame)
        {
            puzzleResultsMap[puzzleGame].Attempts++;  
            puzzleResultsMap[puzzleGame].Solved = true;
            // puzzleScores += (int)ScorePuzzle(puzzleGame, puzzleResultsMap[puzzleGame].Attempts);
            // Debug.Log("Adding Score");
            // puzzleScores += 200;
        }

        private float ScorePuzzle(PuzzleGame puzzleGame, int attempts)
        {
            if (!puzzleResultsMap[puzzleGame].Solved) return 0;
            float multiplier = GetAttemptMultiplier(puzzleGame, attempts);
            return BASE_SCORE * Puzzles[puzzleGame].Weight * multiplier;
        }

        private void UpdateCutTime(float cutTimeWindow)
        {
            wireCutDelta = cutTimeWindow;
        }

        private bool IsPerfectRun(double bombTime)
        {
            if (!puzzleResultsMap[PuzzleGame.WCM].Solved) return false;
            if (deathCount != 0) return false;
            if (wireCutDelta > 1.0f) return false;
            if (bombTime < 90.0f) return false;
            if (puzzleResultsMap[PuzzleGame.SSM].Attempts > 1) return false;
            if (puzzleResultsMap[PuzzleGame.NGM].Attempts > 7) return false;
            return true;
        }
        
        public void ResetPlayerSession()
        {
            username = "Unknown";
            deathCount = 0;
            puzzleScores = 0;
            ResetPuzzleResults();

            if (dbAPI != null)
            {
                dbAPI.UpdateStationStatus(GameplayStationConfig.StationId, "---", "Waiting");
            }
        }
        
        private void ResetPuzzleResults()
        {
            foreach (var key in new List<PuzzleGame>(puzzleResultsMap.Keys))
            {
                puzzleResultsMap[key] = new PuzzleResults(0, false);
            }
            wireCutDelta = 0f;
        }

        private void CalculateFinalScore(double bombTime)
        {
            // var finalScore = puzzleScores + (int)bombTime + deathCount * -10;
            // if (finalScore < 0) finalScore = 0;
            var finalScore = 0f;
            bool defused = puzzleResultsMap[PuzzleGame.WCM].Solved;
            foreach (PuzzleGame game in Enum.GetValues(typeof(PuzzleGame)))
            {
                finalScore += ScorePuzzle(game, puzzleResultsMap[game].Attempts);
            }

            finalScore += (float)bombTime * TIME_BONUS_WEIGHT;
            finalScore -= deathCount * DEATH_PENALTY_BASE;
            finalScore += SyncBonus(wireCutDelta, defused);

            if (!defused)
            {
                finalScore *= LOSS_MULTIPLIER;
            }
            
            if (IsPerfectRun(bombTime))
            {
                finalScore += PERFECT_RUN_BONUS;
            }

            int finalScoreInt = Mathf.Max(0, Mathf.RoundToInt(finalScore));
            
            // bool defused = GameManager.Instance != null &&
            //                GameManager.Instance.CurrentStateKey == GameState.GameWin;
            // yourScore = finalScoreInt;
            // bombTimeRemaining = bombTime;
            FinalRunResults.yourScore = finalScoreInt;
            FinalRunResults.bombTimeRemaining = bombTime;

            Debug.Log($"Final Score {finalScoreInt}");

            if (dbAPI != null)
            {
                dbAPI.SubmitScore(username, finalScoreInt);
                dbAPI.SubmitLeaderboardEntry(username, finalScoreInt, defused, GameplayStationConfig.StationId);
                dbAPI.UpdateStationStatus(GameplayStationConfig.StationId, username, defused ? "Defused" : "Exploded");
            }
        }
    }
}