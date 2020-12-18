using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UnityCpp.Editor
{
    public static class NativeProjectComponents
    {
        private const string _generateRegisterComponentsMenuItem = "Assets/UnityCpp/Re-generate native components";
        private const string _cppProjectPath = "CppSource";
        private const string _componentsFileName = "ComponentsEntryPoint";
        private static readonly string _gameSourcesPath = $"{_cppProjectPath}/UnityCppLib/Game"; 
        private static readonly string _componentsSourcePath = $"{_gameSourcesPath}/{_componentsFileName}.cpp";
        private static readonly string _cmakeListsFilePath = $"{_cppProjectPath}/CMakeLists.txt";

        [UsedImplicitly]
        [MenuItem(_generateRegisterComponentsMenuItem)]
        public static void GenerateNativeComponentsRegistration()
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
            string cmakeListsPath = Path.Combine(projectPath, _cmakeListsFilePath);
            string cmakeListsContents = File.ReadAllText(cmakeListsPath);
            
            string[] outputNames = new string[classesNames.Count * 2];
            
            string classesPath = _gameSourcesPath.Replace($"{_cppProjectPath}/", "");
            
            for (int index = 0; index < outputNames.Length;)
            {
                outputNames[index] = $"{classesPath}/{classesNames[index]}.h";
                index++;
                outputNames[index] = $"{classesPath}/{classesNames[index]}.cpp";
                index++;
            }

            string outputCmakeLists = cmakeListsContents.Replace("#COMPONENTS_GO_HERE", string.Join("\n", outputNames));
            File.WriteAllText(outputCmakeLists, cmakeListsPath);
        }
    }
}