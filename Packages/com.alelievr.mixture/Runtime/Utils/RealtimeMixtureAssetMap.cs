using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mixture
{
    /// <summary>
    /// This asset is created at build time to reference all the realtime graphs
    /// needed in the final build. This is because we can't implement a proper
    /// dependency system between builtin texture and a ScriptableObject.
    /// </summary>
    [Serializable]
    public class RealtimeMixtureAssetMap : ScriptableObject
    {
        [Serializable]
        public struct RuntimeMixtureReferences
        {
            public CustomRenderTexture  asset;
            public MixtureGraph         graph;
        }

        public List<RuntimeMixtureReferences> realtimeReferences = new List<RuntimeMixtureReferences>();
    }
}