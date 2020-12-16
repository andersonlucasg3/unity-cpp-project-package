using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityCpp.Editor
{
    public static class NativeProjectSetup
    {
        private const string _githubRepoUri = "https://github.com/andersonlucasg3/unity-cpp-project/archive/main.zip";
        private const string _setupProjectMenuItem = "Assets/UnityCpp Setup Project";
        private const string _temporaryFolderName = "CppProject";
        private const string _zipFileName = "github.zip";
        private const string _extractedDirectoryName = "unity-cpp-project-main";
        private const string _csharpUnityCppPath = "Assets/UnityCpp";

        private static readonly Dictionary<string, string> _directoriesToCopy = new Dictionary<string, string>
        {
            { "CppSource", "CppSource" },
            { $"{_csharpUnityCppPath}/Loader", $"{_csharpUnityCppPath}/Loader" },
            { $"{_csharpUnityCppPath}/NativeBridge", $"{_csharpUnityCppPath}/NativeBridge" }
        };
        
        [MenuItem(_setupProjectMenuItem)]
        private static void SetupProject()
        {
            Debug.Log("---->>> Beginning project setup");
            Debug.Log($"Downloading Cpp Project from repository path: {_githubRepoUri}");
            
            string filePath = DownloadRepository();
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("Download repository failed.");
                return;
            }
            
            string extractedPath = ExtractRepositoryFile(filePath);
            if (string.IsNullOrEmpty(extractedPath))
            {
                Debug.LogError("Unzip zip file failed.");
                return;
            }
            
            MoveProjectContent(extractedPath);
        }

        private static string DownloadRepository()
        {
            string repositoryFilePath;
            using (UnityWebRequest request = UnityWebRequest.Get(_githubRepoUri))
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    bool cancelPressed = EditorUtility.DisplayCancelableProgressBar("Downloading from GitHub", $"Downloading from repository Uri: {_githubRepoUri}", operation.progress);
                    if (!cancelPressed) continue;
                    request.Abort();
                    EditorUtility.ClearProgressBar();
                    return null;
                }
                repositoryFilePath = WriteDownloadedData(request.downloadHandler.data);
            }
            EditorUtility.ClearProgressBar();
            return repositoryFilePath;
        }

        private static string WriteDownloadedData(byte[] data)
        {
            string cachesPath = Path.Combine(Application.temporaryCachePath, _temporaryFolderName);
            if (Directory.Exists(cachesPath)) Directory.Delete(cachesPath, true);
            Directory.CreateDirectory(cachesPath);
            string cachesFilePath = Path.Combine(cachesPath, _zipFileName);
            Debug.Log($"Writing downloaded content to file at: {cachesPath}");
            File.WriteAllBytes(cachesFilePath, data);
            return cachesFilePath;
        }

        private static string ExtractRepositoryFile(string zipPath)
        {
            FastZipEvents events = new FastZipEvents();
            events.Progress += (sender, args) => EditorUtility.DisplayProgressBar("Unzipping", args.Name, args.PercentComplete);
            FastZip zip = new FastZip(events);

            string targetDirectory = Directory.GetParent(zipPath).ToString();
            zip.ExtractZip(zipPath, targetDirectory, null);

            Debug.Log($"Extracted repository zip to path: {targetDirectory}");
            EditorUtility.ClearProgressBar();

            return Path.Combine(targetDirectory, _extractedDirectoryName);
        }

        private static void MoveProjectContent(string extractedPath)
        {
            Debug.Log($"Extracted path: {extractedPath}");

            foreach (KeyValuePair<string, string> pair in _directoriesToCopy)
            {
                string projectPath = Directory.GetParent(Application.dataPath).ToString();

                string fromFullPath = Path.Combine(extractedPath, pair.Key);
                string toFullPath = Path.Combine(projectPath, pair.Value);

                CopyFilesRecursively(new DirectoryInfo(fromFullPath), new DirectoryInfo(toFullPath));
            }
        }
        
        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }
    }
}