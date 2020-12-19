using JetBrains.Annotations;
using UnityEngine;

namespace UnityCpp.NativeBridge.UnityBridges
{
    public class BehaviourBridge : ComponentBridge
    {
        [UsedImplicitly] public readonly Behaviour unityBehaviour;

        [UsedImplicitly]
        public bool enabled
        {
            get => unityBehaviour.enabled;
            set => unityBehaviour.enabled = value;
        }

        [UsedImplicitly]
        public bool isActiveAndEnabled => unityBehaviour.isActiveAndEnabled;

        protected BehaviourBridge(Behaviour behaviour) : base(behaviour) => unityBehaviour = behaviour;

        public static implicit operator BehaviourBridge(Behaviour behaviour) => new BehaviourBridge(behaviour);
    }
}