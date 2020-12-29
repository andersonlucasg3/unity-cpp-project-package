using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

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
            
            WriteRegistrationSourceFile(projectPath, out string[] classesNames);

            Debug.Log("---->>> Finished generating components registration");

            Debug.Log("---->>> Beginning to update CMakeLists.txt project");

            UpdateCMakeListsProject(projectPath, classesNames);
            
            Debug.Log("---->>> Finished updating CMakeLists.txt project");
        }

        private static void WriteRegistrationSourceFile(string projectPath, out string[] classesNames)
        {
            void AddInclude(string headerName, TextWriter w)
            {
                w.WriteLine($"#include \"{headerName}.h\"");
            }
            
            void AddRegisterCall(string className, TextWriter w)
            {
                w.WriteLine($"\t{className}::Register();");
            }
            
            if (File.Exists(_componentsSourcePath))
            {
                File.Delete(_componentsSourcePath);
            }
            
            string gameSourcesPath = Path.Combine(projectPath, _gameSourcesPath);

            TextWriter writer = File.CreateText(_componentsSourcePath);
            AddInclude(_componentsFileName, writer);
            
            string[] files = Directory.GetFiles(gameSourcesPath, "*.h");
            IEnumerable<string> names = from file in files 
                select Path.GetFileName(file) 
                into name where !name.Contains(_componentsFileName) 
                select name.Replace(".h", "");
            classesNames = names.ToArray();

            writer.WriteLine();
            
            foreach (string className in classesNames)
            {
                AddInclude(className, writer);
            }
            
            writer.WriteLine();
            
            writer.WriteLine("void RegisterComponents() {");
            
            foreach (string className in classesNames)
            {
                AddRegisterCall(className, writer);
                
                Debug.Log($"Registered class: {className}");
            }
            writer.WriteLine("}");
            
            writer.Close();
        }

        private static void UpdateCMakeListsProject(string projectPath, IReadOnlyList<string> classesNames)
        {
            const string componentsGoHereString = "#COMPONENTS_GO_HERE";
            string cmakeListsPath = Path.Combine(projectPath, _cmakeListsFilePath);
            string cmakeListsContents = File.ReadAllText(cmakeListsPath);

            List<string> outputNames = new List<string>();
            
            string classesPath = _gameSourcesPath.Replace($"{_cppProjectPath}/", "");
            
            for (int index = 0; index < classesNames.Count; index++)
            {
                string className = classesNames[index];
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
}