using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
#if MIXTURE_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
#if MIXTURE_URP
using UnityEngine.Rendering.Universal;
#endif

namespace Mixture
{
    // This is to handle normal output and maybe more
    [ExecuteAlways, AddComponentMenu("")]
    public class MixtureBufferOutput : MonoBehaviour
    {
#if MIXTURE_HDRP
        CustomPassVolume    volume;
        BufferOutputPass    bufferPass;
#endif

        void Awake()
        {
            UpdateSceneComponents();
        }

        public virtual void SetOutputSettings(PrefabCaptureNode.OutputMode mode, Camera targetCamera)
        {
            UpdateSceneComponents();

            var pipeline = RenderPipelineManager.currentPipeline;

            if (pipeline != null) // SRP in use, so we'll use an SRP specific implementation
            {
#if MIXTURE_HDRP
                if (pipeline.GetType() == typeof(HDRenderPipeline))
                {
                    bufferPass.SetOutputSettings(mode, targetCamera);
                }
#endif
            }
            else
            {
                // TODO: legacy support with replacement shaders
            }
        }

        void UpdateSceneComponents()
        {
#if MIXTURE_HDRP
            if (volume == null)
            {
                volume = gameObject.GetComponent<CustomPassVolume>();
                if (volume == null)
                    volume = gameObject.AddComponent<CustomPassVolume>();
                volume.hideFlags = HideFlags.HideAndDontSave;
                volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
            }
            if (bufferPass == null)
            {
                bufferPass = volume.customPasses.FirstOrDefault(c => c is BufferOutputPass) as BufferOutputPass;
                if (bufferPass == null)
                {
                    bufferPass = new BufferOutputPass{ name = "Mixture Buffer Output"};
                    volume.customPasses.Add(bufferPass);
                }
            }
#endif
        }

        void OnDestroy()
        {
#if MIXTURE_HDRP
            volume.enabled = false;
#endif
        }
    }
}