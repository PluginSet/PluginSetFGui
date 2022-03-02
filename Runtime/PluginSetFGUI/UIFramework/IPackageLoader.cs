using FairyGUI;
using PluginSet.Core;

namespace PluginSet.FGUI
{
    public interface IPackageLoader
    {
        AsyncOperationHandle PreparePackage(string packageName);
        
        UIPackage LoadPackage(string packageName);

        void OnBranchChanged(UIPackage package, string fromBranch, string toBranch);
        
        void ReloadPackage(UIPackage package);
        
        void ReleasePackage(UIPackage package, bool remove = false);
    }
}