using System;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityCpp.NativeBridge.UnityBridges
{
    public class GameObjectBridge : ObjectBridge
    {
        [UsedImplicitly] public readonly GameObject unityGameObject;

        [UsedImplicitly] public TransformBridge transform => unityGameObject.transform;
        [UsedImplicitly] public bool activeSelf => unityGameObject.activeSelf;
        [UsedImplicitly] public bool activeInHierarchy => unityGameObject.activeInHierarchy;
        [UsedImplicitly] public ulong sceneCullingMask => unityGameObject.sceneCullingMask;

        [UsedImplicitly] public bool isStatic
        {
            get => unityGameObject.isStatic;
            set => unityGameObject.isStatic = value;
        }

        [UsedImplicitly] public int layer
        {
            get => unityGameObject.layer;
            set => unityGameObject.layer = value;
        }

        [UsedImplicitly] public string tag
        {
            get => unityGameObject.tag;
            set => unityGameObject.tag = value;
        }

        public GameObjectBridge() : base(new GameObject()) => unityGameObject = (GameObject) unityObject;
        public GameObjectBridge(string name) : base(new GameObject(name)) => unityGameObject = (GameObject) unityObject;
        public GameObjectBridge(string name, Type[] components) : base(new GameObject(name, components)) => unityGameObject = (GameObject) unityObject;
        private GameObjectBridge(GameObject gameObject) : base(gameObject) => unityGameObject = gameObject;

        public static implicit operator GameObjectBridge(GameObject gameObject) => new GameObjectBridge(gameObject);

        [UsedImplicitly]
        public ComponentBridge AddComponent(Type componentType) => unityGameObject.AddComponent(componentType);

        [UsedImplicitly]
        public ComponentBridge GetComponent(Type componentType) => unityGameObject.GetComponent(componentType);

        [UsedImplicitly]
        public bool TryGetComponent(Type component, out ComponentBridge output)
        {
            if (unityGameObject.TryGetComponent(component, out Component outComponent))
            {
                output = outComponent;
                return true;
            }
            output = null;
            return false;
        }
    }
}