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

        [UsedImplicitly]
        public static void Destroy(ObjectBridge obj, float t) => Object.Destroy(obj.unityObject);

        [UsedImplicitly]
        public static void DestroyImmediate(ObjectBridge obj, bool allowDestroyingAssets) => Object.DestroyImmediate(obj.unityObject, allowDestroyingAssets);

        [UsedImplicitly]
        public static void DontDestroyOnLoad(ObjectBridge target) => Object.DontDestroyOnLoad(target.unityObject);
    }
}