using UnityEngine;
using System;

namespace Mixture
{
    public class BuiltinRealtimeMixtureUpdater : MonoBehaviour
    {
        public event Action onPreRender;

        void Update() => onPreRender?.Invoke();
    }
}