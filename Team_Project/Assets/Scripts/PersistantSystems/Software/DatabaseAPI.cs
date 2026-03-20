using System;
using System.Collections.Generic;
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
    public string score = string.Empty;

    public User() {}
    public User(string username, string score) {
        this.username = username;
        this.score = score;
    }
}

public class DatabaseAPI: MonoBehaviour
{
    public event Action OnFirebaseInitialized;
    private const string DatabaseURL = "https://teamproject-c1ec1-default-rtdb.europe-west1.firebasedatabase.app/";
    private DatabaseReference reference;
    
    private async void Awake() 
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == DependencyStatus.Available)
            {
                var db = FirebaseDatabase.GetInstance(DatabaseURL);
                reference = db.RootReference;

                Debug.Log("Firebase is ready and Database is connected!");

                MockPopulateDatabase();

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

    public async Task<List<(string, string, string)>> GetScoresAsync()
    {
        List<(string, string, string)> topScores = new();

        if (reference == null) return topScores;
        try
        {
            var snapshot = await reference.Child("users").GetValueAsync();

            if (snapshot.Exists && snapshot.HasChildren)
            {
                foreach (var childSnapshot in snapshot.Children)
                {
                    if (!childSnapshot.HasChild("username") || !childSnapshot.HasChild("score")) continue;
                    
                    var playerName = childSnapshot.Child("username").Value.ToString();
                    var playerScore = childSnapshot.Child("score").Value.ToString();

                    topScores.Add((playerName, playerScore, playerName));
                    
                }
                Debug.Log($"Found {topScores.Count} Scores");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Database error: " + e.Message);
        }
        return topScores; 
    }

    private void CheckAndUpdateUser(string userId, string username, string score) 
    { 
        var userRef = reference.Child("users").Child(userId);

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Database error: " + task.Exception);
                return;
            }

            var snapshot = task.Result;

            if (snapshot.Exists)
            {
                userRef.Child("score").SetValueAsync(score);
                Debug.Log($"Updated score for existing user: {userId}");
            }
            else
            {
                var newUser = new User(username, score);
                var json = JsonUtility.ToJson(newUser);
                
                userRef.SetRawJsonValueAsync(json);
                Debug.Log($"Created new user: {userId}");
            }
        });
    }

    private void MockPopulateDatabase()
    {
        CheckAndUpdateUser("1", "Felix", "2000");
        CheckAndUpdateUser("2", "Jude", "1000");
        CheckAndUpdateUser("3", "Elijah", "3000");
        CheckAndUpdateUser("4", "Eddie", "5000");
        CheckAndUpdateUser("5", "Sriram", "7000");
        CheckAndUpdateUser("5", "Kevin", "92000");
    }
}
}
