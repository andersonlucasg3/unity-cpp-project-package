using UnityEditor;
using UnityEditor.PackageManager;
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