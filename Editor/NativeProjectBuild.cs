using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityCpp.Editor
{
    public static class NativeProjectBuild
    {
        private const string _buildProjectMenuItem = "Assets/UnityCpp/Setup Project #B";
        private const string _cppProjectPath = "CppSource";
        private const string _progressBarTitle = "C++";

        [MenuItem(_buildProjectMenuItem)]
        public static void BuildProject()
        {
            Debug.Log("---->>> Starting C++ project build");

            string projectPath = Directory.GetParent(Application.dataPath).ToString();
            string cppProjectPath = Path.Combine(projectPath, _cppProjectPath);
            
            EditorUtility.DisplayProgressBar(_progressBarTitle, "Starting C++ project build", 0F);

            RunProcess(cppProjectPath);

            EditorUtility.ClearProgressBar();

            Debug.Log("---->>> Finished C++ project build");
        }

        private static void RunProcess(string projectPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("cmake", $"--build {projectPath} -- -j 4")
            {
                WorkingDirectory = projectPath
            };

            Process buildProcess = new Process
            {
                StartInfo = startInfo
            };

            buildProcess.OutputDataReceived += BuildProcessOnOutputDataReceived;

            buildProcess.Start();
            buildProcess.WaitForExit();
        }

        private static void BuildProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data;

            float progress = 0F;
            string fileName = "";
            Match match = Regex.Match(line, "\\[\\s+([0-9]+)%\\]");
            if (match.Success)
            {
                Group group = match.Groups[0];
                if (group.Success)
                {
                    Capture capture = group.Captures[0];
                    if (capture.Length > 0)
                    {
                        progress = float.Parse(capture.Value) * .01F;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            match = Regex.Match(line, "/(\\w+\\.cpp)\\.o");
            if (match.Success)
            {
                Group group = match.Groups[0];
                if (group.Success)
                {
                    Capture capture = group.Captures[0];
                    if (capture.Length > 0)
                    {
                        fileName = capture.Value;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            
            EditorUtility.DisplayProgressBar(_progressBarTitle, $"Compiling {fileName}", progress);
        }
    }
}