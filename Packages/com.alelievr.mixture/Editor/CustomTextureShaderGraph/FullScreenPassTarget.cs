#if MIXTURE_SHADERGRAPH
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEditor.Graphing;
using UnityEditor.UIElements;
using UnityEditor.ShaderGraph.Serialization;
using UnityEditor.ShaderGraph.Legacy;

namespace Mixture 
{
    [GenerateBlocks]
    internal struct FullScreenBlocks
    {
        public static BlockFieldDescriptor colorBlock = new BlockFieldDescriptor(String.Empty, "Color", "Color",
                new ColorRGBAControl(UnityEngine.Color.white), ShaderStage.Fragment);
    }

    sealed class FullScreenPassTarget : Target, ILegacyTarget
    {
        // Constants
        const string kAssetGuid = "a0bae34258e39cd4899b63278c24c086"; 

        // SubTarget
        List<SubTarget> m_SubTargets;
        List<string> m_SubTargetNames;
        int activeSubTargetIndex => m_SubTargets.IndexOf(m_ActiveSubTarget);

        // View
        PopupField<string> m_SubTargetField;
        TextField m_CustomGUIField;

        [SerializeField]
        JsonData<SubTarget> m_ActiveSubTarget;

        [SerializeField]
        string m_CustomEditorGUI;

        public FullScreenPassTarget()
        {
            displayName = "Mixture";
            isHidden = false;
            m_SubTargets = GetSubTargets(this);
            m_SubTargetNames = m_SubTargets.Select(x => x.displayName).ToList();
            ProcessSubTargetList(ref m_ActiveSubTarget, ref m_SubTargets);
        }

        public static void ProcessSubTargetList(ref JsonData<SubTarget> activeSubTarget, ref List<SubTarget> subTargets)
        {
            if(subTargets == null || subTargets.Count == 0)
                return;

            // assign the initial sub-target, if none is assigned yet
            if (activeSubTarget.value == null)
            {
                // this is a bit of a hack: prefer subtargets named "Lit" if they exist, otherwise default to the first one
                // in the future, we should make the default sub-target user configurable
                var litSubTarget = subTargets.FirstOrDefault(x => x.displayName == "Lit");
                if (litSubTarget != null)
                    activeSubTarget = litSubTarget;
                else
                    activeSubTarget = subTargets[0];
                return;
            }

            // Update SubTarget list with active SubTarget
            var activeSubTargetType = activeSubTarget.value.GetType();
            var activeSubTargetCurrent = subTargets.FirstOrDefault(x => x.GetType() == activeSubTargetType);
            var index = subTargets.IndexOf(activeSubTargetCurrent);
            subTargets[index] = activeSubTarget;
        }

        public static List<SubTarget> GetSubTargets<T>(T target) where T : Target
        {
            // Get Variants
            var subTargets = ListPool<SubTarget>.Get();
            var typeCollection = TypeCache.GetTypesDerivedFrom<SubTarget>();
            foreach (var type in typeCollection)
            {
                if(type.IsAbstract || !type.IsClass)
                    continue;

                var subTarget = (SubTarget)Activator.CreateInstance(type);
                if(!subTarget.isHidden && subTarget.targetType.Equals(typeof(T)))
                {
                    subTarget.target = target;
                    subTargets.Add(subTarget);
                }
            }

            return subTargets;
        }

        // TODO: remove?
        public SubTarget activeSubTarget
        {
            get => m_ActiveSubTarget;
            set => m_ActiveSubTarget = value;
        }

        public string customEditorGUI
        {
            get => m_CustomEditorGUI;
            set => m_CustomEditorGUI = value;
        }

        public override bool IsActive() => activeSubTarget.IsActive();
        public override bool IsNodeAllowedByTarget(Type nodeType) => true;

        public override void Setup(ref TargetSetupContext context)
        {
            // Setup the Target
            context.AddAssetDependency(new GUID(kAssetGuid), AssetCollection.Flags.SourceDependency);

            // Setup the active SubTarget
            ProcessSubTargetList(ref m_ActiveSubTarget, ref m_SubTargets);
            m_ActiveSubTarget.value.target = this;
            m_ActiveSubTarget.value.Setup(ref context);

            // Override EditorGUI
            if(!string.IsNullOrEmpty(m_CustomEditorGUI))
            {
                context.SetDefaultShaderGUI(m_CustomEditorGUI);
            }
        }

