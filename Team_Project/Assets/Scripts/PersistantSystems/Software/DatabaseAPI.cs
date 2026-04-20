using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using PersistantSystems.Software.FirebaseModels;
using UnityEngine;

namespace PersistantSystems.Software
{
    [Serializable]
    public class User {
    public string username = string.Empty;
    public int score = 0;

    public User() {}
    public User(string username, int score) {
        this.username = username;
        this.score = score;
    }
}

public class DatabaseAPI: MonoBehaviour
{
    
    public static DatabaseAPI Instance { get; private set; }
    public bool IsInitialized => reference != null;
    
    public event Action OnFirebaseInitialized; 
    public event Action<List<LeaderboardEntryData>> OnTopLeaderboardUpdated;
    public event Action<string, StationStatusData> OnStationStatusUpdated;
    private const string DatabaseURL = "https://teamproject-c1ec1-default-rtdb.europe-west1.firebasedatabase.app/";
    private DatabaseReference reference;
    private Query leaderboardQuery;
    private DatabaseReference station1Ref;
    private DatabaseReference station2Ref;

    
    [SerializeField] private bool mockPopulateDatabase = false;
    
    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    
    private async void Awake() 
    {
        try
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebaseAuthentication();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Exception during db initialization: " + e.Message);
        }
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromLeaderboard();
        UnsubscribeFromStationStatuses();

        if (Instance == this)
            Instance = null;
    }
    
