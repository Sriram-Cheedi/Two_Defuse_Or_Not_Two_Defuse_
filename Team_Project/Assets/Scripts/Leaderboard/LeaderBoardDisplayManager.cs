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

    private readonly List<LeaderboardRowUI> spawnedRows = new();
    private DatabaseAPI dbAPI;

    private void Start()
    {
        BuildRows();

        dbAPI = FindFirstObjectByType<DatabaseAPI>();

        if (dbAPI == null)
        {
            Debug.LogError("LeaderboardDisplayManager could not find DatabaseAPI.");
            return;
        }

        dbAPI.OnFirebaseInitialized += HandleFirebaseInitialized;
        dbAPI.OnTopLeaderboardUpdated += HandleLeaderboardUpdated;
        dbAPI.OnStationStatusUpdated += HandleStationStatusUpdated;

        SetEmptyBoard();
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

    private void BuildRows()
    {
        if (rowPrefab == null || rowsContainer == null)
        {
            Debug.LogError("LeaderboardDisplayManager is missing rowPrefab or rowsContainer.");
            return;
        }

        for (int i = 0; i < maxRows; i++)
        {
            LeaderboardRowUI row = Instantiate(rowPrefab, rowsContainer);
            spawnedRows.Add(row);
        }
    }

    private void HandleFirebaseInitialized()
    {
        dbAPI.SubscribeToTopScores(maxRows);
        dbAPI.SubscribeToStationStatuses();
    }

    private void HandleLeaderboardUpdated(List<LeaderboardEntryData> entries)
    {
        SetLeaderboard(entries);
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

    private void SetEmptyBoard()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            spawnedRows[i].SetEmpty(i + 1);
        }

        if (station1Panel != null)
            station1Panel.SetStatus("---", "Waiting");

        if (station2Panel != null)
            station2Panel.SetStatus("---", "Waiting");
    }

    public void SetLeaderboard(List<LeaderboardEntryData> entries)
    {
        if (entries == null)
            entries = new List<LeaderboardEntryData>();

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
}