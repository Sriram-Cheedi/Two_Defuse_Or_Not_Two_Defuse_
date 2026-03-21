using System.Collections.Generic;
using UnityEngine;

namespace PersistantSystems.Software
{
    public class LeaderboardManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] scoreTiles;
        private DatabaseAPI dbAPI;
        
        private List<(string, int)> topScores = new();

        private void Awake()
        {
            dbAPI = FindFirstObjectByType<DatabaseAPI>();
            if (dbAPI == null) Debug.LogError("Database Manager not found");
        }

        private void OnEnable()
        {
            dbAPI.OnFirebaseInitialized += SpawnTopScores;
        }

        private void OnDisable()
        {
            dbAPI.OnFirebaseInitialized -= SpawnTopScores;
        }

        private async void SpawnTopScores()
        {
            try
            {
                topScores = await dbAPI.GetScoresAsync();
            
                int i;
                for (i = 0; i < scoreTiles.Length; i++)
                {
                    if (i >= topScores.Count)
                    {
                        Debug.Log($"Despawning ScoreTile {i}");
                        scoreTiles[i].SetActive(false);
                    }
                    else
                    {
                        Debug.Log($"Setting ScoreTile {i}");
                        scoreTiles[i].GetComponent<ScoreTile>().SetData(topScores[i].Item1, topScores[i].Item2);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to getScores for Tile Spawns: " +  e.Message);

            }
        }
    }   
}