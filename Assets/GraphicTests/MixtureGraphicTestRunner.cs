using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Graphics;
using System.IO;
using UnityEditor;
using System.Linq;
using UnityEngine.Experimental.Rendering;

namespace Mixture
{
    class MixtureGraphicTestRunner
    {
        const string mixtureTestFolder = "Assets/GraphicTests/Mixtures/";
        const string referenceImagesFolder = "Assets/GraphicTests/ReferenceImages/";
        
        public struct MixtureTestCase
        {
            public MixtureGraph graph;
            public Texture2D    expected;

            public override string ToString() => graph.name;
        }

        public static IEnumerable<MixtureTestCase> GetMixtureTestCases()
        {
            foreach (var assetPath in Directory.GetFiles(mixtureTestFolder, "*.asset", SearchOption.AllDirectories))
            {
                var graph = MixtureEditorUtils.GetGraphAtPath(assetPath);
                string graphName = Path.GetFileNameWithoutExtension(assetPath);
                string referenceImagePath = Path.Combine(referenceImagesFolder, graphName + ".png");
                var expectedImage = AssetDatabase.LoadAssetAtPath<Texture2D>(referenceImagePath);

                if (graph != null)
                {
                    yield return new MixtureTestCase
                    {
                        graph = graph,
                        expected = expectedImage
                    };
                }
            }
        }

        // [PrebuildSetup("SetupGraphicsTestCases")] // TODO: enable this?
        [UnityTest]
        [Timeout(300 * 1000)] // Set timeout to 5 minutes to handle complex scenes with many shaders (default timeout is 3 minutes)
        public IEnumerator MixtureTests([ValueSource(nameof(GetMixtureTestCases))] MixtureTestCase testCase)
        {
            ShaderUtil.allowAsyncCompilation = false;

            var result = ExecuteAndReadback(testCase.graph);

            if (testCase.expected == null)
            {
                string expectedPath = referenceImagesFolder + testCase.graph.name + ".png";
                Debug.Log($"No reference image found for {testCase.graph}, Creating one at {expectedPath}");

                var bytes = ImageConversion.EncodeToPNG(result);
                File.WriteAllBytes(expectedPath, bytes);
                AssetDatabase.ImportAsset(expectedPath);
            }
            else
            {
                var settings = testCase.graph.outputNode.rtSettings;
                Texture2D destination = new Texture2D(
                    settings.GetWidth(testCase.graph),
                    settings.GetHeight(testCase.graph),
                    settings.GetGraphicsFormat(testCase.graph), // We only use this format for tests
                    TextureCreationFlags.None
                );

                // Convert image to RGBA
                var colors = testCase.expected.GetPixels();
                destination.SetPixels(colors);

                ImageAssert.AreEqual(destination, result, new ImageComparisonSettings{
                    TargetWidth = destination.width,
                    TargetHeight = destination.height,
                    PerPixelCorrectnessThreshold = 0.001f,
                    AverageCorrectnessThreshold = 0.0015f,
                    UseHDR = false,
                    UseBackBuffer = false,
                });
            }

            yield return null;
        }

        Texture2D ExecuteAndReadback(MixtureGraph graph)
        {
            // Process the graph andreadback the result
            var processor = new MixtureGraphProcessor(graph);
            processor.Run();

            graph.outputNode.enableCompression = false;
            var settings = graph.outputNode.rtSettings;
            Texture2D destination = new Texture2D(
                settings.GetWidth(graph),
                settings.GetHeight(graph),
                settings.GetGraphicsFormat(graph),
                TextureCreationFlags.None
            );

            graph.ReadbackMainTexture(destination);

            // Output the image to a file

            return destination;
        }

    #if UNITY_EDITOR

        [TearDown]
        public void TearDown()
        {
            UnityEditor.TestTools.Graphics.ResultsUtility.ExtractImagesFromTestProperties(TestContext.CurrentContext.Test);
            ShaderUtil.allowAsyncCompilation = true;
        }
    #endif

    }
}