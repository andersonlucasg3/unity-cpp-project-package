#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#define UNITY_WINDOWS
#endif

using UnityEngine;

namespace UnityCpp.NativeBridge
{
    internal static class NativeConstants
    {
#if UNITY_WINDOWS
#if UNITY_EDITOR
        private const string nativeCodeAssemblyPath = "/Plugins/NativeComponents/UnityCppLib.dll";
#else
        private const string nativeCodeAssemblyPath = "/Plugins/x86_64/UnityCppLib.dll";
#endif
#else
#if UNITY_EDITOR
        private const string nativeCodeAssemblyPath = "/Plugins/NativeComponents/libUnityCppLib.dylib";
#else
        private const string nativeCodeAssemblyPath = "/Plugins/libUnityCppLib.dylib";
#endif
#endif
        internal const string nativeLoaderName = "UnityCppAssemblyHelper";
        internal const string nativeGetErrorFuncName = "__GetError";
        internal const string nativeLoadLibraryFuncName = "__LoadLibrary";
        internal const string nativeFreeLibraryFuncName = "__FreeLibrary";
        internal const string nativeGetProcAddressFuncName = "__GetProcAddress";

        public static string GetAssemblyPath()
        { 
            return Application.dataPath + nativeCodeAssemblyPath;
        }
    }
}