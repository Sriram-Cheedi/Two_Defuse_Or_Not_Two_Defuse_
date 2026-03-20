using System;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

[System.Serializable] 
public class User {
    public string username;
    public string time;

    public User() {}
    public User(string username, string time) {
        this.username = username;
        this.time = time;
    }
}

public class DatabaseManager: MonoBehaviour
{
    private DatabaseReference reference;
    
    // Paste the URL you copied from the Firebase Console here!
    private string databaseURL = "https://teamproject-c1ec1-default-rtdb.europe-west1.firebasedatabase.app/";
    
    async void Start() 
    {
        
        DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        
        if (dependencyStatus == DependencyStatus.Available) 
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            reference = FirebaseDatabase.GetInstance(app, databaseURL).RootReference;
            
            Debug.Log("Firebase is ready and Database is connected!");
            
            CheckAndUpdateUser("test_user_123", "PlayerOne", "100"); 
        } 
        else 
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }

    public void CheckAndUpdateUser(string userId, string name, string currentTime) 
    { 
        DatabaseReference userRef = reference.Child("users").Child(userId);

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Database error: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                userRef.Child("time").SetValueAsync(currentTime);
                Debug.Log($"Updated time for existing user: {userId}");
            }
            else
            {
                User newUser = new User(name, currentTime);
                string json = JsonUtility.ToJson(newUser);
                
                userRef.SetRawJsonValueAsync(json);
                Debug.Log($"Created new user profile for: {userId}");
            }
        });
    }

    public void GetTopScores()
    {
        
    }
}