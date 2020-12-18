using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEditorInternal;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UnityCpp.Editor
{
    public static class NativeProjectUpdate
    {
        private const string _updatePackageMenuItem = "Assets/UnityCpp/Update Package";

        [MenuItem(_updatePackageMenuItem)]
        public static void UpdatePackage()
        {
            PackageInfo info = PackageInfo.FindForAssembly(typeof(NativeProjectUpdate).Assembly);
            Client.Remove(info.packageId);
            Client.Add(info.repository.url);
        }
    }
}