using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Graphics;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.IO;

public class MixtureGraphicTestRunner
{
    const string mixtureTestFolder = "Assets/GraphicTests/Mixtures/";
    
    // [PrebuildSetup("SetupGraphicsTestCases")] // TODO: enable this?
    [UseGraphicsTestCases]
    [Timeout(300 * 1000)] // Set timeout to 5 minutes to handle complex scenes with many shaders (default timeout is 3 minutes)
    public IEnumerator Run(GraphicsTestCase testCase)
    {
        // We shouldn't need to load a scene?
        SceneManager.LoadScene(testCase.ScenePath);

        // TODO: load all test mixtures, process them and compare the screenshots

        // Load the test settings
        // var settings = GameObject.FindObjectOfType<HDRP_TestSettings>();

        // var camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        // if (camera == null) camera = GameObject.FindObjectOfType<Camera>();
        // if (camera == null)
        // {
        //     Assert.Fail("Missing camera for graphic tests.");
        // }

        // TODO: convert mixture to texture2D for comparison

        // ImageAssert.AreEqual(testCase.ReferenceImage, result2D, settings?.ImageComparisonSettings);

        yield break;
    }

#if UNITY_EDITOR

    [TearDown]
    public void DumpImagesInEditor()
    {
        UnityEditor.TestTools.Graphics.ResultsUtility.ExtractImagesFromTestProperties(TestContext.CurrentContext.Test);
    }

    [TearDown]
    public void ResetSystemState()
    {
        // TODO: remove?
        // XRGraphicsAutomatedTests.running = false;
    }
#endif

}
