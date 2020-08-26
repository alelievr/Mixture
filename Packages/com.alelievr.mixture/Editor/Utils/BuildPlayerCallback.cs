using UnityEngine;
using UnityEditor;

namespace Mixture
{
    [InitializeOnLoad]
    static class BuildPlayerCallback
    {
        static BuildPlayerCallback() {
            BuildPlayerWindow.RegisterBuildPlayerHandler(
                    new System.Action<BuildPlayerOptions>(buildPlayerOptions =>
                    {
                        // TODO:
                        // buildAssetBundles();
                        BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
                    }));
        }
    }
}