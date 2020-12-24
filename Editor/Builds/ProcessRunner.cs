using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityCpp.Editor.Builds
{
    internal class ProcessRunner
    {
        private readonly GccOutputParser _outputParser = new GccOutputParser();
        private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

        public string applicationPath { get; set; }
        public string[] arguments { get; set; }
        public string workingDirectory { get; set; }
        public EventHandler<UpdateEventArgs> progressUpdateEvent { get; set; }
        public EventHandler exitEvent { get; set; }
        
        public string projectPath { get; }
        public string cppProjectPath { get; }
        public string cmakeCachesPath { get; }

        public ProcessRunner(string cppProjectPath, string cmakeCachesPath)
        {
            projectPath = Directory.GetParent(Application.dataPath).ToString();
            this.cppProjectPath = Path.Combine(projectPath, cppProjectPath);
            this.cmakeCachesPath = Path.Combine(this.cppProjectPath, cmakeCachesPath);
        }
        
        public void RunProcess()
        {
            EditorApplication.update += EditorUpdate;
            
            string argumentsString = string.Join(" ", arguments);
            ProcessStartInfo startInfo = new ProcessStartInfo(applicationPath, argumentsString)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                LoadUserProfile = true,
            };
            SetupEnvironment(startInfo);

            string logContents = $"Running command line: {startInfo.FileName} {startInfo.Arguments}\n" +
                                 $"Working directory: {workingDirectory}";
            Debug.Log(logContents);

            Process buildProcess = new Process
            {
                StartInfo = startInfo, 
                EnableRaisingEvents = true
            };

            buildProcess.OutputDataReceived += _outputParser.BuildProcessOnOutputDataReceived;
            buildProcess.ErrorDataReceived += BuildProcessOnErrorDataReceived;
            buildProcess.Exited += (s, a) =>
            {
                _outputParser.configs.Enqueue(new ProgressConfig
                {
                    info = "Completed process", 
                    progress = 1F
                });
                _actions.Enqueue(() =>
                {
                    exitEvent.Invoke(s, a);
                    EditorApplication.update -= EditorUpdate;
                });
            };


            if (!buildProcess.Start())
            {
                Debug.LogError("Failed to start cmake process!");
                return;
            }
            
            buildProcess.BeginOutputReadLine();
            buildProcess.BeginErrorReadLine();
            
            new Thread(buildProcess.WaitForExit).Start();
        }

        private static void SetupEnvironment(ProcessStartInfo startInfo)
        {
            const string visualStudioPath = "C:\\Program Files (x86)\\Microsoft Visual Studio";
            const string windowsKitsPath = "C:\\Program Files (x86)\\Windows Kits";
            
            string[] compilersPaths = Directory.GetFiles(visualStudioPath, "cl.exe", SearchOption.AllDirectories);
            string compilerPath = "";
            foreach (string compiler in compilersPaths)
            {
                bool is64bitCompiler = Environment.Is64BitOperatingSystem && compiler.Contains("x64");
                bool is32bitCompiler = !Environment.Is64BitOperatingSystem && compiler.Contains("x86");
                if (!is64bitCompiler && !is32bitCompiler) continue;
                compilerPath = Directory.GetParent(compiler).ToString();
                break;
            }

            string[] visualStudioIncludes = Directory.GetDirectories(visualStudioPath, "include", SearchOption.AllDirectories);
            // visualStudioIncludes = GetAllSubDirectories(visualStudioIncludes);
            string[] visualStudioLibs = Directory.GetDirectories(visualStudioPath, "lib", SearchOption.AllDirectories);
            // visualStudioLibs = GetAllSubDirectories(visualStudioLibs);
            string[] windowsKitsIncludes = Directory.GetDirectories(windowsKitsPath, "Include", SearchOption.AllDirectories);
            // windowsKitsIncludes = GetAllSubDirectories(windowsKitsIncludes);
            string[] windowsKitsLibs = Directory.GetDirectories(windowsKitsPath, "Lib", SearchOption.AllDirectories);
            // windowsKitsLibs = GetAllSubDirectories(windowsKitsLibs);
            
            string include = $"{string.Join(";", visualStudioIncludes)};{string.Join(";", windowsKitsIncludes)};";
            string lib = $"{string.Join(";", visualStudioLibs)};{string.Join(";", windowsKitsLibs)};";
            
            string path = startInfo.Environment["Path"];
            path += $"{path};{compilerPath};{include}{lib};C:\\MinGW\\bin;";
            startInfo.Environment["Path"] = path;
            
            path = startInfo.EnvironmentVariables["Path"];
            path += $"{path};{compilerPath};{include}{lib};C:\\MinGW\\bin";
            startInfo.EnvironmentVariables["Path"] = path;
        }

        // private static string[] GetAllSubDirectories(string[] directories)
        // {
        //     List<string> allSubDirectories = new List<string>(directories);
        //     foreach (string directory in directories)
        //     {
        //         string[] children = Directory.GetDirectories(directory);
        //         allSubDirectories.AddRange(children);
        //         allSubDirectories.AddRange(GetAllSubDirectories(children));
        //     }
        //     return allSubDirectories.ToArray();
        // } 

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

        private void EditorUpdate()
        {
            if (!_outputParser.configs.TryDequeue(out ProgressConfig config))
            {
                if (_actions.TryDequeue(out Action action))
                {
                    action.Invoke();
                }
                return;
            }

            progressUpdateEvent.Invoke(this, new UpdateEventArgs(config));
        }
    }

    public class UpdateEventArgs
    {
        public ProgressConfig config { get; }

        public UpdateEventArgs(ProgressConfig config)
        {
            this.config = config;
        }
    }
}