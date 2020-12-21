using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityCpp.Loader;
using UnityCpp.NativeBridge.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityCpp.NativeBridge
{
    [DefaultExecutionOrder(int.MaxValue)]
    public class NativeEnd : MonoBehaviour
    {
        private static readonly List<IntPtr> _allocatedNativePointers = new List<IntPtr>();
        private static readonly ConcurrentQueue<IntPtr> _destructionQueue = new ConcurrentQueue<IntPtr>();
        
        private IntPtr _nativeAssemblyHandle = IntPtr.Zero;

#if UNITY_EDITOR
        private void Awake()
        {
            EditorApplication.playModeStateChanged += change =>
            {
                if (change == PlayModeStateChange.ExitingPlayMode)
                {
                    Destroy(gameObject);
                }
            };
        }
#endif

        private void Update()
        {
            while (_destructionQueue.TryDequeue(out IntPtr managedPointer))
            {
                ReflectionHelpers.DeallocPtr(managedPointer);
            }
        }

        private void OnDestroy()
        {
            for (int index = 0; index < _allocatedNativePointers.Count; index++)
            {
                IntPtr nativePointer = _allocatedNativePointers[index];
                NativeMethods.destroyNativeMonoBehaviour.Invoke(nativePointer);
            }

            NativeMethods.DeInitialize(_nativeAssemblyHandle);

            if (!NativeAssembly.Unload(_nativeAssemblyHandle))
            {
                Debug.Log("Something went wrong unloading native code handle.");
            }
        }

        private void OnApplicationQuit()
        {
            Destroy(gameObject);
        }

        public void SetNativeHandle(IntPtr handle)
        {
            _nativeAssemblyHandle = handle;
        }
        
        public static void AddNativePointer(IntPtr nativePointer)
        {
            _allocatedNativePointers.Add(nativePointer);
        }

        public static void RemoveNativePointer(IntPtr nativePointer)
        {
            _allocatedNativePointers.Remove(nativePointer);
        }

        public static void EnqueueDestruction(IntPtr managedPointer)
        {
            _destructionQueue.Enqueue(managedPointer);
        }
    }
}