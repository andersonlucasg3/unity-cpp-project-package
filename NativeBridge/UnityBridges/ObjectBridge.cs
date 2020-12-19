using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityCpp.NativeBridge.UnityBridges
{
    public class ObjectBridge
    {
        [UsedImplicitly] public readonly Object unityObject;

        [UsedImplicitly]
        public string name
        {
            get => unityObject.name;
            set => unityObject.name = value;
        }

        [UsedImplicitly]
        public HideFlags hideFlags
        {
            get => unityObject.hideFlags;
            set => unityObject.hideFlags = value;
        }

        protected ObjectBridge(Object obj) => unityObject = obj;

        public static implicit operator ObjectBridge(Object obj) => new ObjectBridge(obj);

        [UsedImplicitly]
        public int GetInstanceID() => unityObject.GetInstanceID();
    }
}