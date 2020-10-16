using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Mixture
{
    public static class BuildPlayerCallback
    {
        [InitializeOnLoadMethod]
        public static void RegisterMixtureBuildCallbacks()
        {
            // TODO: replace with pre build callback
            // Warning: this will break if the user call this function with its own callbacks.
            // Only the last registered function will be called, so if this function isn't called automatically,
            // it can be called manually.
            BuildPlayerWindow.RegisterBuildPlayerHandler(buildPlayerOptions => {
                BuildMixtureAssetBundle(buildPlayerOptions.target);
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
            });
        }

        public static void BuildMixtureAssetBundle(BuildTarget buildTarget)
        {
            AssetBundleBuild buildMap = new AssetBundleBuild{ assetBundleName = "MixtureAssetBundle"};
            List<string> mixturePathes = new List<string>();

            var db = ScriptableObject.CreateInstance<MixtureDatabase>();

            foreach (var path in Directory.GetFiles("Assets", "*.asset", SearchOption.AllDirectories))
                AddMixtureToAssetBundle(path);
            foreach (var path in Directory.GetFiles("Packages", "*.asset", SearchOption.AllDirectories))
                AddMixtureToAssetBundle(path);

            void AddMixtureToAssetBundle(string path)
            {
                var graph = MixtureEditorUtils.GetGraphAtPath(path);

                if (graph != null)
                {
                    if (graph.isRealtime)
                        db.realtimeGraphs.Add(graph);
                    else if (graph.embedInBuild)
                        db.staticGraphs.Add(graph);
                }
            }

            buildMap.assetNames = mixturePathes.ToArray();

            if (!Directory.Exists("Assets/Resources/Mixture"))
                Directory.CreateDirectory("Assets/Resources/Mixture");
            AssetDatabase.DeleteAsset("Assets/Resources/Mixture/DataBase.asset");
            AssetDatabase.CreateAsset(db, "Assets/Resources/Mixture/DataBase.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}