using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityCpp.Editor
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
        private const string _cmakeCompileParameter = " --target all -- -j 3";
#else
        private const string _cmakePath = "C:\\Program Files\\CMake\\bin\\cmake.exe";
        private const string _cmakeGenerationString = "Visual Studio 16 2019";
        private const string _cmakeCompileParameter = "";
#endif

        private static ConcurrentQueue<ProgressConfig> _configs;
        private static ConcurrentQueue<Action> _actions;
         
        [MenuItem(_buildProjectMenuItem), InitializeOnLoadMethod]
        public static void BuildProject()
        {
            ClearConsoleLogs();
            
            AssetDatabase.DisallowAutoRefresh();

            Debug.Log("---->>> Starting C++ project build");
            
            PrepareUpdates();

            string projectPath = Directory.GetParent(Application.dataPath).ToString();
            string cppProjectPath = Path.Combine(projectPath, _cppProjectPath);
            string cmakeCachesPath = Path.Combine(cppProjectPath, _cmakeCachesPath);

            if (!Directory.Exists(cmakeCachesPath)) Directory.CreateDirectory(cmakeCachesPath);
            
            EditorUtility.DisplayProgressBar(_progressBarTitle, "Starting C++ project build", 0F);

            string arguments = $"{_buildTypeParameter} " +
                               $"-G \"{_cmakeGenerationString}\" " +
                               $"\"{cppProjectPath}\" " +
                               $"-B \"{cmakeCachesPath}\"";
            RunProcess(cppProjectPath, arguments,  (a, b) =>
            {
                RunProcess(cppProjectPath, $"--build \"{cmakeCachesPath}\"{_cmakeCompileParameter}", (x, y) =>
                {
                    _actions.Enqueue(() =>
                    {
                        EditorUtility.ClearProgressBar();

                        EndUpdates();
                
                        Debug.Log("---->>> Finished C++ project build");
                
                        AssetDatabase.AllowAutoRefresh();
                        
                        AssetDatabase.Refresh();
                    });
                });                
            });
        }

        [MenuItem(_cleanProjectMenuItem)]
        public static void CleanProject()
        {
            string projectPath = Directory.GetParent(Application.dataPath).ToString();
            string cppProjectPath = Path.Combine(projectPath, _cppProjectPath);
            string cmakeCachesPath = Path.Combine(cppProjectPath, _cmakeCachesPath);
            
            Debug.Log("Started cleaning C++ build caches");
            string arguments = $"--clean " +
                               $"\"{cppProjectPath}\" " +
                               $"-B \"{cmakeCachesPath}\"";
            RunProcess(cppProjectPath, arguments, (a, b) =>
            {
                if (Directory.Exists(cmakeCachesPath))
                {
                    Directory.Delete(cmakeCachesPath, true);
                }
                Debug.Log("Finished cleaning C++ build caches");                
            });
        }
        
        private static void PrepareUpdates()
        {
            _configs = new ConcurrentQueue<ProgressConfig>();
            _actions = new ConcurrentQueue<Action>();
            EditorApplication.update += EditorUpdate;
        }
        
        private static void EndUpdates()
        {
            EditorApplication.update -= EditorUpdate;
            _configs = null;
            _actions = null;
        }

        private static void ClearConsoleLogs()
        {
            Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null,null);
        }

        private static void EditorUpdate()
        {
            if (!_configs.TryDequeue(out ProgressConfig config))
            {
                if (_actions.TryDequeue(out Action action))
                {
                    action.Invoke();
                }

                return;
            }
            Debug.Log($"[{(int)(100 * config.progress)}%] {config.info}");
            EditorUtility.DisplayProgressBar(_progressBarTitle, config.info, config.progress);
        }

        private static void RunProcess(string workingDirectory, string arguments, EventHandler exitEvent)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(_cmakePath, arguments)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                LoadUserProfile = true
            };

            string logContents = $"Running command line: {startInfo.FileName} {startInfo.Arguments}\n" +
                                 $"Working directory: {workingDirectory}";
            Debug.Log(logContents);

            
            Process buildProcess = new Process
            {
                StartInfo = startInfo, 
                EnableRaisingEvents = true
            };

            buildProcess.OutputDataReceived += BuildProcessOnOutputDataReceived;
            buildProcess.ErrorDataReceived += BuildProcessOnErrorDataReceived;
            buildProcess.Exited += exitEvent;


            if (!buildProcess.Start())
            {
                Debug.LogError("Failed to start cmake process!");
                return;
            }
            
            buildProcess.BeginOutputReadLine();
            buildProcess.BeginErrorReadLine();
            
            new Thread(buildProcess.WaitForExit).Start();
        }

        private static void BuildProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            if (e.Data.Contains("warning:"))
            {
                Debug.LogWarning(e.Data);
            }
            else
            {
                Debug.LogError(e.Data);
            }
        }

        private static void BuildProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (sender)
            {
                string line = e.Data;

                if (string.IsNullOrEmpty(line)) return;

                ProgressConfig config = new ProgressConfig();

                bool fileRegexResult = MatchRegex(line, "\\[\\s+([0-9]+)%\\]", progress =>
                {
                    config.progress = float.Parse(progress) * .01F;
                    if (config.progress < 1F)
                    {
                        return MatchRegex(line, "/(\\w+\\.cpp)\\.o", fileName =>
                        {
                            config.info = $"Compiled {fileName}";
                            return true;
                        });
                    }

                    return true;
                });
                if (!fileRegexResult && !MatchRegex(line, ".+Built\\s\\w+\\s(\\w+)", buildModule =>
                {
                    config.progress = 1F;
                    config.info = $"Built module {buildModule}";
                    return true;
                }))
                {
                    return;
                }

                _configs.Enqueue(config);
            }
        }

        private static bool MatchRegex(string contents, string regex, Func<string, bool> matchAction)
        {
            Match match = Regex.Match(contents, regex);
            if (!match.Success) return false;
            
            Group group = match.Groups[1];
            if (!group.Success) return false;
            
            Capture capture = @group.Captures[0];
            
            return matchAction.Invoke(capture.Value);
        }

        private struct ProgressConfig
        {
            public string info;
            public float progress;
        }
    }
}