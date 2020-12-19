using System;
using System.Collections.Generic;
using UnityCpp.Loader;
using UnityEditor;
using UnityEngine;

namespace UnityCpp.NativeBridge
{
    [DefaultExecutionOrder(100000)]
    public class NativeEnd : MonoBehaviour
    {
        private static readonly List<IntPtr> _allocatedNativePointers = new List<IntPtr>();
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
    }
}