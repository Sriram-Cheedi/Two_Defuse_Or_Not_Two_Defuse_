using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PersistantSystems.Software;
using PersistantSystems.Software.FirebaseModels;
using System.Text.RegularExpressions;

namespace Tests.Software
{
    [TestFixture]
    public class T_DatabaseAPI
    {
        private GameObject _dbObject;
        private DatabaseAPI _databaseAPI;

        [SetUp]
        public void SetUp()
        {
            _dbObject = new GameObject("DatabaseAPI");
            _databaseAPI = _dbObject.AddComponent<DatabaseAPI>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_dbObject != null)
            {
                Object.DestroyImmediate(_dbObject);
            }

            DatabaseAPI[] leakingAPIs = Object.FindObjectsByType<DatabaseAPI>(FindObjectsSortMode.None);
            foreach (DatabaseAPI api in leakingAPIs)
            {
                if (api != null) Object.DestroyImmediate(api.gameObject);
            }

            typeof(DatabaseAPI).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                ?.SetValue(null, null);
        }

        [Test]
        public void DatabaseAPI_Singleton_AssignsInstanceAndPersists()
        {
            Assert.IsNotNull(DatabaseAPI.Instance, "DatabaseAPI should assign its singleton Instance on Awake.");
            Assert.AreEqual(_databaseAPI, DatabaseAPI.Instance, "The Instance should match the created component.");
        }

        [Test]
        public void DatabaseAPI_Singleton_DestroysDuplicateInstances()
        {
            GameObject duplicateObj = new GameObject("DuplicateDatabaseAPI");
            DatabaseAPI duplicateAPI = duplicateObj.AddComponent<DatabaseAPI>();
            
            Assert.AreEqual(_databaseAPI, DatabaseAPI.Instance, "The singleton Instance should not be overwritten by duplicates.");
            
            Object.DestroyImmediate(duplicateObj);
        }

        [Test]
        public void SubmitLeaderboardEntry_WhenDatabaseNotInitialized_LogsErrorAndReturns()
        {
            LogAssert.Expect(LogType.Error, "Cannot submit leaderboard entry because database reference is null.");

            _databaseAPI.SubmitLeaderboardEntry("TestUser", 1000, true, "station1");

            Assert.Pass();
        }

        [Test]
        public void UpdateStationStatus_WhenDatabaseNotInitialized_LogsErrorAndReturns()
        {
            LogAssert.Expect(LogType.Error, "Cannot update station status because database reference is null.");

            _databaseAPI.UpdateStationStatus("station1", "TeamAlpha", "Active");

            Assert.Pass();
        }

        [Test]
        public void SubscribeToTopScores_WhenDatabaseNotInitialized_LogsWarningAndReturns()
        {
            LogAssert.Expect(LogType.Warning, "Cannot subscribe to top scores because database reference is null.");

            _databaseAPI.SubscribeToTopScores(10);

            Assert.Pass();
        }

        [Test]
        public void SubscribeToStationStatuses_WhenDatabaseNotInitialized_LogsWarningAndReturns()
        {
            LogAssert.Expect(LogType.Warning, "Cannot subscribe to station statuses because database reference is null.");

            _databaseAPI.SubscribeToStationStatuses();

            Assert.Pass();
        }
    }
}