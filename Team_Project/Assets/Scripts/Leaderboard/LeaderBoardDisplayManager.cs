using System.Collections.Generic;
using PersistantSystems.Software;
using PersistantSystems.Software.FirebaseModels;
using UnityEngine;

public class LeaderboardDisplayManager : MonoBehaviour
{
    [Header("Row Setup")]
    [SerializeField] private Transform rowsContainer;
    [SerializeField] private LeaderboardRowUI rowPrefab;
    [SerializeField] private int maxRows = 10;
    [Header("Station Panels")]
    [SerializeField] private StationStatusPanelUI station1Panel;
    [SerializeField] private StationStatusPanelUI station2Panel;
    private List<LeaderboardRowUI> spawnedRows = new();
    private DatabaseAPI dbAPI;

    private void Start()
    {
        if (spawnedRows.Count > 0)
        {
            return;
        }
        for (int i = 0; i < maxRows; i++)
        {
            LeaderboardRowUI row = Instantiate(rowPrefab, rowsContainer);
            spawnedRows.Add(row);
        }
        
        dbAPI = FindFirstObjectByType<DatabaseAPI>();
        dbAPI.OnFirebaseInitialized += HandleFirebaseInitialized;
        dbAPI.OnTopLeaderboardUpdated += HandleLeaderboardUpdated;
        dbAPI.OnStationStatusUpdated += HandleStationStatusUpdated;
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            spawnedRows[i].SetEmpty(i + 1);
        }
        station1Panel.SetStatus("---", "Waiting");
        station2Panel.SetStatus("---", "Waiting");
        
        if (dbAPI.IsInitialized)
        {
            HandleFirebaseInitialized();
        }
    }

    private void OnDestroy()
    {
        if (dbAPI != null)
        {
            dbAPI.OnFirebaseInitialized -= HandleFirebaseInitialized;
            dbAPI.OnTopLeaderboardUpdated -= HandleLeaderboardUpdated;
            dbAPI.OnStationStatusUpdated -= HandleStationStatusUpdated;
        }
    }
    

    private void HandleFirebaseInitialized()
    {
        dbAPI.SubscribeToTopScores(maxRows);
        dbAPI.SubscribeToStationStatuses();
    }

    private void HandleLeaderboardUpdated(List<LeaderboardEntryData> entries)
    {
        if (entries == null)
        {
            entries = new List<LeaderboardEntryData>();
        }
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (i < entries.Count)
            {
                spawnedRows[i].SetRow(i + 1, entries[i].username, entries[i].score, entries[i].defused);
            }
            else
            {
                spawnedRows[i].SetEmpty(i + 1);
            }
        }
    }

    private void HandleStationStatusUpdated(string stationId, StationStatusData data)
    {
        if (stationId == "station1" && station1Panel != null)
        {
            station1Panel.SetStatus(data.teamName, data.status);
        }
        else if (stationId == "station2" && station2Panel != null)
        {
            station2Panel.SetStatus(data.teamName, data.status);
        }
    }
}