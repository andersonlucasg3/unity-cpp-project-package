using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityCpp.Loader;
using UnityCpp.NativeBridge.Reflection;
using UnityEngine;

namespace UnityCpp.NativeBridge
{
    [DefaultExecutionOrder(-100000)]
    public class NativeEntryPoint : MonoBehaviour
    {
        private static readonly List<IntPtr> _allocatedNativePointers = new List<IntPtr>();
        
        private IntPtr _nativeAssemblyHandle = IntPtr.Zero;

        private void Awake()
        {
            string assemblyPath =  NativeConstants.GetAssemblyPath();
            Debug.Log($"Searching for native library in {assemblyPath}");

            _nativeAssemblyHandle = NativeAssembly.Load(assemblyPath);
            if (_nativeAssemblyHandle == IntPtr.Zero)
            {
                Debug.Log($"Failed to load native assembly {assemblyPath}");
                return;
            }
            
            NativeMethods.Initialize(_nativeAssemblyHandle);
        }

        private void OnDestroy()
        {
            for (int index = 0; index < _allocatedNativePointers.Count; index++)
            {
                IntPtr nativePointer = _allocatedNativePointers[index];
                NativeMethods.destroyNativeMonoBehaviour.Invoke(nativePointer);
            }

            if (!NativeAssembly.Unload(_nativeAssemblyHandle))
            {
                Debug.Log("Something went wrong unloading native code handle.");
            }
        }

        public static int AddNativePointer(IntPtr nativePointer)
        {
            _allocatedNativePointers.Add(nativePointer);
            return _allocatedNativePointers.Count - 1;
        }

        public static void RemoveNativePointer(int index)
        {
            _allocatedNativePointers.RemoveAt(index);
        }
    }
}