        public override void GetFields(ref TargetFieldContext context)
        {
            var descs = context.blocks.Select(x => x.descriptor);
            // Core fields
            context.AddField(Fields.GraphVertex,            descs.Contains(BlockFields.VertexDescription.Position) ||
                                                            descs.Contains(BlockFields.VertexDescription.Normal) ||
                                                            descs.Contains(BlockFields.VertexDescription.Tangent));
            context.AddField(Fields.GraphPixel);

            // SubTarget fields
            m_ActiveSubTarget.value.GetFields(ref context);
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            // SubTarget blocks
            m_ActiveSubTarget.value.GetActiveBlocks(ref context);
        }

        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<String> registerUndo)
        {
            // Core properties
            m_SubTargetField = new PopupField<string>(m_SubTargetNames, activeSubTargetIndex);
            context.AddProperty("Material", m_SubTargetField, (evt) =>
            {
                if (Equals(activeSubTargetIndex, m_SubTargetField.index))
                    return;

                registerUndo("Change Material");
                m_ActiveSubTarget = m_SubTargets[m_SubTargetField.index];
                onChange();
            });

            // SubTarget properties
            m_ActiveSubTarget.value.GetPropertiesGUI(ref context, onChange, registerUndo);

            // Custom Editor GUI
            // Requires FocusOutEvent
            m_CustomGUIField = new TextField("") { value = customEditorGUI };
            m_CustomGUIField.RegisterCallback<FocusOutEvent>(s =>
            {
                if (Equals(customEditorGUI, m_CustomGUIField.value))
                    return;

                registerUndo("Change Custom Editor GUI");
                customEditorGUI = m_CustomGUIField.value;
                onChange();
            });
            context.AddProperty("Custom Editor GUI", m_CustomGUIField, (evt) => {});
        }

        public bool TrySetActiveSubTarget(Type subTargetType)
        {
            if(!subTargetType.IsSubclassOf(typeof(SubTarget)))
                return false;

            foreach(var subTarget in m_SubTargets)
            {
                if(subTarget.GetType().Equals(subTargetType))
                {
                    m_ActiveSubTarget = subTarget;
                    return true;
                }
            }

            return false;
        }

        public bool TryUpgradeFromMasterNode(IMasterNode1 masterNode, out Dictionary<BlockFieldDescriptor, int> blockMap)
        {
            // Upgrade SubTarget
            foreach(var subTarget in m_SubTargets)
            {
                if(!(subTarget is ILegacyTarget legacySubTarget))
                    continue;

                if(legacySubTarget.TryUpgradeFromMasterNode(masterNode, out blockMap))
                {
                    m_ActiveSubTarget = subTarget;
                    return true;
                }
            }

            blockMap = null;
            return false;
        }

        public override bool WorksWithSRP(RenderPipelineAsset scriptableRenderPipeline) => true; 
    }

    static class FullscreePasses
    {
        public static PassDescriptor CustomRenderTexture = new PassDescriptor
        {
            // Definition
            referenceName = "SHADERPASS_CRT",
            useInPreview = true,

            // Template
            passTemplatePath = AssetDatabase.GUIDToAssetPath("afa536a0de48246de92194c9e987b0b8"),

            // Port Mask
            validVertexBlocks = new BlockFieldDescriptor[]
            {
                    BlockFields.VertexDescription.Position,
                    BlockFields.VertexDescription.Normal,
                    BlockFields.VertexDescription.Tangent,
            },
            validPixelBlocks = new BlockFieldDescriptor[]
            {
                    FullScreenBlocks.colorBlock,
                    BlockFields.SurfaceDescription.BaseColor,
            },

            // Fields
            structs = new StructCollection
            {
                { Structs.Attributes },
                { Structs.SurfaceDescriptionInputs },
                { Structs.VertexDescriptionInputs },
            },
            requiredFields = new FieldCollection()
            {
                StructFields.Attributes.color,
                StructFields.Attributes.uv0,
                StructFields.Varyings.color,
                StructFields.Varyings.texCoord0,

            },
            fieldDependencies = new DependencyCollection()
            {
                { FieldDependencies.Default },
            },
        };

        public static PassDescriptor MipMap = new PassDescriptor
        {
            // Definition
            referenceName = "SHADERPASS_MIPMAP",
            useInPreview = true,

            // Template
            passTemplatePath = AssetDatabase.GUIDToAssetPath("e2bd3f2f69902b64c8cd77ba7948c1b9"),

            // Port Mask
            validVertexBlocks = new BlockFieldDescriptor[]
            {
                    BlockFields.VertexDescription.Position,
                    BlockFields.VertexDescription.Normal,
                    BlockFields.VertexDescription.Tangent,
            },
            validPixelBlocks = new BlockFieldDescriptor[]
            {
                    FullScreenBlocks.colorBlock,
                    BlockFields.SurfaceDescription.BaseColor,
            },

            // Fields
            structs = new StructCollection
            {
                { Structs.Attributes },
                { Structs.SurfaceDescriptionInputs },
                { Structs.VertexDescriptionInputs },
            },
            requiredFields = new FieldCollection()
            {
                StructFields.Attributes.color,
                StructFields.Attributes.uv0,
                StructFields.Varyings.color,
                StructFields.Varyings.texCoord0,

            },
            fieldDependencies = new DependencyCollection()
            {
                { FieldDependencies.Default },
            },
        };

    }
}
#endif
