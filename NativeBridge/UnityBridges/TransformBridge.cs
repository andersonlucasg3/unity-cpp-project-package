using JetBrains.Annotations;
using UnityEngine;

namespace UnityCpp.NativeBridge.UnityBridges
{
    public class TransformBridge : ComponentBridge
    {
        [UsedImplicitly]
        public readonly Transform unityTransform;
        
        [UsedImplicitly]
        public int childCount => unityTransform.childCount;
        
        [UsedImplicitly]
        public Vector3Bridge eulerAngles
        {
            get => unityTransform.eulerAngles;
            set => unityTransform.eulerAngles = value;
        }

        [UsedImplicitly] public Vector3Bridge forward
        {
            get => unityTransform.forward;
            set => unityTransform.forward = value;
        }

        [UsedImplicitly] public bool hasChanged => unityTransform.hasChanged;
        [UsedImplicitly] public int hierarchyCapacity => unityTransform.hierarchyCapacity;
        [UsedImplicitly] public int hierarchyCount => unityTransform.hierarchyCount;

        [UsedImplicitly] public Vector3Bridge localEulerAngles
        {
            get => unityTransform.localEulerAngles;
            set => unityTransform.localEulerAngles = value;
        }

        [UsedImplicitly] public Vector3Bridge localPosition
        {
            get => unityTransform.localPosition;
            set => unityTransform.localPosition = value;
        }

        [UsedImplicitly] public QuaternionBridge localRotation
        {
            get => unityTransform.localRotation;
            set => unityTransform.localRotation = value;
        }

        [UsedImplicitly] public Vector3Bridge localScale
        {
            get => unityTransform.localScale;
            set => unityTransform.localScale = value;
        }
        
        [UsedImplicitly] public Vector3Bridge lossyScale => unityTransform.lossyScale;

        [UsedImplicitly]
        public TransformBridge parent
        {
            get => unityTransform.parent;
            set => unityTransform.parent = value;
        }

        [UsedImplicitly]
        public TransformBridge root => unityTransform.root;

        [UsedImplicitly] public Vector3Bridge position
        {
            get => unityTransform.position;
            set => unityTransform.position = value;
        }

        [UsedImplicitly] public Vector3Bridge right
        {
            get => unityTransform.right;
            set => unityTransform.right = value;
        }

        [UsedImplicitly] public QuaternionBridge rotation
        {
            get => unityTransform.rotation;
            set => unityTransform.rotation = value;
        }

        [UsedImplicitly] public Vector3Bridge up
        {
            get => unityTransform.up;
            set => unityTransform.up = value;
        }

        private TransformBridge(Transform transform) : base(transform) => unityTransform = transform;

        public static implicit operator TransformBridge(Transform transform) => new TransformBridge(transform);
        public static implicit operator Transform(TransformBridge transformBridge) => transformBridge.unityTransform;
        
        public new Transform toUnity() => unityTransform;
    }
}