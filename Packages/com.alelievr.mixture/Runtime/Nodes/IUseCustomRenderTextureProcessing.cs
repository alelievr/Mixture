using UnityEngine;
using System.Collections.Generic;

namespace Mixture
{
    public interface IUseCustomRenderTextureProcessing
    {
        IEnumerable<CustomRenderTexture> GetCustomRenderTextures();
    }
}