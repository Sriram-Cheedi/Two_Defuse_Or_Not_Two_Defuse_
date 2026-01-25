using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SuiteAssertion
{
    // A Test behaves as an ordinary method
    [Test]
    public void SuiteAssertionSimplePasses()
    {
        int result = 2 + 2;
        Assert.AreEqual(4, result);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator SuiteAssertionWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
