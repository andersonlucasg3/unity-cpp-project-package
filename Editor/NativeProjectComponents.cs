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

        private static TextWriter _sourceFileStream;

        [UsedImplicitly]
        [MenuItem(_generateRegisterComponentsMenuItem)]
        public static void GenerateNativeComponentsRegistration()
        {
            Debug.Log("---->>> Beginning generation of components registration");
            
            string projectPath = Directory.GetParent(Application.dataPath).ToString();
            string gameSourcesPath = Path.Combine(projectPath, _gameSourcesPath);

            BeginSourceFile();
            
            string[] files = Directory.GetFiles(gameSourcesPath, "*.h");
            IEnumerable<string> classesNames = from file in files 
                select Path.GetFileName(file) 
                into name where !name.Contains(_componentsFileName) 
                select name.Replace(".h", "");
            classesNames = classesNames.ToList();

            foreach (string className in classesNames)
            {
                AddInclude(className);
            }
            
            _sourceFileStream.WriteLine();
            
            _sourceFileStream.WriteLine("void RegisterComponents() {");
            foreach (string className in classesNames)
            {
                AddRegisterCall(className);
                
                Debug.Log($"Registered class: {className}");
            }
            _sourceFileStream.WriteLine("}");
            
            _sourceFileStream.Close();
            _sourceFileStream = null;
            
            Debug.Log("---->>> Finished generating components registration");
        }

        private static void BeginSourceFile()
        {
            if (File.Exists(_componentsSourcePath))
            {
                File.Delete(_componentsSourcePath);
            }

            _sourceFileStream = File.CreateText(_componentsSourcePath);
            AddInclude(_componentsFileName);
            _sourceFileStream.WriteLine();
        }

        private static void AddInclude(string headerName)
        {
            _sourceFileStream.WriteLine($"#include \"{headerName}.h\"");
        }

        private static void AddRegisterCall(string className)
        {
            _sourceFileStream.WriteLine($"\t{className}::Register();");
        }
    }
}