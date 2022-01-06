using FairyGUI;
using PluginSet.Core;

namespace PluginSet.FGUI
{
    public class PackageLoaderImpl: IPackageLoader
    {
        protected virtual string GetResourceName(string packageName)
        {
            return packageName;
        }
        
        protected virtual string GetBundleName(string packageName)
        {
            return packageName;
        }
        
        public AsyncOperationHandle PreparePackage(string packageName)
        {
            var manager = ResourcesManager.Instance;
            var bundleName = GetBundleName(packageName);
            if (manager.ExistsBundle(bundleName))
                return manager.LoadBundleAsync(bundleName);
            
            return AsyncNonResultHandle<object>.Default;
        }

        public UIPackage LoadPackage(string packageName)
        {
            var package = UIPackage.GetByName(packageName);
            if (package != null)
            {
                ReloadPackage(package);
                return package;
            }
            
            var manager = ResourcesManager.Instance;
            var bundleName = GetBundleName(packageName);
            if (manager.ExistsBundle(bundleName))
                return UIPackage.AddPackage(manager.LoadBundle(bundleName));
            
            return UIPackage.AddPackage(GetResourceName(packageName));
        }
        
        public void ReloadPackage(UIPackage package)
        {
            var manager = ResourcesManager.Instance;
            var bundleName = GetBundleName(package.name);
            if (manager.ExistsBundle(bundleName))
                package.ReloadAssets(manager.LoadBundle(bundleName));
            else
                package.ReloadAssets();
        }

        public void ReleasePackage(UIPackage package)
        {
            var manager = ResourcesManager.Instance;
            var bundleName = GetBundleName(package.name);
            if (manager.ExistsBundle(bundleName))
                manager.ReleaseBundle(bundleName);
            
            // dont remove package
//            UIPackage.RemovePackage(package.id);
        }
    }
}