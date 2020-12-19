using JetBrains.Annotations;
using UnityEngine;

namespace UnityCpp.NativeBridge.UnityBridges
{
    public class MonoBehaviourBridge : BehaviourBridge
    {
        [UsedImplicitly] public MonoBehaviour unityMonoBehaviour;

        protected MonoBehaviourBridge(MonoBehaviour monoBehaviour) : base(monoBehaviour) => unityMonoBehaviour = monoBehaviour;

        public static implicit operator MonoBehaviourBridge(MonoBehaviour monoBehaviour) => new MonoBehaviourBridge(monoBehaviour);
    }
}