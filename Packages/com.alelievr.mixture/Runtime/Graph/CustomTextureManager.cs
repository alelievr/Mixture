using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public static class CustomTextureManager
{
    static CustomTextureManager() => SetupManager();

    public static List<CustomRenderTexture> customRenderTextures = new List<CustomRenderTexture>();
    static List<CustomRenderTexture> sortedCustomRenderTextures = new List<CustomRenderTexture>();

    static bool builtinCustomRenderTextureEnabled = true;

    [RuntimeInitializeOnLoadMethod]
    static void SetupManager()
    {
        CustomRenderTextureManager.onTextureLoaded -= OnCRTLoaded;
        CustomRenderTextureManager.onTextureLoaded += OnCRTLoaded;
        CustomRenderTextureManager.onTextureUnloaded -= OnCRTUnloaded;
        CustomRenderTextureManager.onTextureUnloaded += OnCRTUnloaded;

#if UNITY_EDITOR
        // In the editor we might not always have a camera to update our custom render textures
        UnityEditor.EditorApplication.update -= UpdateCRTsEditor;
        UnityEditor.EditorApplication.update += UpdateCRTsEditor;
#else
        RenderPipelineManager.beginFrameRendering -= UpdateCRTsRuntime;
        RenderPipelineManager.beginFrameRendering += UpdateCRTsRuntime;
#endif

        GraphicsSettings.useBuiltinCustomRenderTexture = false;
        UpdateSRPCustomRenderTextureStatus();
    }

    static void UpdateCRTsEditor()
    {
        if (GraphicsSettings.useBuiltinCustomRenderTexture)
            return;
        
        UpdateDependencies();

        Graphics.ExecuteCommandBuffer(MakeCRTCommandBuffer());
    }

    static void UpdateCRTsRuntime(ScriptableRenderContext context, Camera[] cameras)
    {
        if (GraphicsSettings.useBuiltinCustomRenderTexture)
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

    static void UpdateSRPCustomRenderTextureStatus()
    {
        if (GraphicsSettings.useBuiltinCustomRenderTexture)
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

        builtinCustomRenderTextureEnabled = GraphicsSettings.useBuiltinCustomRenderTexture;
    }

    static void OnCRTLoaded(CustomRenderTexture crt)
    {
        if (!customRenderTextures.Contains(crt))
        {
            customRenderTextures.Add(crt);
            InitializeCustomRenderTexture(crt);
        }
    }

    static void InitializeCustomRenderTexture(CustomRenderTexture crt) => Debug.Log("Load: " + crt.name);

    static void OnCRTUnloaded(CustomRenderTexture crt)
    {
        customRenderTextures.Remove(crt);
        Debug.Log("Unload: " + crt.name);
    }

    static void UpdateDependencies()
    {
        // temp code: no sorting
        sortedCustomRenderTextures = customRenderTextures;
        
        foreach (var crt in customRenderTextures)
        {
            if (crt.material == null)
                continue;

            foreach(var texID in crt.material.GetTexturePropertyNameIDs())
            {
                if (crt.material.GetTexture(texID) is CustomRenderTexture)
                {
                    // TODO
                }
            }
        }
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
            (float)sliceIndex / depth,
            crt.dimension == TextureDimension.Tex3D ? 1.0f : 0.0f,
            0.0f
            );
    }

    static void UpdateCustomRenderTexture(CommandBuffer cmd, CustomRenderTexture crt)
    {
        if (crt.material != null)
        {
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
                    crt.CheckDoubleBufferConsistentcy();
                }

                MaterialPropertyBlock block = new MaterialPropertyBlock();

                int sliceCount = (crt.dimension == TextureDimension.Cube) ? 6 : crt.volumeDepth;
                for (int slice = 0; slice < sliceCount; slice++)
                {
                    RenderTexture renderTexture = crt.doubleBuffered ? crt.GetDoubleBufferRenderTexture() : crt;
                    cmd.SetRenderTarget(renderTexture, 0, (crt.dimension == TextureDimension.Cube) ? (CubemapFace)slice : 0,  (crt.dimension == TextureDimension.Tex3D) ? slice : 0);
                    cmd.SetViewport(new Rect(0, 0, crt.width, crt.height));
                    // cmd.ClearRenderTarget(true, true, Color.red, 0); // debug
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
                    // TODO !
                    var zonePrimitiveIDs = updateZones.Select(z => 0.0f).ToList();

                    bool firstUpdate = true;
                    foreach (var zone in updateZones)
                    {
                        if (zone.needSwap && !firstUpdate)
                        {
                            var doubleBuffer = crt.GetDoubleBufferRenderTexture();
                            if (doubleBuffer != null)
                            {
                                // For now, it's just a copy, once we actually do the swap of pointer, be careful to reset the Active Render Texture
                                cmd.Blit(doubleBuffer, crt);
                                cmd.SetRenderTarget(doubleBuffer, 0, (crt.dimension == TextureDimension.Cube) ? (CubemapFace)slice : 0, slice);
                            }
                        }

                        int passIndex = zone.passIndex == -1 ? 0: zone.passIndex;

                        block.SetVectorArray(kUpdateDataCenters, zoneCenters);
                        block.SetVectorArray(kUpdateDataSizesAndRotation, zoneSizesAndRotation);
                        block.SetFloatArray(kUpdateDataPrimitiveIDs, zonePrimitiveIDs);

                        cmd.DrawProcedural(Matrix4x4.identity, crt.material, passIndex, MeshTopology.Triangles, 6 * updateZones.Count, 1, block);

                        firstUpdate = false;
                    }
                }
            }
            
            crt.IncrementUpdateCount();
        }
    }
}
