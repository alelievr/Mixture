using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;
using Mixture;
using GraphProcessor;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Reflection;

public class MixtureDocumentationWindow : EditorWindow
{
    IEnumerator generateScreenshotsProcess;

    static string docFxDir => Application.dataPath + "/../docs/docfx";
    static string nodeIndexFile => "NodeLibraryIndex.md";
    static string manualDir => docFxDir + "/manual/nodes/";

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Mixture Documentation")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MixtureDocumentationWindow window = (MixtureDocumentationWindow)EditorWindow.GetWindow(typeof(MixtureDocumentationWindow));
        window.titleContent  = new GUIContent("Mixture Doc");
        window.Show();
    }

    void OnEnable()
    {
        EditorApplication.update += Update;
    }

    void OnGUI()
    {
        if (GUILayout.Button("Update Node Documentation"))
            UpdateNodeDoc();
    }

    void Update()
    {
        if (generateScreenshotsProcess != null)
        {
            if (!generateScreenshotsProcess.MoveNext())
                generateScreenshotsProcess = null;
        }
    }

    void UpdateNodeDoc()
    {
        // TODO: create new mixturegraph, open the window with a min size
        // foreach nodes, create the node and save a screenshot
        // Update the yml of the node with [Documentation]

        generateScreenshotsProcess = GenerateScreenshots();
    }

    IEnumerator GenerateScreenshots()
    {
        var t = Resources.Load<Texture>("DocumentationGraph");//.FirstOrDefault(g => { Debug.Log(g); return g is MixtureGraph;}) as MixtureGraph;
        MixtureGraph docGraph = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(t)).FirstOrDefault(g => g is MixtureGraph) as MixtureGraph;

        // Setup doc graph properties:
        docGraph.scale = Vector3.one;
        docGraph.position = new Vector3(0, 21, 0);

        yield return null;

        var window = EditorWindow.CreateWindow<MixtureGraphWindow>();
        window.Show();
        window.Focus();
        window.InitializeGraph(docGraph);

        window.minSize = new Vector2(1024, 1024);
        var graphView = window.view;

        var nodeViews = new List<BaseNodeView>();
        foreach (var node in NodeProvider.GetNodeMenuEntries())
        {
            // Skip non-mixture nodes:
            if (!node.Value.FullName.Contains("Mixture"))
                continue;

            var baseNode = graphView.graph.AddNode(BaseNode.CreateFromType(node.Value, new Vector2(0, 0)));
            var nodeView = graphView.AddNodeView(baseNode);
            nodeViews.Add(nodeView);
            SetupNodeIfNeeded(nodeView);

            graphView.graph.UpdateComputeOrder();

            // Wait multiple frame so the node displays in the window
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            if (window == null)
                yield break;

            TakeAndSaveNodeScreenshot(window, nodeView);

            GenerateNodeMarkdownDoc(nodeView);

            graphView.RemoveNodeView(nodeView);
            graphView.graph.RemoveNode(baseNode);
            
            yield return null;
        }

        nodeViews.Sort((n1, n2) => n1.nodeTarget.name.CompareTo(n2.nodeTarget.name));

        GenerateNodeIndexMarkdown(nodeViews);

        foreach (var node in docGraph.nodes.ToList())
            if (!(node is OutputNode))
                docGraph.RemoveNode(node);

        window.Close();
    }

    void TakeAndSaveNodeScreenshot(MixtureGraphWindow window, BaseNodeView nodeView)
    {
        var size = new Vector2(nodeView.style.width.value.value, nodeView.worldBound.height);
        var windowRoot = window.rootVisualElement;
        var nodePosition = nodeView.worldBound.position;
        nodePosition += window.position.position;

        var colors = InternalEditorUtility.ReadScreenPixel(nodePosition, (int)size.x, (int)size.y);
        var result = new Texture2D((int)size.x, (int)size.y);
        result.SetPixels(colors);
        var bytes = result.EncodeToPNG();
        Object.DestroyImmediate(result);
        string docImageDir = docFxDir + "/images";
        File.WriteAllBytes(Path.Combine(docImageDir, nodeView.nodeTarget.GetType() + ".png"), bytes);
    }

    void SetupNodeIfNeeded(BaseNodeView nodeView)
    {
        switch (nodeView)
        {
            case AutoComputeShaderNodeView a:
                var csNode = a.nodeTarget as AutoComputeShaderNode;
                a.SetComputeShader(Resources.Load<ComputeShader>("DocumentationComputeShader"));
                a.AutoAllocResource("_Output");
                csNode.UpdateComputeShader();
                break;
        }
    }

    void GenerateNodeMarkdownDoc(BaseNodeView view)
    {
        using (var fs = new FileStream(manualDir + view.nodeTarget.GetType().ToString() + ".md", FileMode.OpenOrCreate))
        {
            fs.SetLength(0);

            using (var sw = new StreamWriter(fs))
            {
                // Append title
                sw.WriteLine($"# {view.nodeTarget.name}");

                // Add link to node image
                sw.WriteLine($"![{view.nodeTarget.GetType()}]({GetImageLink(view.nodeTarget)})");

                // Add node input tooltips
                if (view.inputPortViews.Count > 0)
                {
                    sw.WriteLine($"## Inputs");
                    sw.WriteLine("Port Name | Description");
                    sw.WriteLine("--- | ---");
                    foreach (var pv in view.inputPortViews)
                        sw.WriteLine($"{pv.portData.displayName} | {pv.portData.tooltip ?? ""}");
                }

                // Empty line to end the table
                sw.WriteLine();

                // Add node output tooltips
                if (view.outputPortViews.Count > 0)
                {
                    sw.WriteLine($"## Output");
                    sw.WriteLine("Port Name | Description");
                    sw.WriteLine("--- | ---");
                    foreach (var pv in view.outputPortViews)
                        sw.WriteLine($"{pv.portData.displayName} | {pv.portData.tooltip ?? ""}");
                }

                // Empty line to end the table
                sw.WriteLine();

                sw.WriteLine("## Description");

                // Add node documentation if any
                var docAttr = view.nodeTarget.GetType().GetCustomAttribute<DocumentationAttribute>();
                if (docAttr != null)
                {
                    sw.WriteLine(docAttr.markdown.Trim());
                }

                sw.WriteLine();

                sw.Flush();
            }
        }
    }

    string GetImageLink(BaseNode node) => $"../../images/{node.GetType()}.png";

    void GenerateNodeIndexMarkdown(List<BaseNodeView> nodeViews)
    {
        using (var fs = new FileStream(manualDir + nodeIndexFile, FileMode.OpenOrCreate))
        {
            fs.SetLength(0);

            using (var sw = new StreamWriter(fs))
            {
                foreach (var nodeView in nodeViews)
                {
                    sw.WriteLine($"[{nodeView.nodeTarget.name}]({nodeView.nodeTarget.GetType()}.md)  ");
                    sw.WriteLine();
                }
            }
        }
    }
}
