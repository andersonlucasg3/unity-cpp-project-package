using System;
using UnityCpp.Loader;
using UnityEngine;

namespace UnityCpp.NativeBridge
{
    [DefaultExecutionOrder(int.MinValue)]
    public class NativeStart : MonoBehaviour
    {
        private void Awake()
        {
            string assemblyPath =  NativeConstants.GetAssemblyPath();
            Debug.Log($"Searching for native library in {assemblyPath}");

            IntPtr nativeAssemblyHandle = NativeAssembly.Load(assemblyPath);
            if (nativeAssemblyHandle == IntPtr.Zero)
            {
                Debug.Log($"Failed to load native assembly {assemblyPath}");
                return;
            }
            
            NativeMethods.Initialize(nativeAssemblyHandle);
            NativeEnd.SetNativeHandle(nativeAssemblyHandle);
            Destroy(gameObject);
        }
    }
}