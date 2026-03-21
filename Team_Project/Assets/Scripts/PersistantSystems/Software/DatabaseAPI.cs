using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
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
    
    public event Action OnFirebaseInitialized;
    private const string DatabaseURL = "https://teamproject-c1ec1-default-rtdb.europe-west1.firebasedatabase.app/";
    private DatabaseReference reference;
    
    [SerializeField] private bool mockPopulateDatabase = false;
    
    
    private async void Awake() 
    {
        try
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == DependencyStatus.Available)
            {
                var db = FirebaseDatabase.GetInstance(DatabaseURL);
                reference = db.RootReference;

                Debug.Log("Firebase is ready and Database is connected!");

                if (mockPopulateDatabase) MockPopulateDatabase();

                OnFirebaseInitialized?.Invoke();
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
                
                Debug.Log($"Found {topScores.Count} Scores");
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
