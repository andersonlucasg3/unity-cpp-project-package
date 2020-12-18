using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UnityCpp.Editor
{
    public static class NativeProjectUpdate
    {
        private const string _updatePackageMenuItem = "Assets/UnityCpp/Update Package";
        private const string _titleLabel = "Updating package";
        private const string _infoLabel = "Unity Cpp Project";

        [MenuItem(_updatePackageMenuItem)]
        public static void UpdatePackage()
        {
            float progress = 0F;
            
            PackageInfo info = PackageInfo.FindForAssembly(typeof(NativeProjectUpdate).Assembly);
            
            RemoveRequest removeRequest = Client.Remove(info.packageId);
            while (!removeRequest.IsCompleted)
            {
                EditorUtility.DisplayProgressBar(_titleLabel, _infoLabel, Mathf.Clamp(progress += 0.01F, 0F, .5F));
                Thread.Sleep(100);
            }
            EditorUtility.DisplayProgressBar(_titleLabel, _infoLabel, .5F);
            
            AddRequest addRequest = Client.Add(info.packageId);
            while (!addRequest.IsCompleted)
            {
                EditorUtility.DisplayProgressBar(_titleLabel, _infoLabel, Mathf.Clamp(progress += 0.01F, .5F, 1F));
                Thread.Sleep(100);
            }
            EditorUtility.DisplayProgressBar(_titleLabel, _infoLabel, 1F);
            Thread.Sleep(100);
            
            EditorUtility.ClearProgressBar();
        }
    }
}