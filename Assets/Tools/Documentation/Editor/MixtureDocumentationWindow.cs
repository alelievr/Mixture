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
    static string nodeManualDir => docFxDir + "/manual/nodes/";

    static readonly int toolbarHeight = 21;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Mixture DocTool")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MixtureDocumentationWindow window = (MixtureDocumentationWindow)EditorWindow.GetWindow(typeof(MixtureDocumentationWindow));
        window.titleContent  = new GUIContent("Mixture Doc");
        window.Show();
    }

    void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    void OnGUI()
    {
        if (GUILayout.Button("Update Node Documentation"))
        {
            // TODO: remove generated images + md files to cleanup deleted nodes
            UpdateNodeDoc();
        }
    }

    void EditorUpdate()
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
        docGraph.position = new Vector3(0, toolbarHeight, 0);
        docGraph.nodes.RemoveAll(n => !(n is OutputNode));

        // yield return null;

        var window = EditorWindow.CreateWindow<MixtureGraphWindow>();
        window.Show();
        window.Focus();

        window.minSize = new Vector2(1024, 1024);
        window.maxSize = new Vector2(1024, 1024);

        var nodeViews = new List<BaseNodeView>();
        foreach (var node in NodeProvider.GetNodeMenuEntries())
        {
            if (node.Key.Contains("Experimental"))
                continue;

            // Skip non-mixture nodes:
            if (!node.Value.FullName.Contains("Mixture"))
                continue;

            // We'll suport loops after
            if (node.Value == typeof(ForeachStart) || node.Value == typeof(ForStart))
                continue;

            window.InitializeGraph(docGraph);
            var graphView = window.view;
            var newNode = BaseNode.CreateFromType(node.Value, new Vector2(0, toolbarHeight));
            var nodeView = graphView.AddNode(newNode);
            nodeViews.Add(nodeView);
            graphView.Add(nodeView);
            SetupNodeIfNeeded(nodeView);

            graphView.MarkDirtyRepaint();
            graphView.UpdateViewTransform(new Vector3(0, 0, 0), Vector3.one * graphView.scale);
            graphView.Focus();

            yield return new WaitForEndOfFrame();

            if (window == null)
                yield break;

            TakeAndSaveNodeScreenshot(window, nodeView);

            GenerateNodeMarkdownDoc(nodeView);

            graphView.RemoveNodeView(nodeView);
            graphView.graph.RemoveNode(nodeView.nodeTarget);

            graphView.MarkDirtyRepaint();
            yield return new WaitForEndOfFrame();
        }

        nodeViews.Sort((n1, n2) => n1.nodeTarget.name.CompareTo(n2.nodeTarget.name));

        GenerateNodeIndexFiles(nodeViews);

        foreach (var node in docGraph.nodes.ToList())
            if (!(node is OutputNode))
                docGraph.RemoveNode(node);

        window.Close();
    }

    void TakeAndSaveNodeScreenshot(MixtureGraphWindow window, BaseNodeView nodeView)
    {
        var size = new Vector2(nodeView.resolvedStyle.width, nodeView.resolvedStyle.height);
        var windowRoot = window.rootVisualElement;
        var nodePosition = nodeView.parent.worldBound.position;
        nodePosition += window.position.position + new Vector2(0, toolbarHeight);

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
        using (var fs = new FileStream(nodeManualDir + view.nodeTarget.GetType().ToString() + ".md", FileMode.OpenOrCreate))
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

                if (view.nodeTarget is ShaderNode s)
                {
                    // In case a node doesn't support all dimensions:
                    if (s.supportedDimensions.Count != 3)
                    {
                        sw.WriteLine("Please note that this node only support " + string.Join(" and ", s.supportedDimensions) + " dimension(s).");
                    }
                }

                sw.Flush();
            }
        }
    }

    string GetImageLink(BaseNode node) => $"../../images/{node.GetType()}.png";

    string GetNodeName(BaseNodeView view)
    {
        string name = view.nodeTarget.name;

        // Special case for the compute shader node as the name of the node is inherited from the compute shader.
        name = name == "DocumentationComputeShader" ? "Compute Shader" : name;

        return name;
    }

    void GenerateNodeIndexFiles(List<BaseNodeView> nodeViews)
    {
        using (var fs = new FileStream(nodeManualDir + nodeIndexFile, FileMode.OpenOrCreate))
        {
            fs.SetLength(0);

            using (var sw = new StreamWriter(fs))
            {
                foreach (var nodeView in nodeViews)
                {

                    sw.WriteLine($"[{GetNodeName(nodeView)}]({nodeView.nodeTarget.GetType()}.md)  ");
                    sw.WriteLine();
                }
                sw.Flush();
            }
        }

        // We also generate toc.yml in the nodes folder
        using (var fs = new FileStream(nodeManualDir + "toc.yml", FileMode.OpenOrCreate))
        {
            fs.SetLength(0);

            using (var sw = new StreamWriter(fs))
            {
                foreach (var nodeView in nodeViews)
                {
                    sw.WriteLine($"- name: {GetNodeName(nodeView)}");
                    sw.WriteLine($"  href: {nodeView.nodeTarget.GetType().ToString()}.md");
                }
            }
        }
    }
}
