using NUnit.Framework;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tests.Managers
{
    [TestFixture]
    public class T_GridManager
    {
        private GameObject _environmentRoot;
        private GameObject _spawnPoint;
        private GridManager _manager;

        [SetUp]
        public void Setup()
        {
            _environmentRoot = new GameObject("TestEnvironment");
            
            _spawnPoint = new GameObject("SpawnPoint");
            _spawnPoint.transform.SetParent(_environmentRoot.transform);
            _spawnPoint.transform.position = Vector3.zero;
            
            GameObject gmObj = new GameObject("GridManager");
            gmObj.transform.SetParent(_environmentRoot.transform);
            
            _manager = gmObj.AddComponent<GridManager>();
            _manager.spawnPoint = _spawnPoint;

            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    GameObject floorObj = new GameObject($"Floor_{x}_{y}");
                    floorObj.transform.SetParent(_environmentRoot.transform);
                    
                    floorObj.SetActive(false); // Delay Awake

                    floorObj.AddComponent<MeshRenderer>(); 
                    BoxCollider col = floorObj.AddComponent<BoxCollider>();
                    col.size = new Vector3(1, 1, 1);

                    CrackFloor floor = floorObj.AddComponent<CrackFloor>();
                    
                    var type = typeof(CrackFloor);
                    type.GetField("gridX", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(floor, x);
                    type.GetField("gridY", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(floor, y);
                    type.GetField("floorCollider", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(floor, col);
                    
                    floorObj.SetActive(true);
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_environmentRoot != null)
            {
                Object.DestroyImmediate(_environmentRoot);
            }

            GridManager[] leakingManagers = Object.FindObjectsByType<GridManager>(FindObjectsSortMode.None);
            foreach (GridManager man in leakingManagers)
            {
                if (man != null) Object.DestroyImmediate(man.gameObject);
            }
        }

        [Test]
        public void GridManager_Start_InitializesGridAndFindsFloors()
        {
            _manager.Start();
        
            Assert.IsNotNull(_manager.floors, "GridManager should populate the floors array in Start.");
            Assert.AreEqual(36, _manager.floors.Length, "GridManager should find exactly 36 CrackFloor objects.");
        }

        [Test]
        public void BuildPath_EnsuresStartPointIsAlwaysSafe()
        {
            _manager.safeUnsafeProbabilty = 0f; 
            
            _manager.Start();

            Assert.IsFalse(_manager.floors[0].willBreak, "The starting floor (0,0) must be safe to stand on.");
        }

        [Test]
        public void ResetPath_ClearsOldPathAndGeneratesNewOne()
        {
            _manager.Start();

            _manager.floors[0].willBreak = true;
            _manager.ResetPath();

            Assert.IsFalse(_manager.floors[0].willBreak, "ResetPath should have regenerated the path and made the start point safe again.");
        }

        [Test]
        public void SafeUnsafeProbability_WhenSetToOne_MakesAllFloorsSafe()
        {
            _manager.safeUnsafeProbabilty = 1.0f; 
            
            _manager.Start();
            int breakingFloorsCount = _manager.floors.Count(f => f.willBreak);

            Assert.AreEqual(0, breakingFloorsCount, "With a probability of 1.0, no floors should be marked to break.");
        }
    }
}