using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class API_StructureTests
{
    private GameObject _testParent;
    private API_Provider _provider;

    [SetUp]
    public void Setup()
    {
        _testParent = new GameObject();
        _provider = _testParent.AddComponent<API_Provider>();

        GameObject child = new GameObject();
        child.transform.SetParent(_testParent.transform);
        child.AddComponent<API_Controller>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testParent);
    }

    [Test]
    public void Provider_AssignsMock_WhenToggleIsTrue()
    {
        _provider.useMockHardware = true;

        // This calls the private awake method
        MethodInfo awakeMethod = typeof(API_Provider).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        awakeMethod.Invoke(_provider, null);

        Assert.IsInstanceOf<Mock_API_Controller>(_provider.ActiveAPIController,
            "Provider should assign a Mock controller when useMockHardware is true.");
    }

    [Test]
    public void MockController_ReturnsSimulatedGyro()
    {
        var mock = new Mock_API_Controller();
        mock.Initialize();

        Vector3 gyro = mock.getGyroValue;

        Assert.AreEqual(Vector3.zero, gyro, "Initial mock gyro should be zero.");
    }
}
