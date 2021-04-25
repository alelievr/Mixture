using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mixture
{
	[System.Serializable]
	public abstract class BasePrefabNode : MixtureNode
	{
		public override bool 	hasSettings => true;
		public override float	nodeWidth => MixtureUtils.defaultNodeWidth;

        public override bool    showDefaultInspector => true;

        protected abstract string defaultPrefabName { get; }

		protected override MixtureSettings defaultRTSettings
        {
            get
            {
                var settings = base.defaultRTSettings;
                settings.editFlags = EditFlags.All ^ EditFlags.POTSize;
                return Get2DOnlyRTSettings(settings);
            }
        }

        public GameObject       prefab;

        [System.NonSerialized]
        internal bool           prefabOpened = false;
#if UNITY_EDITOR
        [System.NonSerialized]
        protected bool          createNewPrefab = false;
#endif

        public override void OnNodeCreated()
        {
            base.OnNodeCreated();

#if UNITY_EDITOR
            createNewPrefab = true;
#endif
        }

#if UNITY_EDITOR
        protected abstract GameObject LoadDefaultPrefab();

        GameObject SavePrefab(GameObject sceneObject)
        {
            string dirPath = Path.GetDirectoryName(graph.mainAssetPath) + "/" + graph.name;
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(dirPath + "/" + defaultPrefabName + ".prefab");

            return PrefabUtility.SaveAsPrefabAssetAndConnect(sceneObject, prefabPath, InteractionMode.UserAction);
        }
#endif

        protected override void Enable()
        {
#if UNITY_EDITOR
            if (createNewPrefab)
            {
                // Create and save the new prefab
                var defaultPrefab = GameObject.Instantiate(LoadDefaultPrefab());
                prefab = SavePrefab(defaultPrefab);
                MixtureUtils.DestroyGameObject(defaultPrefab);
                ProjectWindowUtil.ShowCreatedAsset(prefab);
                EditorGUIUtility.PingObject(prefab);
            }
#endif
        }
	}
}