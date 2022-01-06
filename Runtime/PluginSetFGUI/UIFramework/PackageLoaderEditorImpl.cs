using FairyGUI;
using PluginSet.Core;

#if UNITY_EDITOR
namespace PluginSet.FGUI
{
    public class PackageLoaderEditorImpl: IPackageLoader
    {
        protected virtual string GetEditorPath(string packageName)
        {
            return "Assets/FairyGUI/" + packageName;
        }
        
        public AsyncOperationHandle PreparePackage(string packageName)
        {
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
            
            return UIPackage.AddPackage(GetEditorPath(packageName));
        }

        public void ReloadPackage(UIPackage package)
        {
            package.ReloadAssets();
        }

        public void ReleasePackage(UIPackage package)
        {
            // do nothing
//            if (FairyGUI.StageEngine.beingQuit)
//                return;
//            
//            UIPackage.RemovePackage(package.id);
        }
    }
}
#endif