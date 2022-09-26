using GraphProcessor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
  [System.Serializable, NodeMenuItem("Subgraph")]
  public class SubgraphNode : MixtureNode, ICreateNodeFrom<CustomRenderTexture>, IRealtimeReset
  {
    // Placeholders for custom port behaviours
    [Input, System.NonSerialized] public int subgraphInputs;
    [Output, System.NonSerialized] public int subgraphOutputs;

    public MixtureGraph templateGraph;
    public CustomRenderTexture templateGraphTexture;
    public int previewOutputIndex = -1;

    List<CustomRenderTexture> _outputTextures = new List<CustomRenderTexture>();
    MixtureGraph _clonedGraph;
    MixtureGraphProcessor _processor;

    public override string name => templateGraph?.name ?? "Subgraph";
    public override bool isRenamable => true;
    public override bool hasPreview => -1 < previewOutputIndex && previewOutputIndex < _outputTextures.Count;
    public override bool canEditPreviewSRGB => false;
    public override Texture previewTexture => hasPreview ? _outputTextures[previewOutputIndex] : null;

    public bool InitializeNodeFromObject(CustomRenderTexture texture)
    {
      templateGraph = MixtureDatabase.GetGraphFromTexture(texture);

      if (templateGraph == null) return false;

      templateGraphTexture = texture;
      previewOutputIndex = Mathf.Clamp(previewOutputIndex, 0, templateGraph.outputNode.outputTextureSettings.Count - 1);

      return true;
    }

    public void RealtimeReset()
    {
      UpdateClonedGraph();

      if (_clonedGraph?.type == MixtureGraphType.Realtime)
      {
        _clonedGraph.RestartRealtime();
      }
    }

    [CustomPortBehavior(nameof(subgraphInputs))]
    public IEnumerable<PortData> ListGraphInputs(List<SerializableEdge> edges)
    {
      if (templateGraph == null || templateGraph == graph) yield break;

      for (var i = 0; i < templateGraph.exposedParameters.Count; i++)
      {
        var parameter = templateGraph.exposedParameters[i];

        yield return new PortData
        {
          identifier = System.Convert.ToString(i),
          displayName = parameter.name,
          displayType = parameter.GetValueType(),
          acceptMultipleEdges = false,
        };
      }
    }

    [CustomPortBehavior(nameof(subgraphOutputs))]
    public IEnumerable<PortData> ListGraphOutputs(List<SerializableEdge> edges)
    {
      if (templateGraph == null || templateGraph == graph) yield break;

      var settings = templateGraph.outputNode.outputTextureSettings;
      var textureType = GetSubgraphTextureType();

      for (var i = 0; i < settings.Count; i++)
      {
        yield return new PortData
        {
          identifier = System.Convert.ToString(i),
          displayName = settings[i].name,
          displayType = textureType,
          acceptMultipleEdges = true,
        };
      }
    }

    [CustomPortInput(nameof(subgraphInputs), typeof(object))]
    public void AssignGraphInputs(List<SerializableEdge> edges)
    {
      foreach (var edge in edges)
      {
        var index = System.Convert.ToInt32(edge.inputPortIdentifier);
        var parameter = _clonedGraph.exposedParameters[index];

        switch (edge.passThroughBuffer)
        {
          // Manually convert between float, Vector2, Vector3 and Vector4
          case float v: parameter.value = CoerceVectorValue(parameter, new Vector4(v, v, v, v)); break;
          case Vector2 v: parameter.value = CoerceVectorValue(parameter, v); break;
          case Vector3 v: parameter.value = CoerceVectorValue(parameter, v); break;
          case Vector4 v: parameter.value = CoerceVectorValue(parameter, v); break;
          default: parameter.value = edge.passThroughBuffer; break;
        }
      }
    }

    [CustomPortOutput(nameof(subgraphOutputs), typeof(object))]
    public void AssignGraphOutputs(List<SerializableEdge> edges)
    {
      foreach (var edge in edges)
      {
        var index = System.Convert.ToInt32(edge.outputPortIdentifier);
        edge.passThroughBuffer = _outputTextures[index];
      }
    }

    public void UpdateClonedGraph()
    {
      DestroyClonedGraph();

      if (templateGraph != null && templateGraph != graph)
      {
        _clonedGraph = templateGraph.Clone();
        _processor = new MixtureGraphProcessor(_clonedGraph);
      }
    }

    public void DestroyClonedGraph()
    {
      _processor?.Dispose();
      _processor = null;
      CoreUtils.Destroy(_clonedGraph);
      _clonedGraph = null;
    }

    public void UpdateOutputTextures()
    {
      ReleaseOutputTextures();

      if (templateGraph != null && templateGraph != graph) GenerateOutputTextures();
    }

    public void GenerateOutputTextures()
    {
      var settings = templateGraph.outputNode.outputTextureSettings;

      _outputTextures.Capacity = Mathf.Max(_outputTextures.Capacity, settings.Count);

      foreach (var setting in settings)
      {
        CustomRenderTexture outputTexture = null;
        UpdateTempRenderTexture(ref outputTexture);
        _outputTextures.Add(outputTexture);
      }
    }

    public void ReleaseOutputTextures()
    {
      _outputTextures.ForEach(CoreUtils.Destroy);
      _outputTextures.Clear();
    }

    protected override void Enable()
    {
      base.Enable();

      UpdateClonedGraph();
      UpdateOutputTextures();
    }

    protected override void Disable()
    {
      base.Disable();

      DestroyClonedGraph();
      ReleaseOutputTextures();
    }

    public override bool canProcess => base.canProcess && templateGraph != null && templateGraph != graph;

    protected override bool ProcessNode(CommandBuffer cmd)
    {
      if (!base.ProcessNode(cmd)) return false;

      // _clonedGraph equals to null when stopping Play Mode
      if (_clonedGraph == null) UpdateClonedGraph();

      _processor.Run();

      using (var copyCmd = new CommandBuffer { name = $"{graph.name}/{_clonedGraph.name}-{GUID.Substring(0, 6)}/Copy" })
      {
        for (int i = 0; i < _outputTextures.Count; i++)
        {
          var inputTexture = _clonedGraph.outputNode.outputTextureSettings[i].inputTexture;

          if (inputTexture != null)
          {
            TextureUtils.CopyTexture(copyCmd, inputTexture, _outputTextures[i]);
          }
        }

        Graphics.ExecuteCommandBuffer(copyCmd);
      }

      return true;
    }

    System.Type GetSubgraphTextureType()
    {
      var textureDimension = templateGraph.settings.GetResolvedTextureDimension(templateGraph);

      switch (textureDimension)
      {
        case UnityEngine.Rendering.TextureDimension.Tex2D: return typeof(Texture2D);
        case UnityEngine.Rendering.TextureDimension.Tex3D: return typeof(Texture3D);
        case UnityEngine.Rendering.TextureDimension.Cube: return typeof(Cubemap);
        default: throw new System.Exception($"Texture dimension not supported: {textureDimension}");
      }
    }

    object CoerceVectorValue(ExposedParameter parameter, Vector4 vector)
    {
      switch (parameter.value)
      {
        case float _: return vector.x;
        case Vector2 _: return (Vector2)vector;
        case Vector3 _: return (Vector3)vector;
        case Vector4 _: return vector;
        default: throw new System.Exception($"Cannot cast vector to {parameter.GetValueType()}");
      }
    }
  }
}
