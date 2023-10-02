using GraphProcessor;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mixture
{
  [NodeCustomEditor(typeof(SubgraphNode))]
  public class SubgraphNodeView : MixtureNodeView
  {
    SubgraphNode _node;

    protected override void DrawPreviewToolbar(Texture texture)
    {
      base.DrawPreviewToolbar(texture);

      var settings = _node.templateGraph.outputNode.outputTextureSettings;

      _node.previewOutputIndex = EditorGUILayout.Popup(
        Mathf.Clamp(_node.previewOutputIndex, 0, settings.Count - 1),
        settings.Select(setting => setting.name).ToArray(),
        EditorStyles.toolbarDropDown,
        GUILayout.Width(150)
      );
    }

    public override void Enable(bool fromInspector)
    {
      base.Enable(fromInspector);

      _node = nodeTarget as SubgraphNode;

      var templateGraphTextureField = new ObjectField("Graph")
      {
        value = _node.templateGraphTexture,
        objectType = typeof(CustomRenderTexture)
      };
      templateGraphTextureField.RegisterValueChangedCallback(e =>
      {
        _node.templateGraphTexture = e.newValue as CustomRenderTexture;
        UpdateSubgraph();
        title = _node.name;
      });

      controlsContainer.Add(templateGraphTextureField);

      UpdateSubgraph();
    }

    public override void Disable()
    {
      base.Disable();
    }

    void UpdateSubgraph()
    {
      if (_node.templateGraph != null)
      {
        _node.templateGraph.onGraphChanges -= UpdateClonedGraph;
        _node.templateGraph.onExposedParameterModified -= UpdateInputs;
        _node.templateGraph.onExposedParameterListChanged -= UpdateInputs;
        _node.templateGraph.outputNode.onPortsUpdated -= UpdateOutputs;
      }

      _node.templateGraph = MixtureDatabase.GetGraphFromTexture(_node.templateGraphTexture);

      if (ValidateSubgraph())
      {
        _node.templateGraph.onGraphChanges += UpdateClonedGraph;
        _node.templateGraph.onExposedParameterModified += UpdateInputs;
        _node.templateGraph.onExposedParameterListChanged += UpdateInputs;
        _node.templateGraph.outputNode.onPortsUpdated += UpdateOutputs;

        _node.previewOutputIndex = Mathf.Clamp(_node.previewOutputIndex, 0, _node.templateGraph.outputNode.outputTextureSettings.Count - 1);
        _node.UpdateClonedGraph();
        _node.UpdateOutputTextures();
      }
      else
      {
        _node.previewOutputIndex = -1;
        _node.DestroyClonedGraph();
        _node.ReleaseOutputTextures();
      }

      ForceUpdatePorts();
      NotifyNodeChanged();
    }

    void UpdateClonedGraph(GraphChanges _) => _node.UpdateClonedGraph();

    void UpdateInputs()
    {
      _node.UpdateClonedGraph();
      ForceUpdatePorts();
      NotifyNodeChanged();
    }

    void UpdateInputs(ExposedParameter _) => UpdateInputs();

    void UpdateOutputs()
    {
      _node.UpdateClonedGraph();
      _node.UpdateOutputTextures();
      ForceUpdatePorts();
      NotifyNodeChanged();
    }

    void UpdateOutputs(string _) => UpdateOutputs();

    bool ValidateSubgraph()
    {
      _node.ClearMessages();

      if (_node.templateGraphTexture == null)
      {
        return false;
      }

      if (_node.templateGraph == null)
      {
        _node.AddMessage($"Cannot find Mixture graph for texture: {_node.templateGraphTexture.name}", NodeMessageType.Error);
        return false;
      }

      if (_node.templateGraph == _node.graph)
      {
        _node.AddMessage($"Cannot execute graph recursively!", NodeMessageType.Error);
        return false;
      }

      return true;
    }
  }
}
