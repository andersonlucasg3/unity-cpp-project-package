using JetBrains.Annotations;
using UnityCpp.NativeBridge.Scripting;

namespace UnityCpp.NativeBridge.UnityBridges
{
    public class NativeMonoBehaviourBridge : MonoBehaviourBridge
    {
        [UsedImplicitly] public NativeMonoBehaviour monoBehaviour;

        protected NativeMonoBehaviourBridge(NativeMonoBehaviour behaviour) : base(behaviour) => monoBehaviour = behaviour;

        public static implicit operator NativeMonoBehaviourBridge(NativeMonoBehaviour behaviour) => new NativeMonoBehaviourBridge(behaviour);
    }
}