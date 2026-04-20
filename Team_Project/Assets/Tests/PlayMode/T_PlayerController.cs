using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Tests.Player
{
    [TestFixture]
    public class T_PlayerController
    {
        private GameObject _playerObj;
        private PlayerController _playerController;
        private CharacterController _characterController;
        private GameObject _apiProviderObj;

        [SetUp]
        public void Setup()
        {
            LogAssert.Expect(LogType.Error, new Regex("Failed connect to hardware"));
            
            _apiProviderObj = new GameObject("API_Provider");
            API_Provider provider = _ apiProviderObj.AddComponent<API_Provider>();
            provider.useMockHardware = true;
            
            _playerObj = new GameObject("Player");
            _playerObj.transform.position = new Vector3(0, 10, 0);
            
            _characterController = _playerObj.AddComponent<CharacterController>();
            
            PlayerInput playerInput = _playerObj.AddComponent<PlayerInput>();
            InputActionAsset dummyAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            dummyAsset.AddActionMap(new InputActionMap("Player"));
            playerInput.actions = dummyAsset;

            _playerController = _playerObj.AddComponent<PlayerController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObj != null) Object.DestroyImmediate(_playerObj);
            if (_apiProviderObj != null) Object.DestroyImmediate(_apiProviderObj);

            typeof(API_Provider).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                ?.SetValue(null, null);
        }

        [Test]
        public void PlayerController_Awake_CalculatesCrouchAndStandHeights()
        {
            float standHeight = (float)GetPrivateField("standHeight");
            float crouchHeight = (float)GetPrivateField("crouchHeight");

            Assert.AreEqual(2f, standHeight, "Default stand height should match CharacterController default (2.0).");
            Assert.AreEqual(1f, crouchHeight, "Crouch height should be exactly half of stand height.");
        }

        [UnityTest]
        public IEnumerator Player_Crouch_ReducesControllerHeight()
        {
            SetPrivateField("crouchPressed", true);
            SetPrivateField("grounded", true); 

            for (int i = 0; i < 20; i++)
            {
                yield return null; 
            }

            Assert.Less(_characterController.height, 1.9f, "Controller height should be lerping downward towards crouch height.");
        }

        [UnityTest]
        public IEnumerator Player_ObstacleAbove_PreventsStandingUp()
        {
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.transform.position = _playerObj.transform.position + Vector3.up * 1.25f;
            
            SetPrivateField("crouchPressed", true);
            for (int i = 0; i < 20; i++) yield return null; // Let it shrink
            SetPrivateField("crouchPressed", false);
            for (int i = 0; i < 10; i++) yield return null; // Wait to see if it grows

            Assert.Less(_characterController.height, 1.95f, "Player should not be able to stand up while an obstacle is above them.");

            Object.DestroyImmediate(ceiling);
        }

        [UnityTest]
        public IEnumerator Player_Gravity_PullsPlayerDownward()
        {
            float startY = _playerObj.transform.position.y;
            for (int i = 0; i < 5; i++)
            {
                yield return null; 
            }

            Assert.Less(_playerObj.transform.position.y, startY, "Player should fall downward due to gravity.");
        }

        [Test]
        public void RotateLevel_WithStrongNegativeZAccelerometer_InvertsGravity()
        {
            Vector3 dummyGyro = Vector3.zero;
            Vector3 accelerometerUpsideDown = new Vector3(0, 0, -10f);

            MethodInfo rotateMethod = typeof(PlayerController).GetMethod("RotateLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            rotateMethod.Invoke(_playerController, new object[] { dummyGyro, accelerometerUpsideDown });

            bool invertGravity = (bool)GetPrivateField("invertGravity");
            bool invertedPlayer = (bool)GetPrivateField("invertedPlayer");

            Assert.IsTrue(invertGravity, "Gravity should invert when Accelerometer Z is less than -9.");
            Assert.IsTrue(invertedPlayer, "Player inversion flag should be set to true.");
        }
        
        private object GetPrivateField(string fieldName)
        {
            return typeof(PlayerController)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(_playerController);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            typeof(PlayerController)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_playerController, value);
        }
    }
}