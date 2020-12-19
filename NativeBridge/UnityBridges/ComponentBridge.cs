using System;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityCpp.NativeBridge.UnityBridges
{
    public class ComponentBridge : ObjectBridge
    {
        [UsedImplicitly]
        public readonly Component unityComponent;

        [UsedImplicitly] public TransformBridge transform => unityComponent.transform;
        [UsedImplicitly] public GameObjectBridge gameObject => unityComponent.gameObject;
        
        [UsedImplicitly]
        public string tag
        {
            get => unityComponent.tag;
            set => unityComponent.tag = value;
        }
        
        protected ComponentBridge(Component component) : base(component) => unityComponent = component;

        public static implicit operator ComponentBridge(Component component) => new ComponentBridge(component);
        
        [UsedImplicitly]
        public ComponentBridge GetComponent(Type componentType) => unityComponent.GetComponent(componentType);

        [UsedImplicitly]
        public ComponentBridge[] GetComponents(Type componentType) => Convert(unityComponent.GetComponents(componentType));

        [UsedImplicitly]
        public bool TryGetComponent(Type componentType, out ComponentBridge component)
        {
            if (unityComponent.TryGetComponent(componentType, out Component comp))
            {
                component = comp;
                return true;
            }
            component = null;
            return false;
        }

        [UsedImplicitly]
        public ComponentBridge GetComponentInChildren(Type componentType) => unityComponent.GetComponentInChildren(componentType);

        [UsedImplicitly]
        public ComponentBridge GetComponentInChildren(Type componentType, bool includeInactive) => unityComponent.GetComponentInChildren(componentType, includeInactive);

        [UsedImplicitly]
        public ComponentBridge[] GetComponentsInChildren(Type componentType) => Convert(unityComponent.GetComponentsInChildren(componentType));

        [UsedImplicitly]
        public ComponentBridge[] GetComponentsInChildren(Type componentType, bool includeInactive) => Convert(unityComponent.GetComponentsInChildren(componentType, includeInactive));

        [UsedImplicitly]
        public ComponentBridge GetComponentInParent(Type componentType) => unityComponent.GetComponentInParent(componentType);

        [UsedImplicitly]
        public ComponentBridge[] GetComponentsInParent(Type componentType) => Convert(unityComponent.GetComponentsInParent(componentType));
        
        public Component toUnity() => unityComponent;

        private static ComponentBridge[] Convert(Component[] components) => Array.ConvertAll<Component, ComponentBridge>(components, each => each);
    }
}