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
        private IntPtr _nativeAssemblyHandle = IntPtr.Zero;

        private void OnDestroy()
        {
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
    }
}