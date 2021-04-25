using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
Renders the Unity particle system (shuriken) inside a texture.
More information here: https://docs.unity3d.com/ScriptReference/ParticleSystem.html
")]

	[System.Serializable, NodeMenuItem("Utils/Particles (Experimental)")]
	public class ParticlesNode : BasePrefabNode 
	{
        [Tooltip("Rendered particle system")]
		[Output]
        [System.NonSerialized]
        public Texture outputTexture;

        [ShowInInspector]
        public ClearFlag clearMode = ClearFlag.All;

		public override string	name => "Particles (Experimental)";
		public override Texture	previewTexture => outputTexture;
        public override bool isRenamable => true;

        protected override string defaultPrefabName => "Particles Node";

        // We don't use the 'Custom' part of the render texture but function are taking this type in parameter
        internal CustomRenderTexture     tmpRenderTexture;

        ParticleSystemRenderer renderer;
        ParticleSystem system;
        Mesh mesh;

        internal GameObject instance;

#if UNITY_EDITOR
        protected override GameObject LoadDefaultPrefab()
            => Resources.Load<GameObject>("Particles Node Prefab");
#endif

        protected override void Enable()
        {
            base.Enable();
            UpdateTempRenderTexture(ref tmpRenderTexture);

#if UNITY_EDITOR
            instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
#else
            instance = GameObject.Instantiate(prefab);
#endif
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.name = "Particles";
            renderer = instance.GetComponent<ParticleSystemRenderer>();
            system = instance.GetComponent<ParticleSystem>();
            mesh = new Mesh{ indexFormat = IndexFormat.UInt32, hideFlags = HideFlags.HideAndDontSave };

            PlayParticleSystem();
        }

        public void PlayParticleSystem()
        {
            // Disable the renderer to hide the effect in the scene (the GameObject is instanciated inside the scene after all)
            renderer.enabled = false; 
            instance.hideFlags = HideFlags.HideAndDontSave;
            system.Play();
        }

        protected override void Disable()
        {
            base.Disable();
            CoreUtils.Destroy(tmpRenderTexture);
            if (renderer != null && renderer.gameObject != null)
                CoreUtils.Destroy(renderer.gameObject);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || system == null)
                return false;

            UpdateTempRenderTexture(ref tmpRenderTexture);

            if (!system.isPlaying)
                PlayParticleSystem();

            system.Simulate(Time.deltaTime, true, false, true);

            // TODO: depth buffer support
            cmd.SetRenderTarget(tmpRenderTexture);
            cmd.SetViewport(new Rect(0, 0, tmpRenderTexture.width, tmpRenderTexture.height));
            cmd.ClearRenderTarget((clearMode & ClearFlag.Depth) != 0, (clearMode & ClearFlag.Color) != 0, new Color(0, 0, 0, 1), 1.0f);
            var material = renderer.sharedMaterial;

            int passIndex = -1;
            // Try to find the best pass index to render the particles:
            if (passIndex == -1)
                passIndex = renderer.sharedMaterial.FindPass("Forward");
            if (passIndex == -1)
                passIndex = renderer.sharedMaterial.FindPass("ForwardOnly");
            if (passIndex == -1)
                passIndex = 4; // by default the Particle Unlit color pass is the 4th

            mesh.Clear();
            renderer.BakeMesh(mesh, true);
            cmd.SetViewMatrix(Matrix4x4.identity); // We can't apply any rotation otherwise the particle billboard won't follow :/ 
            cmd.SetProjectionMatrix(Matrix4x4.Ortho(-1, 1, -1, 1, -1, 1));
            cmd.DrawMesh(mesh, Matrix4x4.identity, renderer.sharedMaterial, 0, passIndex);

            outputTexture = tmpRenderTexture;

            return true;
        }
	}
}