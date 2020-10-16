using UnityEngine;
using System.Collections.Generic;

namespace Mixture
{
    public class MixtureDatabase : ScriptableObject
    {
        public static readonly string databaseResourcePath = "Mixture/Database";

        static MixtureDatabase instance;

        public List<MixtureGraph> realtimeGraphs = new List<MixtureGraph>();
        public List<MixtureGraph> staticGraphs = new List<MixtureGraph>();

        Dictionary<Texture, MixtureGraph> graphMap = new Dictionary<Texture, MixtureGraph>();

// The mixture database is only available in build. In the editor we have directly load the graph with AssetDatabase (which is safer).
#if !UNITY_EDITOR
        void OnEnable()
        {
            foreach (var graph in realtimeGraphs)
                AddGraph(graph);
            foreach (var graph in staticGraphs)
                AddGraph(graph);

            void AddGraph(MixtureGraph graph)
            {
                foreach (var outputTexture in graph.outputTextures)
                    graphMap[outputTexture] = graph;
            }
        }

        public static MixtureGraph GetGraphFromTexture(Texture texture)
        {
            if (instance == null)
            {
                instance = Resources.Load<MixtureDatabase>(databaseResourcePath);
                if (instance == null)
                {
                    Debug.LogError("Mixture Database asset not found! Realtime Mixtures won't work as expected.");
                    return null;
                }
            }

            instance.graphMap.TryGetValue(texture, out var graph);
            return graph;
        }
#endif
    }
}