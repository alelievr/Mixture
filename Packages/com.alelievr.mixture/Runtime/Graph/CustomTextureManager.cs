#if DEVELOPMENT_BUILD || UNITY_EDITOR
    #define CUSTOM_TEXTURE_PROFILING
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System;
using UnityEngine.Profiling;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public static class CustomTextureManager
{
    static CustomTextureManager() => SetupManager();

    public static List<CustomRenderTexture> customRenderTextures {get; private set;} = new List<CustomRenderTexture>();
    static List<CustomRenderTexture> sortedCustomRenderTextures = new List<CustomRenderTexture>();

    static HashSet<CustomRenderTexture> needsInitialization = new HashSet<CustomRenderTexture>();
    static Dictionary<CustomRenderTexture, int> needsUpdate = new Dictionary<CustomRenderTexture, int>();

    static Dictionary<CustomRenderTexture, int> computeOrder = new Dictionary<CustomRenderTexture, int>();
    static Dictionary<CustomRenderTexture, CustomSampler> customRenderTextureSamplers = new Dictionary<CustomRenderTexture, CustomSampler>();

    public static event Action<CommandBuffer, CustomRenderTexture> onBeforeCustomTextureUpdated;

    [RuntimeInitializeOnLoadMethod]
    static void SetupManager()
    {
        CustomRenderTextureManager.textureLoaded -= OnCRTLoaded;
        CustomRenderTextureManager.textureLoaded += OnCRTLoaded;
        CustomRenderTextureManager.textureUnloaded -= OnCRTUnloaded;
        CustomRenderTextureManager.textureUnloaded += OnCRTUnloaded;

        CustomRenderTextureManager.updateTriggered += OnUpdateCalled;
        CustomRenderTextureManager.initializeTriggered += OnInitializeCalled;

#if UNITY_EDITOR
        // In the editor we might not always have a camera to update our custom render textures
        UnityEditor.EditorApplication.update -= UpdateCRTsEditor;
        UnityEditor.EditorApplication.update += UpdateCRTsEditor;
#else
        RenderPipelineManager.beginFrameRendering -= UpdateCRTsRuntime;
        RenderPipelineManager.beginFrameRendering += UpdateCRTsRuntime;
#endif

        GraphicsSettings.disableBuiltinCustomRenderTextureUpdate = true;
        UpdateSRPCustomRenderTextureStatus();
    }

    static void UpdateCRTsEditor()
    {
        if (!GraphicsSettings.disableBuiltinCustomRenderTextureUpdate)
            return;

        UpdateDependencies();

        Graphics.ExecuteCommandBuffer(MakeCRTCommandBuffer());
    }

    static void UpdateCRTsRuntime(ScriptableRenderContext context, Camera[] cameras)
    {
        if (!GraphicsSettings.disableBuiltinCustomRenderTextureUpdate)
            return;
        
        UpdateDependencies();

        context.ExecuteCommandBuffer(MakeCRTCommandBuffer());
    }

    static CommandBuffer MakeCRTCommandBuffer()
    {
        var cmd = new CommandBuffer{ name = "SRP Custom Render Texture" };
        foreach (var crt in sortedCustomRenderTextures)
            UpdateCustomRenderTexture(cmd, crt);
        return cmd;
    }

    public static void ForceUpdateNow()
    {
        UpdateDependencies();
        Graphics.ExecuteCommandBuffer(MakeCRTCommandBuffer());
    }

    /// <summary>
    /// Add a CRT that is not yet tracked by the manager because of the frame of delay.
    /// </summary>
    /// <param name="crt"></param>
    public static void RegisterNewCustomRenderTexture(CustomRenderTexture crt)
    {
        OnCRTLoaded(crt);
    }

    static void UpdateSRPCustomRenderTextureStatus()
    {
        if (!GraphicsSettings.disableBuiltinCustomRenderTextureUpdate)
        {
            // SRP custom textures have been disabled so we clear our list
            customRenderTextures.Clear();
        }
        else
        {
            // Gather the list of all running custom render textures and call the loaded callback
            CustomRenderTextureManager.GetAllCustomRenderTextures(customRenderTextures);
            
            foreach (var crt in customRenderTextures)
                InitializeCustomRenderTexture(crt);
        }
    }

    // CustomRenderTexture.Update have been called by the user
    static void OnUpdateCalled(CustomRenderTexture crt, int count)
    {
        if (needsUpdate.ContainsKey(crt))
            needsUpdate[crt] += count;
        else
            needsUpdate[crt] = count;
    }

    // CustomRenderTexture.Initialize have been called by the user
    static void OnInitializeCalled(CustomRenderTexture crt) => needsInitialization.Add(crt);

    static void OnCRTLoaded(CustomRenderTexture crt)
    {
        if (!customRenderTextures.Contains(crt))
        {
            customRenderTextures.Add(crt);
            InitializeCustomRenderTexture(crt);
        }
    }

    static void InitializeCustomRenderTexture(CustomRenderTexture crt)
    {
        // Debug.Log("Load: " + crt + " | " + crt.updateMode);
    }

    static void OnCRTUnloaded(CustomRenderTexture crt) => customRenderTextures.Remove(crt);

    static void UpdateDependencies()
    {
        computeOrder.Clear();
        sortedCustomRenderTextures.Clear();

        foreach (var crt in customRenderTextures)
            UpdateComputeOrder(crt, 0);

        sortedCustomRenderTextures = customRenderTextures.Where(c => computeOrder.ContainsKey(c) && computeOrder[c] != -1).ToList();
        sortedCustomRenderTextures.Sort((c1, c2) => {
            if (!computeOrder.TryGetValue(c1, out int i1))
                i1 = -1;
            if (!computeOrder.TryGetValue(c2, out int i2))
                i2 = -1;

            return i1.CompareTo(i2);
        });
    }

    static int UpdateComputeOrder(CustomRenderTexture crt, int depth)
    {
        int crtComputeOrder = 0;

        if (depth > 500)
        {
            Debug.LogError("Recursion error while updating compute order");
            return -1;
        }

        if (computeOrder.TryGetValue(crt, out crtComputeOrder))
            return crtComputeOrder;

        if (!IsValid(crt))
            return -1;

        foreach(var texID in crt.material.GetTexturePropertyNameIDs())
        {
            if (crt.material.GetTexture(texID) is CustomRenderTexture dep)
            {
                int c = UpdateComputeOrder(dep, depth + 1);

                if (c == -1)
                {
                    crtComputeOrder = -1;
                    break;
                }

                crtComputeOrder += c;
            }
        }

        if (crtComputeOrder != -1)
            crtComputeOrder++;
        
        computeOrder[crt] = crtComputeOrder;

        return crtComputeOrder;
    }

    static bool IsValid(CustomRenderTexture crt)
    {
        if (crt.material == null || crt.material.shader == null)
            return false;
        
        if (crt.material.passCount == 0)
            return false;

#if UNITY_EDITOR
        // If the shader have errors
        var compilationMessages = UnityEditor.ShaderUtil.GetShaderMessages(crt.material.shader);
        if (compilationMessages.Any(m => m.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))
            return false;
#endif

        return true;
    }

    static int kUpdateDataCenters              = Shader.PropertyToID("CustomRenderTextureCenters");
    static int kUpdateDataSizesAndRotation     = Shader.PropertyToID("CustomRenderTextureSizesAndRotations");
    static int kUpdateDataPrimitiveIDs         = Shader.PropertyToID("CustomRenderTexturePrimitiveIDs");
    static int kCustomRenderTextureParameters  = Shader.PropertyToID("CustomRenderTextureParameters");
    static int kCustomRenderTextureInfo        = Shader.PropertyToID("_CustomRenderTextureInfo");
    static int kSelf2D                         = Shader.PropertyToID("_SelfTexture2D");
    static int kSelf3D                         = Shader.PropertyToID("_SelfTexture3D");
    static int kSelfCube                       = Shader.PropertyToID("_SelfTextureCube");

    // Returns user facing texture info
    static Vector4 GetTextureInfos(CustomRenderTexture crt, int sliceIndex)
        => new Vector4((float)crt.width, (float)crt.height, crt.volumeDepth, (float)sliceIndex);

    // Returns internal parameters for rendering
    static Vector4 GetTextureParameters(CustomRenderTexture crt, int sliceIndex)
    {
        int depth = crt.dimension == TextureDimension.Cube ? 6 : crt.volumeDepth;
        return new Vector4(
            (crt.updateZoneSpace == CustomRenderTextureUpdateZoneSpace.Pixel) ? 1.0f : 0.0f,
            // Important: textureparam.y is used for the z coordinate in the CRT and in case of 2D, we use 0.5 because most of the 3D compatible effects will use a neutral value 0.5
            crt.dimension == TextureDimension.Tex2D ? 0.5f : (float)sliceIndex / depth,
            // 0 => 2D, 1 => 3D, 2 => Cube
            crt.dimension == TextureDimension.Tex3D ? 1.0f : (crt.dimension == TextureDimension.Cube ? 2.0f : 0.0f),
            0.0f
            );
    }

    // Update one custom render texture.
    public static void UpdateCustomRenderTexture(CommandBuffer cmd, CustomRenderTexture crt)
    {
        bool firstPass = crt.updateCount == 0;

        // Handle initialization here too:
        if (crt.initializationMode == CustomRenderTextureUpdateMode.Realtime || needsInitialization.Contains(crt) || (firstPass && crt.initializationMode == CustomRenderTextureUpdateMode.OnLoad))
        {
            switch (crt.initializationSource)
            {
                case CustomRenderTextureInitializationSource.Material:
                    // TODO
                    break;
                case CustomRenderTextureInitializationSource.TextureAndColor:
                    // TODO
                    break;
            }
            needsInitialization.Remove(crt);
        }

        needsUpdate.TryGetValue(crt, out int updateCount);

        if (crt.material != null && (crt.updateMode == CustomRenderTextureUpdateMode.Realtime || updateCount > 0 || (firstPass && crt.updateMode == CustomRenderTextureUpdateMode.OnLoad)))
        {
            onBeforeCustomTextureUpdated?.Invoke(cmd, crt);

#if CUSTOM_TEXTURE_PROFILING
            customRenderTextureSamplers.TryGetValue(crt, out var sampler);
            if (sampler == null)
            {
                sampler = customRenderTextureSamplers[crt] = CustomSampler.Create($"{crt.name} - {crt.GetInstanceID()}", true);
                sampler.GetRecorder().enabled = true;
            }
            cmd.BeginSample(sampler);
#endif

            using (new ProfilingScope(cmd, new ProfilingSampler($"Update {crt.name}")))
            {
                // Prepare "self" texture for reading in the shader for double buffered custom textures
                RenderTexture textureSelf2D = null;
                RenderTexture textureSelf3D = null;
                RenderTexture textureSelfCube = null;
                if (crt.doubleBuffered)
                {
                    if (crt.dimension == TextureDimension.Tex2D)
                        textureSelf2D = crt;
                    if (crt.dimension == TextureDimension.Cube)
                        textureSelfCube = crt;
                    if (crt.dimension == TextureDimension.Tex3D)
                        textureSelf3D = crt;
                }

                if (crt.doubleBuffered)
                {
                    // Update the internal double buffered render texture (resize / alloc / ect.)
                    crt.EnsureDoubleBufferConsistency();
                }

                MaterialPropertyBlock block = new MaterialPropertyBlock();

                // If the user didn't called the update on CRT, we still process it because it's realtime
                updateCount = Mathf.Max(updateCount, 1);
                for (int i = 0; i < updateCount; i++)
                {
                    int sliceCount = GetSliceCount(crt);
                    for (int slice = 0; slice < sliceCount; slice++)
                    {
                        RenderTexture renderTexture = crt.doubleBuffered ? crt.GetDoubleBufferRenderTexture() : crt;
                        cmd.SetRenderTarget(renderTexture, 0, (crt.dimension == TextureDimension.Cube) ? (CubemapFace)slice : 0, (crt.dimension == TextureDimension.Tex3D) ? slice : 0);
                        cmd.SetViewport(new Rect(0, 0, crt.width, crt.height));
                        block.SetVector(kCustomRenderTextureInfo, GetTextureInfos(crt, slice));
                        block.SetVector(kCustomRenderTextureParameters, GetTextureParameters(crt, slice));
                        if (textureSelf2D != null)
                            block.SetTexture(kSelf2D, textureSelf2D);
                        if (textureSelf3D != null)
                            block.SetTexture(kSelf3D, textureSelf3D);
                        if (textureSelfCube != null)
                            block.SetTexture(kSelfCube, textureSelfCube);

                        List<CustomRenderTextureUpdateZone> updateZones = new List<CustomRenderTextureUpdateZone>();
                        crt.GetUpdateZones(updateZones);

                        if (updateZones.Count == 0)
                            updateZones.Add(new CustomRenderTextureUpdateZone{ needSwap = false, updateZoneCenter = new Vector3(0.5f, 0.5f, 0.5f), updateZoneSize = Vector3.one, rotation = 0, passIndex = 0});

                        var zoneCenters = updateZones.Select(z => new Vector4(z.updateZoneCenter.x, z.updateZoneCenter.y, z.updateZoneCenter.z, 0)).ToList();
                        var zoneSizesAndRotation = updateZones.Select(z => new Vector4(z.updateZoneSize.x, z.updateZoneSize.y, z.updateZoneSize.z, z.rotation)).ToList();
                        var zonePrimitiveIDs = Enumerable.Range(0, updateZones.Count).Select(j => (float)j).ToList();// updateZones.Select(z => 0.0f).ToList();

                        bool firstUpdate = true;
                        foreach (var zone in updateZones)
                        {
                            if (zone.needSwap && !firstUpdate)
                            {
                                var doubleBuffer = crt.GetDoubleBufferRenderTexture();
                                if (doubleBuffer != null)
                                {
                                    // For now, it's just a copy, once we actually do the swap of pointer, be careful to reset the Active Render Texture
                                    cmd.CopyTexture(doubleBuffer, slice, crt, slice);
                                    cmd.SetRenderTarget(doubleBuffer, 0, (crt.dimension == TextureDimension.Cube) ? (CubemapFace)slice : 0, slice);
                                }
                            }

                            int passIndex = zone.passIndex == -1 ? 0 : zone.passIndex;

                            block.SetVectorArray(kUpdateDataCenters, zoneCenters);
                            block.SetVectorArray(kUpdateDataSizesAndRotation, zoneSizesAndRotation);
                            block.SetFloatArray(kUpdateDataPrimitiveIDs, zonePrimitiveIDs);

                            cmd.DrawProcedural(Matrix4x4.identity, crt.material, passIndex, MeshTopology.Triangles, 6 * updateZones.Count, 1, block);

                            firstUpdate = false;
                        }
                    }
                }

                needsUpdate.Remove(crt);
            }

#if CUSTOM_TEXTURE_PROFILING
            cmd.EndSample(sampler);
#endif
            crt.IncrementUpdateCount();
        }
    }

    public static CustomSampler GetCustomTextureProfilingSampler(CustomRenderTexture crt)
    {
        customRenderTextureSamplers.TryGetValue(crt, out var sampler);
        return sampler;
    }

    static int GetSliceCount(CustomRenderTexture crt)
    {
        switch (crt.dimension)
        {
            case TextureDimension.Cube:
                return 6;
            case TextureDimension.Tex3D:
                return crt.volumeDepth;
            default:
            case TextureDimension.Tex2D:
                return 1;
        }
    }
}
