using FairyGUI;
using PluginSet.Core;

namespace PluginSet.FGUI
{
    public interface IPackageLoader
    {
        AsyncOperationHandle PreparePackage(string packageName);
        
        UIPackage LoadPackage(string packageName);
        
        void ReloadPackage(UIPackage package);
        
        void ReleasePackage(UIPackage package);
    }
}