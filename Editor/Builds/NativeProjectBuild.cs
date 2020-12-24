using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityCpp.Editor.Builds
{
    public static class NativeProjectBuild
    {
        private const string _buildProjectMenuItem = "Assets/UnityCpp/Project/Build &B";
        private const string _cleanProjectMenuItem = "Assets/UnityCpp/Project/Clean &C";
        private const string _cppProjectPath = "CppSource";
        private const string _cmakeCachesPath = "cmake-build-debug";
        private const string _progressBarTitle = "C++";
        private const string _buildTypeParameter = "-DCMAKE_BUILD_TYPE=Debug";
#if UNITY_EDITOR_OSX
        private const string _cmakePath = "/usr/local/bin/cmake";
        private const string _cmakeGenerationString = "CodeBlocks - Unix Makefiles";
        private const string _cmakeCompileParameter = "--target all -- -j 3";
#else
        private const string _cmakePath = "C:\\Program Files\\CMake\\bin\\cmake.exe";
        private const string _cmakeGenerationString = "CodeBlocks - MinGW Makefiles";
        private const string _cmakeCompileParameter = "--target all";
#endif

        private static bool _isAnythingRunning;

        [MenuItem(_buildProjectMenuItem)]
        public static void BuildProject()
        {
            if (_isAnythingRunning) return;
            _isAnythingRunning = true;
        
            ClearConsoleLogs();
            
            AssetDatabase.DisallowAutoRefresh();

            Debug.Log("---->>> Starting C++ project build");

            ProcessRunner runner = new ProcessRunner(_cppProjectPath, _cmakeCachesPath);

            if (!Directory.Exists(runner.cmakeCachesPath)) Directory.CreateDirectory(runner.cmakeCachesPath);
            
            EditorUtility.DisplayProgressBar(_progressBarTitle, "Starting C++ project build", 0F);

            string[] arguments = {
                _buildTypeParameter,
                $"-G \"{_cmakeGenerationString}\"",
                $"\"{runner.cppProjectPath}\"",
                $"-B \"{runner.cmakeCachesPath}\"",
            };

            runner.applicationPath = _cmakePath;
            runner.arguments = arguments;
            runner.workingDirectory = runner.cppProjectPath;
            runner.progressUpdateEvent = UpdateProgressBar;
            runner.exitEvent = DidFinishGeneratingCppProject;
            
            runner.RunProcess();
        }

        private static void DidFinishGeneratingCppProject(object sender, EventArgs e)
        {
            ProcessRunner runner = new ProcessRunner(_cppProjectPath, _cmakeCachesPath);
            
            string[] arguments = {
                "--build",
                $"\"{runner.cmakeCachesPath}\"",
                _cmakeCompileParameter
            };

            runner.applicationPath = _cmakePath;
            runner.arguments = arguments;
            runner.workingDirectory = runner.cppProjectPath;
            runner.progressUpdateEvent = UpdateProgressBar;
            runner.exitEvent = DidFinishBuildProcess;
            
            runner.RunProcess();
        }

        private static void DidFinishBuildProcess(object sender, EventArgs e)
        {
            Debug.Log("---->>> Finished C++ project build");
            
            EditorUtility.ClearProgressBar();
                
            AssetDatabase.AllowAutoRefresh();
                        
            AssetDatabase.Refresh();

            _isAnythingRunning = false;
        }

        [MenuItem(_cleanProjectMenuItem)]
        public static void CleanProject()
        {
            if (_isAnythingRunning) return;
            _isAnythingRunning = true;
            
            Debug.Log("Started cleaning C++ build caches");

            string projectPath = Directory.GetParent(Application.dataPath).ToString();
            string cppProjectPath = Path.Combine(projectPath, _cppProjectPath);
            string cmakeCachesPath = Path.Combine(cppProjectPath, _cmakeCachesPath);
            
            if (Directory.Exists(cmakeCachesPath))
            {
                Directory.Delete(cmakeCachesPath, true);
            }
            
            Debug.Log("Finished cleaning C++ build caches");

            _isAnythingRunning = false;
        }

        private static void ClearConsoleLogs()
        {
            Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null,null);
        }

        private static void UpdateProgressBar(object sender, UpdateEventArgs args)
        {
            Debug.Log($"[{(int) (100 * args.config.progress)}%] {args.config.info}");
            EditorUtility.DisplayProgressBar(_progressBarTitle, args.config.info, args.config.progress);
        }
    }
}