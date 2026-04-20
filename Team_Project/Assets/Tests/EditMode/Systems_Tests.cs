using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class API_StructureTests
{
    private GameObject _testParent;
    private API_Provider _provider;

    private GameObject _puzzleParent;
    private PuzzleManager _puzzleManager;

    [SetUp]
    public void Setup()
    {
        _puzzleParent = new GameObject();
        _puzzleManager = _puzzleParent.AddComponent<PuzzleManager>();
        
        MethodInfo awakeMethod = typeof(PuzzleManager).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        awakeMethod.Invoke(_puzzleManager, null);

        _testParent = new GameObject();
        _provider = _testParent.AddComponent<API_Provider>();

        GameObject child = new GameObject();
        child.transform.SetParent(_testParent.transform);
        child.AddComponent<API_Controller>();
    }

    [TearDown]
    public void Teardown()
    {
        var instanceProperty = typeof(PuzzleManager).GetProperty("Instance",
            BindingFlags.Static | BindingFlags.Public);
        instanceProperty.SetValue(null, null);

        Object.DestroyImmediate(_puzzleParent);
        Object.DestroyImmediate(_testParent);
    }

    [Test]
    public void MockController_ReturnsSimulatedGyro()
    {
        var mock = new Mock_API_Controller();
        mock.InitializeSelf();

        Vector3 gyro = mock.getGyroValue;

        Assert.AreEqual(Vector3.zero, gyro, "Initial mock gyro should be zero.");
    }
}
