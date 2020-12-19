using System;
using UnityCpp.Loader;
using UnityEngine;

namespace UnityCpp.NativeBridge
{
    public class NativeEntryPoint : MonoBehaviour
    {
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
            if (!NativeAssembly.Unload(_nativeAssemblyHandle))
            {
                Debug.Log("Something went wrong unloading native code handle.");
            }
        }
    }
}