    private void InitializeFirebaseAuthentication()
    {
        auth = FirebaseAuth.DefaultInstance;

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            currentUser = task.Result.User;
            Debug.Log($"User signed in anonymously. Auth ID: {currentUser.UserId}");

            InitializeDatabase();
        });
    }

    private void InitializeDatabase()
    {
        var db = FirebaseDatabase.GetInstance(DatabaseURL);
        reference = db.RootReference;

        Debug.Log("db Connected");

        if (mockPopulateDatabase) MockPopulateDatabase();

        OnFirebaseInitialized?.Invoke();
    }
    

    public async Task<List<(string, int)>> GetScoresAsync()
    {
        List<(string, int)> topScores = new();

        if (reference == null) return topScores;
        try
        {
            var query = reference.Child("users").OrderByChild("score").LimitToLast(5);
            
            var snapshot = await query.GetValueAsync();

            if (snapshot.Exists && snapshot.HasChildren)
            {
                foreach (var childSnapshot in snapshot.Children)
                {
                    if (!childSnapshot.HasChild("username") || !childSnapshot.HasChild("score")) continue;
                    
                    var playerName = childSnapshot.Child("username").Value.ToString();
                    var playerScore = Convert.ToInt32(childSnapshot.Child("score").Value);

                    topScores.Add((playerName, playerScore));
                    
                }
                topScores.Reverse();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Database error: " + e.Message);
        }

        return topScores;
    }
    
    public void SubmitScore(string username, int score) 
    { 
        var newScoreRef = reference.Child("users").Push();

        var newUser = new User(username, score);
        var json = JsonUtility.ToJson(newUser);
        
        newScoreRef.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Database error: " + task.Exception);
            }
            else 
            {
                Debug.Log($"Created new arcade entry for: {username} with score: {score}");
            }
        });
    }
    
    public void SubmitLeaderboardEntry(string username, int score, bool defused, string stationId)
    {
        if (reference == null)
        {
            Debug.LogError("Cannot submit leaderboard entry because database reference is null.");
            return;
        }

        var newScoreRef = reference.Child("leaderboard").Push();

        var newEntry = new LeaderboardEntryData(username, score, defused, stationId);
        var json = JsonUtility.ToJson(newEntry);

        newScoreRef.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Leaderboard submit error: " + task.Exception);
            }
            else
            {
                Debug.Log($"Created new leaderboard entry for: {username} with score: {score}, defused: {defused}, station: {stationId}");
            }
        });
    }
    
    public void UpdateStationStatus(string stationId, string teamName, string status)
    {
        if (reference == null)
        {
            Debug.LogError("Cannot update station status because database reference is null.");
            return;
        }

        var stationData = new StationStatusData(teamName, status);
        var json = JsonUtility.ToJson(stationData);

        reference.Child("stations").Child(stationId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Station update error for {stationId}: " + task.Exception);
            }
            else
            {
                Debug.Log($"Updated station {stationId}: {teamName} / {status}");
            }
        });
        Debug.Log($"UpdateStationStatus called: station={stationId}, team={teamName}, status={status}, referenceNull={reference == null}");
    }
    
    public void SubscribeToTopScores(int amount = 10)
    {
        if (reference == null)
        {
            Debug.LogWarning("Cannot subscribe to top scores because database reference is null.");
            return;
        }

        UnsubscribeFromLeaderboard();

        leaderboardQuery = reference.Child("leaderboard").OrderByChild("score").LimitToLast(amount);
        leaderboardQuery.ValueChanged += HandleLeaderboardChanged;
    }

    public void SubscribeToStationStatuses()
    {
        if (reference == null)
        {
            Debug.LogWarning("Cannot subscribe to station statuses because database reference is null.");
            return;
        }

        UnsubscribeFromStationStatuses();

        station1Ref = reference.Child("stations").Child("station1");
        station2Ref = reference.Child("stations").Child("station2");

        station1Ref.ValueChanged += HandleStation1Changed;
        station2Ref.ValueChanged += HandleStation2Changed;
    }
    
    public void UnsubscribeFromLeaderboard()
    {
        if (leaderboardQuery != null)
        {
            leaderboardQuery.ValueChanged -= HandleLeaderboardChanged;
            leaderboardQuery = null;
        }
    }

    public void UnsubscribeFromStationStatuses()
    {
        if (station1Ref != null)
        {
            station1Ref.ValueChanged -= HandleStation1Changed;
            station1Ref = null;
        }

        if (station2Ref != null)
        {
            station2Ref.ValueChanged -= HandleStation2Changed;
            station2Ref = null;
        }
    }

        private void HandleLeaderboardChanged(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("Leaderboard update error: " + args.DatabaseError.Message);
                return;
            }

            List<LeaderboardEntryData> entries = new();

            if (args.Snapshot != null && args.Snapshot.Exists && args.Snapshot.HasChildren)
            {
                foreach (var childSnapshot in args.Snapshot.Children)
                {
                    try
                    {
                        string username = childSnapshot.HasChild("username")
                            ? childSnapshot.Child("username").Value.ToString()
                            : "---";

                        int score = childSnapshot.HasChild("score")
                            ? Convert.ToInt32(childSnapshot.Child("score").Value)
                            : 0;

                        bool defused = childSnapshot.HasChild("defused") &&
                                       Convert.ToBoolean(childSnapshot.Child("defused").Value);

                        string stationId = childSnapshot.HasChild("stationId")
                            ? childSnapshot.Child("stationId").Value.ToString()
                            : string.Empty;

                        entries.Add(new LeaderboardEntryData(username, score, defused, stationId));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Failed to parse leaderboard entry: " + e.Message);
                    }
                }

                entries = entries
                    .OrderByDescending(x => x.score)
                    .Take(10)
                    .ToList();
            }

            OnTopLeaderboardUpdated?.Invoke(entries);
        }

        private void HandleStation1Changed(object sender, ValueChangedEventArgs args)
        {
            HandleStationChanged("station1", args);
        }

        private void HandleStation2Changed(object sender, ValueChangedEventArgs args)
        {
            HandleStationChanged("station2", args);
        }

        private void HandleStationChanged(string stationId, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"Station update error for {stationId}: " + args.DatabaseError.Message);
                return;
            }

            StationStatusData data = new("---", "Waiting");

            if (args.Snapshot != null && args.Snapshot.Exists)
            {
                try
                {
                    if (args.Snapshot.HasChild("teamName"))
                    {
                        data.teamName = args.Snapshot.Child("teamName").Value?.ToString() ?? "---";
                    }

                    if (args.Snapshot.HasChild("status"))
                    {
                        data.status = args.Snapshot.Child("status").Value?.ToString() ?? "Waiting";
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to parse station status for {stationId}: " + e.Message);
                }
            }

            OnStationStatusUpdated?.Invoke(stationId, data);
        }
    // public void CheckAndUpdateUser(string userId, string username, string score) 
    // { 
    //     var userRef = reference.Child("users").Child(userId);
    //
    //     userRef.GetValueAsync().ContinueWithOnMainThread(task =>
    //     {
    //         if (task.IsFaulted)
    //         {
    //             Debug.LogError("Database error: " + task.Exception);
    //             return;
    //         }
    //
    //         var snapshot = task.Result;
    //
    //         if (snapshot.Exists)
    //         {
    //             userRef.Child("score").SetValueAsync(score);
    //             Debug.Log($"Updated score for existing user: {userId}");
    //         }
    //         else
    //         {
    //             var newUser = new User(username, score);
    //             var json = JsonUtility.ToJson(newUser);
    //             
    //             userRef.SetRawJsonValueAsync(json);
    //             Debug.Log($"Created new user: {userId}");
    //         }
    //     });
    // }

    private void MockPopulateDatabase()
    {
        SubmitScore("Felix", -2000);
        SubmitScore("Jude", -1000);
        SubmitScore("Elijah", -3000);
        SubmitScore("Eddie", -5000);
        SubmitScore("Sriram", -7000);
        SubmitScore("Kevin", -3200);
    }
}
}
