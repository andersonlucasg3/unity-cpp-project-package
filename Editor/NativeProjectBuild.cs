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
#if UNITY_EDITOR_OSX
        private const string _cmakePath = "/usr/local/bin/cmake";
#else
        private const string _cmakePath = "/c/Program Files/CMake/bin/cmake";
#endif

        private static ConcurrentQueue<ProgressConfig> _configs;
        private static ConcurrentQueue<Action> _actions;
        
        [MenuItem(_buildProjectMenuItem)]
        public static void BuildProject()
        {
            ClearConsoleLogs();

            Debug.Log("---->>> Starting C++ project build");
            
            _configs = new ConcurrentQueue<ProgressConfig>();
            _actions = new ConcurrentQueue<Action>();
            EditorApplication.update += EditorUpdate;


            string projectPath = Directory.GetParent(Application.dataPath).ToString();
            string cppProjectPath = Path.Combine(projectPath, _cppProjectPath);
            string cmakeCachesPath = Path.Combine(cppProjectPath, _cmakeCachesPath);

            if (!Directory.Exists(cmakeCachesPath)) Directory.CreateDirectory(cmakeCachesPath);
            
            EditorUtility.DisplayProgressBar(_progressBarTitle, "Starting C++ project build", 0F);
            
            RunProcess(cppProjectPath, $"-DCMAKE_BUILD_TYPE=Debug -G \"CodeBlocks - Unix Makefiles\" {cppProjectPath} -B {cmakeCachesPath}",  (a, b) =>
            {
                RunProcess(cppProjectPath, $"--build {cmakeCachesPath} --target all -- -j 3", BuildProcessOnExited);                
            });
        }

        private static void ClearConsoleLogs()
        {
            Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null,null);
        }

        [MenuItem(_cleanProjectMenuItem)]
        public static void CleanProject()
        {
            string projectPath = Directory.GetParent(Application.dataPath).ToString();
            string cppProjectPath = Path.Combine(projectPath, _cppProjectPath);
            string cmakeCachesPath = Path.Combine(cppProjectPath, _cmakeCachesPath);
            
            Debug.Log("Started cleaning C++ build caches");
            if (Directory.Exists(cmakeCachesPath))
            {
                Directory.Delete(cmakeCachesPath, true);
            }
            Debug.Log("Finished cleaning C++ build caches");
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
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
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

        private static void BuildProcessOnExited(object sender, EventArgs e)
        {
            _actions.Enqueue(() =>
            {
                EditorUtility.ClearProgressBar();

                EditorApplication.update -= EditorUpdate;
                
                Debug.Log("---->>> Finished C++ project build");
            });
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