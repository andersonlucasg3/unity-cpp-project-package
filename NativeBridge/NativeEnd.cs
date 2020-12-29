using System;
using UnityCpp.Loader;
using UnityEngine;

namespace UnityCpp.NativeBridge
{
    [DefaultExecutionOrder(int.MaxValue), DisallowMultipleComponent]
    public class NativeEnd : MonoBehaviour
    {
        private static int _nativeEndCountdown = 0;
        private static IntPtr _nativeAssemblyHandle = IntPtr.Zero;

        public static void SetNativeHandle(IntPtr handle)
        {
            _nativeAssemblyHandle = handle;
        }
        
        private void Awake()
        {
            _nativeEndCountdown += 1;
        }

        private void OnDestroy()
        {
            _nativeEndCountdown -= 1;

            if (_nativeEndCountdown > 0) return;
            
            NativeMethods.DeInitialize(_nativeAssemblyHandle);

            if (!NativeAssembly.Unload(_nativeAssemblyHandle))
            {
                Debug.Log("Something went wrong unloading native code handle.");
            }
        }
    }
}