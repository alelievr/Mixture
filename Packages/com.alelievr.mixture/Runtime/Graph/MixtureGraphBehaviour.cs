using UnityEngine;
using GraphProcessor;

namespace Mixture
{
    [ExecuteAlways]
    [AddComponentMenu("")] // Hide component as it's not ready yet
    public class MixtureGraphBehaviour : MonoBehaviour
    {
		public static readonly string	behaviourGraphTemplate = "Templates/BehaviourGraphTemplate";

        public MixtureGraph graph;

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            if (graph == null)
            {
                var template = Resources.Load<MixtureGraph>(behaviourGraphTemplate);
                graph = ScriptableObject.Instantiate(template);
                graph.ClearObjectReferences();
            }
#endif

            graph.name = name;
            graph.LinkToScene(gameObject.scene);
        }
    }
}