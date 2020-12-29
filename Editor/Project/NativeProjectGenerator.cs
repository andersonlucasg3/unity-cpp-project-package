using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using static UnityCpp.Editor.Utils.RegexUtils;

namespace UnityCpp.Editor.Project
{
    public static class NativeProjectGenerator
    {
        private const string _generateRegisterComponentsMenuItem = "Assets/UnityCpp/Project/Re-generate";
        private const string _cppProjectPath = "CppSource";
        private const string _componentsFileName = "ComponentsEntryPoint";
        private static readonly string _gameSourcesPath = $"{_cppProjectPath}/UnityCppLib/Game"; 
        private static readonly string _componentsSourcePath = $"{_gameSourcesPath}/{_componentsFileName}.cpp";
        private static readonly string _cmakeListsFilePath = $"{_cppProjectPath}/CMakeLists.txt";

        [UsedImplicitly]
        [MenuItem(_generateRegisterComponentsMenuItem)]
        public static void GenerateNativeRegistration()
        {
            string projectPath = Directory.GetParent(Application.dataPath).ToString();
            
            Debug.Log("---->>> Beginning generation of components registration");
            
            WriteRegistrationSourceFile(projectPath, out HeaderFileInfo[] headersInfos);

            Debug.Log("---->>> Finished generating components registration");

            Debug.Log("---->>> Beginning to update CMakeLists.txt project");

            UpdateCMakeListsProject(projectPath, headersInfos);
            
            Debug.Log("---->>> Finished updating CMakeLists.txt project");
        }

        private static void WriteRegistrationSourceFile(string projectPath, out HeaderFileInfo[] headersInfos)
        {
            void AddInclude(string headerName, TextWriter w)
            {
                w.WriteLine($"#include \"{headerName}.h\"");
            }
            
            void AddRegisterCall(HeaderFileInfo headerFileInfo, TextWriter w)
            {
                w.WriteLine($"\t{headerFileInfo.fullQualifiedClassName}::Register();");
            }
            
            if (File.Exists(_componentsSourcePath))
            {
                File.Delete(_componentsSourcePath);
            }
            
            string gameSourcesPath = Path.Combine(projectPath, _gameSourcesPath);

            TextWriter writer = File.CreateText(_componentsSourcePath);
            AddInclude(_componentsFileName, writer);
            
            string[] files = Directory.GetFiles(gameSourcesPath, "*.h", SearchOption.AllDirectories);
            string[] filteredFiles = Array.FindAll(files, input => !input.Contains(_componentsFileName));
            headersInfos = Array.ConvertAll(filteredFiles, input => new HeaderFileInfo(input));

            writer.WriteLine();
            
            foreach (HeaderFileInfo headerFileInfo in headersInfos)
            {
                AddInclude(headerFileInfo.relativeFullPath, writer);
            }
            
            writer.WriteLine();

            writer.WriteLine("void RegisterComponents() {");
            
            foreach (HeaderFileInfo headerFileInfo in headersInfos)
            {
                AddRegisterCall(headerFileInfo, writer);
                
                Debug.Log($"Registered class: {headerFileInfo.fullQualifiedClassName}");
            }
            writer.WriteLine("}");
            
            writer.Close();
        }

        private static void UpdateCMakeListsProject(string projectPath, IReadOnlyList<HeaderFileInfo> headersInfos)
        {
            const string componentsGoHereString = "#COMPONENTS_GO_HERE";
            string cmakeListsPath = Path.Combine(projectPath, _cmakeListsFilePath);
            string cmakeListsContents = File.ReadAllText(cmakeListsPath);

            List<string> outputNames = new List<string>();
            
            string classesPath = _gameSourcesPath.Replace($"{_cppProjectPath}/", "");
            
            for (int index = 0; index < headersInfos.Count; index++)
            {
                string className = headersInfos[index].fileNameWithoutExtension;
                string headerFile = $"{classesPath}/{className}.h";
                string sourceFile = $"{classesPath}/{className}.cpp";
                if (!cmakeListsContents.Contains(headerFile)) outputNames.Add($"\t\t{headerFile}");
                if (!cmakeListsContents.Contains(sourceFile)) outputNames.Add($"\t\t{sourceFile}");
            }
            outputNames.Add(componentsGoHereString);

            string outputCmakeLists = cmakeListsContents.Replace(componentsGoHereString, string.Join("\n", outputNames));
            File.WriteAllText(cmakeListsPath, outputCmakeLists);
        }
    }

    internal readonly struct HeaderFileInfo
    {
        internal string parentPath { get; }
        internal string relativeParentPath { get; }
        internal string fileName { get; }
        internal string fileNameWithoutExtension { get; }
        internal string namespaceName { get; }

        internal string fullPath => Path.Combine(parentPath, fileName);
        internal string relativeFullPath => Path.Combine(relativeParentPath, fileName);

        internal string fullQualifiedClassName => string.IsNullOrEmpty(namespaceName) ? fileNameWithoutExtension : $"{namespaceName}::{fileNameWithoutExtension}";

        public HeaderFileInfo(string filePath, string relativePath = "") : this()
        {
            parentPath = Directory.GetParent(filePath).ToString();
            
            relativeParentPath = string.IsNullOrEmpty(relativePath) ? parentPath : parentPath.Replace(relativePath, "");
            
            fileName = Path.GetFileName(filePath);
            fileNameWithoutExtension = fileName.Replace(".h", "");
            
            string headerFileContents = File.ReadAllText(fullPath);

            namespaceName = SetupNamespaceName(headerFileContents);
        }

        private static string SetupNamespaceName(string headerFileContents)
        {
            string namespaceValue = "";
            _ = MatchRegex(headerFileContents, "namespace\\s+(\\w+)\\s*\\{", match =>
            {
                namespaceValue = match;
                return true;
            });
            return namespaceValue;
        }
    }
}