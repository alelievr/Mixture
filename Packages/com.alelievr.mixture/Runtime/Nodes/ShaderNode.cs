using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[Documentation(@"
This node is the base node of all shader operations, it allows you to create a node with a custom behavior by putting a shader in the Shader field.
Note that the shader must be compatible with Custom Render Textures, otherwise it won't work. If you have a doubt you can create a new shader by pressing the button ""New Shader"".

The node will automatically reflect the shader properties as inputs that you'll be able to connect to other nodes.
This can be especially useful to prototype a new node or just add something that wasn't in the node Library.

For more information, you can check the [Shader Nodes](../ShaderNodes.md) documentation page.
")]

	[System.Serializable, NodeMenuItem("Shader")]
	public class ShaderNode : MixtureNode, IUseCustomRenderTextureProcessing, ICreateNodeFrom<Shader>, ICreateNodeFrom<Material>
	{
		[Serializable]
		public struct ShaderProperty
		{
			public string			displayName;
			public string			referenceName;
			public string			tooltip;
			public SerializableType	type;
		}

		public static readonly string	DefaultShaderName = "Hidden/Mixture/ShaderNodeDefault";

		public override string	name => (shader != null) ? shader.name.Split('/')?.Last() : "Shader";
        public override bool	isRenamable => true;

		[Input(name = "In")]
		public List< object >		materialInputs;

		[Output(name = "Out"), Tooltip("Output Texture")]
		public CustomRenderTexture	output = null;

		[HideInInspector]
		public Shader			shader;
		[HideInInspector]
		public Material			material;

		// We keep internally a list of ports generated from the material exposed properties so when
		// there is an error in the shader or we can't import it, the connections still remains on the node.
		[SerializeField]
		List<ShaderProperty>	exposedProperties = new List<ShaderProperty>();
		// We also keep the GUID of the shader, in case of the script is imported before the shader so we can load it afterwards.
		[SerializeField]
		string					shaderGUID;

		protected virtual bool hasMips => false;

        protected virtual IEnumerable<string> filteredOutProperties => Enumerable.Empty<string>();
		public override Texture previewTexture => output;

		internal IEnumerable<string> GetFilterOutProperties() => filteredOutProperties;

		internal override float processingTimeInMillis
		{
			get
			{
				var sampler = CustomTextureManager.GetCustomTextureProfilingSampler(output);
				if (sampler != null)
					return sampler.GetRecorder().gpuElapsedNanoseconds / 1000000.0f;
				return 0;
			}
		}

		protected override MixtureSettings defaultSettings
		{
			get
			{
                var settings = base.defaultSettings;
                settings.editFlags = EditFlags.All ^ EditFlags.POTSize;
                return settings;
			}
		}

		Shader					defaultShader;

		protected override void Enable()
		{
            base.Enable();
			defaultShader = Shader.Find(DefaultShaderName);

			if (material == null)
			{
				material = new Material(shader != null ? shader : defaultShader);
				material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			beforeProcessSetup += BeforeProcessSetup;

			UpdateShader();
			UpdateExposedProperties();
			UpdateTempRenderTexture(ref output, hasMips: hasMips);
			output.material = material;

			// Update temp RT after process in case RTSettings have been modified in Process()
			afterProcessCleanup += () => {
				UpdateTempRenderTexture(ref output, hasMips: hasMips);
			};
		}

        protected override void Disable()
		{
			base.Disable();
			CoreUtils.Destroy(output);
		}

		public bool InitializeNodeFromObject(Shader value)
		{
			shader = value;
			return true;
		}

		public bool InitializeNodeFromObject(Material value)
		{
			shader = value.shader;
			material = value;
			return true;
		}

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		[CustomPortBehavior(nameof(materialInputs))]
		public IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		{
			if (exposedProperties.Count == 0)
			{
				UpdateShader();
				ValidateShader();
				UpdateExposedProperties();
			}
			else
				UpdateExposedProperties();

			foreach (var p in exposedProperties)
			{
				if (filteredOutProperties.Contains(p.referenceName))
					continue;

				yield return new PortData{
					displayName = p.displayName,
					identifier = p.referenceName,
					tooltip = p.tooltip,
					displayType = p.type.type,
				};
			}
		}

		[CustomPortInput(nameof(materialInputs), typeof(object))]
		protected void GetMaterialInputs(List< SerializableEdge > edges)
		{
			if (material.shader == null)
				UpdateShader();

			if (material.shader != null)
				AssignMaterialPropertiesFromEdges(edges, material);
		}

		// By overriding this function, we mark this node as dependent of the graph, so it will be update
		// so it will be updated when the graph dimension changes (the ports will be correct when we open the create from edge menu)
		[IsCompatibleWithGraph]
	protected static bool IsCompatibleWithGraph(BaseGraph graph) => true;

		void UpdateShader()
		{
#if UNITY_EDITOR
			bool updateGUID = false;

			if (shader == null && !String.IsNullOrEmpty(shaderGUID))
			{
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(shaderGUID);
				if (!String.IsNullOrEmpty(path))
					shader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(path);
			}

			if (shader != null && material.shader != shader)
			{
				material.shader = shader;
				updateGUID = true;
			}

			if (shader != null && (updateGUID || String.IsNullOrEmpty(shaderGUID)))
				UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(shader, out shaderGUID, out long _);
#endif
		}

		void BeforeProcessSetup()
		{
			UpdateShader();
			UpdateTempRenderTexture(ref output, hasMips: hasMips);
		}

		public override bool canProcess => ValidateShader();

		internal bool ValidateShader()
		{
			ClearMessages();

			if (material == null || material.shader == null)
			{
				AddMessage("missing material/shader", NodeMessageType.Error);
				return false;
			}

#if UNITY_EDITOR // IsShaderCompiled is editor only
			if (!IsShaderCompiled(material.shader))
			{
				if (output != null)
					output.material = null;
				foreach (var m in UnityEditor.ShaderUtil.GetShaderMessages(material.shader).Where(m => m.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))
				{
					string file = String.IsNullOrEmpty(m.file) ? material.shader.name : m.file;
					AddMessage($"{file}:{m.line} {m.message}", NodeMessageType.Error);
				}
				return false;
			}
#endif
			return true;
		}

		internal void UpdateExposedProperties()
		{
			if (shader == null || material.shader == null || shader == defaultShader)
				return;

#if UNITY_EDITOR // IsShaderCompiled is editor only
			if (!IsShaderCompiled(material.shader))
				return;
#endif

			exposedProperties.Clear();
			var ports = GetMaterialPortDatas(material);
			foreach (var port in ports)
			{
				exposedProperties.Add(new ShaderProperty{
					displayName = port.displayName,
					referenceName = port.identifier,
					type = new SerializableType(port.displayType),
					tooltip = port.tooltip,
				});
			}
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (output == null)
				return false;

			var outputDimension = settings.GetResolvedTextureDimension(graph);
			MixtureUtils.SetupDimensionKeyword(material, outputDimension);

			var s = material.shader;
			for (int i = 0; i < s.GetPropertyCount(); i++)
			{
				if (s.GetPropertyType(i) != ShaderPropertyType.Texture)
					continue;

				int id = s.GetPropertyNameId(i);
				if (material.GetTexture(id) != null)
					continue; // Avoid overriding existing textures

				var dim = s.GetPropertyTextureDimension(i);
				if (dim == TextureDimension.Tex2D)
					continue; // Texture2D don't need this feature

				// default texture names doesn't work with cubemap and 3D textures so we do it ourselves...
				switch (s.GetPropertyTextureDefaultName(i))
				{
					case "black":
						material.SetTexture(id, TextureUtils.GetBlackTexture(dim));
						break;
					case "white":
						material.SetTexture(id, TextureUtils.GetWhiteTexture(dim));
						break;
					// TODO: grey and bump
				}
			}

			output.material = material;

            bool useCustomUV = material.HasTextureBound("_UV", settings.GetResolvedTextureDimension(graph));
            material.SetKeywordEnabled("USE_CUSTOM_UV", useCustomUV);

			return true;
		}

        public virtual IEnumerable<CustomRenderTexture> GetCustomRenderTextures()
		{
			yield return output;
		}
    }
}