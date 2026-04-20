using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Systems
{
    [TearDown]
    public void TearDown()
    {
        API_Provider[] leakingProviders = Object.FindObjectsOfType<API_Provider>();
        foreach (API_Provider p in leakingProviders)
        {
            if (p != null) Object.DestroyImmediate(p.gameObject);
        }
        
        typeof(API_Provider).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
            ?.SetValue(null, null);
    }

    [Test]
    public void Provider_AssignsMock_WhenToggleIsTrue()
    {
        GameObject testObj = new GameObject("TestObject");
        
        testObj.SetActive(false);
        API_Provider provider = testObj.AddComponent<API_Provider>(); 
        
        provider.useMockHardware = true;
        
        testObj.SetActive(true);
        
        Assert.IsInstanceOf<Mock_API_Controller>(provider.ActiveAPIController, "Provider should assign a Mock controller when useMockHardware is true.");
    }

    [Test]
    public void Provider_AssignsReal_WhenToggleIsFalse()
    {
        LogAssert.Expect(LogType.Error, new Regex("Failed connect to hardware"));

        GameObject testObj = new GameObject("TestObject");
        
        testObj.SetActive(false);
        API_Provider provider = testObj.AddComponent<API_Provider>(); 

        provider.useMockHardware = false;

        testObj.SetActive(true);

        Assert.IsInstanceOf<Mock_API_Controller>(provider.ActiveAPIController, 
            "System did not fall back to Mock when real hardware failed to connect!");
    }
}