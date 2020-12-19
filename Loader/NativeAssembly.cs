#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#define UNITY_WINDOWS
#endif

using System;
using System.Runtime.InteropServices;
using UnityCpp.NativeBridge;
using UnityEngine;

namespace UnityCpp.Loader
{
    using static NativeConstants;
    
    public static class NativeAssembly
    {
        [DllImport(nativeLoaderName, EntryPoint = nativeLoadLibraryFuncName, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr _LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string assemblyName);
        
        [DllImport(nativeLoaderName, EntryPoint = nativeFreeLibraryFuncName, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _FreeLibrary(IntPtr handle);

        [DllImport(nativeLoaderName, EntryPoint = nativeGetProcAddressFuncName)]
        private static extern IntPtr _GetProcAddress(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string methodName);

        [DllImport(nativeLoaderName, EntryPoint = nativeGetErrorFuncName)]
        [return: MarshalAs(UnmanagedType.LPStr)]
#if UNITY_WINDOWS
        private static extern int _GetError();
#else
        private static extern string _GetError();
#endif
        
        public static IntPtr Load(string fileName) => LoadLibrary(fileName);

        public static bool Unload(IntPtr handle) => FreeLibrary(handle);

        public static TMethod GetMethod<TMethod>(IntPtr handle, string methodName) where TMethod : class
        {
            if (handle == IntPtr.Zero)
            {
                Debug.Log($"NULL handle, cannot get method {methodName}");
                return null;
            }

            IntPtr methodPtr = GetProcAddress(handle, methodName);
            if (methodPtr != IntPtr.Zero) return Marshal.GetDelegateForFunctionPointer<TMethod>(methodPtr);

            Debug.Log($"NULL method handle, could not find method {methodName}");
            return null;
        }
        
        private static IntPtr LoadLibrary(string fileName)
        {
            IntPtr handle = _LoadLibrary(fileName);
#if UNITY_WINDOWS
            if (handle != IntPtr.Zero) return handle;
            int error = Marshal.GetLastWin32Error();
            Debug.Log($"Could not load assembly {fileName}\nError code: {error}");
#else
            string error = _GetError();
            if (string.IsNullOrEmpty(error)) return handle;
            Debug.Log($"Could not load assembly {fileName}\n{error}");
#endif
            return IntPtr.Zero;
        }

        private static bool FreeLibrary(IntPtr module)
        {
            if (module == IntPtr.Zero)
            {
                Debug.Log("NULL module");
                return false;
            }
            if (_FreeLibrary(module))
            {
                return true;
            }

#if UNITY_WINDOWS
            int error = _GetError();
            Debug.Log($"Failed to free module.\nError code: {error}");
#else
            string error = _GetError();
            Debug.Log($"Failed to free module.\n{error}");
#endif
            return false;
        }

        private static IntPtr GetProcAddress(IntPtr module, string methodName)
        {
            if (module == IntPtr.Zero)
            {
                Debug.Log("Could not load method, null module.");
                return IntPtr.Zero;
            }

            IntPtr methodPtr = _GetProcAddress(module, methodName);
#if UNITY_WINDOWS
            if (methodPtr != IntPtr.Zero) return methodPtr;
            int error = _GetError();
            Debug.Log($"Could not load method symbol {methodName}.\nError code: {error}");
#else
            string error = _GetError();
            if (string.IsNullOrEmpty(error)) return methodPtr;
            Debug.Log($"Could not load method symbol {methodName}.\n{error}");
#endif
            return IntPtr.Zero;
        }
    }
}