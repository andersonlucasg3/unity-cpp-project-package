using System.Collections.Generic;

namespace UnityCpp.Editor.Builds
{
    internal static class EnvironmentProcessor
    {
        private static string[] _pathEssentialPaths =
        {
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\Extensions\\Microsoft\\IntelliCode\\CLI",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\VC\\Tools\\MSVC\\14.28.29333\\bin\\Hostx64\\x64",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\VC\\VCPackages",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\CommonExtensions\\Microsoft\\TeamFoundation\\Team Explorer",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\bin\\Roslyn",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Team Tools\\Performance Tools",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\Common\\VSPerfCollectionTools\\vs2019\\",
            "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\10.0.19041.0\\x64",
            "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\x64",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\\\MSBuild\\Current\\Bin",
            "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\IDE\\",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\Common7\\Tools\\",
        };

        private static string[] _includePaths =
        {
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\VC\\Tools\\MSVC\\14.28.29333\\ATLMFC\\include",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\VC\\Tools\\MSVC\\14.28.29333\\include",
            "C:\\Program Files (x86)\\Windows Kits\\NETFXSDK\\4.8\\include\\um",
            "C:\\Program Files (x86)\\Windows Kits\\10\\include\\10.0.19041.0\\ucrt",
            "C:\\Program Files (x86)\\Windows Kits\\10\\include\\10.0.19041.0\\shared",
            "C:\\Program Files (x86)\\Windows Kits\\10\\include\\10.0.19041.0\\um",
            "C:\\Program Files (x86)\\Windows Kits\\10\\include\\10.0.19041.0\\winrt",
            "C:\\Program Files (x86)\\Windows Kits\\10\\include\\10.0.19041.0\\cppwinrt",
        };

        private static string[] _libPaths =
        {
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\VC\\Tools\\MSVC\\14.28.29333\\ATLMFC\\lib\\x64",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\VC\\Tools\\MSVC\\14.28.29333\\lib\\x64",
            "C:\\Program Files (x86)\\Windows Kits\\NETFXSDK\\4.8\\lib\\um\\x64",
            "C:\\Program Files (x86)\\Windows Kits\\10\\lib\\10.0.19041.0\\ucrt\\x64",
            "C:\\Program Files (x86)\\Windows Kits\\10\\lib\\10.0.19041.0\\um\\x64",
        };

        private static string[] _libPathPaths =
        {
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\VC\\Tools\\MSVC\\14.28.29333\\ATLMFC\\lib\\x64",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\VC\\Tools\\MSVC\\14.28.29333\\lib\\x64",
            "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\VC\\Tools\\MSVC\\14.28.29333\\lib\\x64\\store\\references",
            "C:\\Program Files (x86)\\Windows Kits\\10\\UnionMetadata\\10.0.19041.0",
            "C:\\Program Files (x86)\\Windows Kits\\10\\References\\10.0.19041.0",
            "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319",
        };
        
        internal static void ProcessEnvironment(IDictionary<string, string> environment)
        {
            // TODO(Anderson) Make this dynamic for any installed VS version and Windows SDK Kit
            
            string path = environment["Path"] ?? "";
            path += $"{path}:{string.Join(";", _pathEssentialPaths)}";
            environment["Path"] = path;

            environment["INCLUDE"] = string.Join(";", _includePaths);
            environment["LIB"] = string.Join(";", _libPaths);
            environment["LIBPATH"] = string.Join(";", _libPathPaths);
        } 
    }
}