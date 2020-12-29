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
        private static readonly string _unityCppLibPath = Path.Combine(_cppProjectPath, "UnityCppLib");
        private static readonly string _gameSourcesPath = Path.Combine(_unityCppLibPath, "Game"); 
        private static readonly string _componentsSourcePath = Path.Combine(_gameSourcesPath, $"{_componentsFileName}.cpp");
        private static readonly string _cmakeListsFilePath = Path.Combine(_cppProjectPath, "CMakeLists.txt");

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
            void AddInclude(string headerPath, TextWriter w)
            {
                w.WriteLine($"#include \"{headerPath}\"");
            }
            
            void AddRegisterCall(HeaderFileInfo headerFileInfo, TextWriter w)
            {
                w.WriteLine($"\t{headerFileInfo.fullQualifiedClassName}::Register();");
            }
            
            if (File.Exists(_componentsSourcePath))
            {
                File.Delete(_componentsSourcePath);
            }

            string unityCppLibPath = Path.Combine(projectPath, _unityCppLibPath);
            string gameSourcesPath = Path.Combine(projectPath, _gameSourcesPath);

            TextWriter writer = File.CreateText(_componentsSourcePath);
            AddInclude($"{_componentsFileName}.h", writer);
            
            string[] files = Directory.GetFiles(gameSourcesPath, "*.h", SearchOption.AllDirectories);
            string[] filteredFiles = Array.FindAll(files, input => !input.Contains(_componentsFileName));
            headersInfos = Array.ConvertAll(filteredFiles, input => new HeaderFileInfo(input, unityCppLibPath));

            writer.WriteLine();
            
            foreach (HeaderFileInfo headerFileInfo in headersInfos)
            {
                AddInclude(headerFileInfo.includePath, writer);
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
            foreach (HeaderFileInfo headerFileInfo in headersInfos)
            {
                string includePath = headerFileInfo.cmakeListsIncludePath;
                string headerFile = $"{includePath}.h";
                string sourceFile = $"{includePath}.cpp";
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
        private readonly string _namespaceName;
        private readonly string _fileNameWithoutExtension;
        
        internal string includePath { get; }
        internal string cmakeListsIncludePath { get; }

        internal string fullQualifiedClassName => string.IsNullOrEmpty(_namespaceName) ? _fileNameWithoutExtension : $"{_namespaceName}::{_fileNameWithoutExtension}";

        public HeaderFileInfo(string filePath, string relativeToPath = "") : this()
        {
            string parentPath = Directory.GetParent(filePath).ToString();

            string fileName = Path.GetFileName(filePath);
            _fileNameWithoutExtension = fileName.Replace(".h", "");
            
            includePath = string.IsNullOrEmpty(relativeToPath) ? parentPath : parentPath.Replace(relativeToPath, "");
            if (includePath.StartsWith(Path.DirectorySeparatorChar.ToString())) includePath = includePath.Substring(1);
            includePath = Path.Combine(includePath, fileName).Replace(Path.DirectorySeparatorChar.ToString(), "/");

            cmakeListsIncludePath = string.IsNullOrEmpty(relativeToPath) ? parentPath : parentPath.Replace(Directory.GetParent(relativeToPath).ToString(), "");
            if (cmakeListsIncludePath.StartsWith(Path.DirectorySeparatorChar.ToString())) cmakeListsIncludePath = cmakeListsIncludePath.Substring(1);
            cmakeListsIncludePath = Path.Combine(cmakeListsIncludePath, _fileNameWithoutExtension).Replace(Path.DirectorySeparatorChar.ToString(), "/");

            string headerFileContents = File.ReadAllText(filePath);

            _namespaceName = SetupNamespaceName(headerFileContents);
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