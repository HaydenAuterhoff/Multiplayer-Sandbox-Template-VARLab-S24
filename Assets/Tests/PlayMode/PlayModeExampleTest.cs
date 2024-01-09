using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.Interactions;

// Note: This is a poor example of a proper test and is currently only used to ensure tests run properly 
// on the DevOps pipeline.
public class PlayModeExampleTest
{
    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Sandbox");
    }

    [UnityTest]
    public IEnumerator SafetyHatIsClickable()
    {
        bool clicked = false;
        GameObject safetyHat = GameObject.Find("Safety Hat");
        Interactable interactable = safetyHat.GetComponent<Interactable>();
        
        interactable.MouseClick.AddListener(delegate { clicked = true; });
        interactable.MouseClick.Invoke(safetyHat);

        yield return new WaitForSeconds(2);
        Assert.IsTrue(clicked);
    }
}
