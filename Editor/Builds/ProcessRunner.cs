﻿using System;
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
#if UNITY_EDITOR_OSX
        private readonly GccOutputParser _outputParser = new GccOutputParser();
#else
        private readonly VisualStudioOutputParser _outputParser = new VisualStudioOutputParser();
#endif
        
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