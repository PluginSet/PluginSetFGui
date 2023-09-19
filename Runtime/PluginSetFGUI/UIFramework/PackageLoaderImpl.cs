using System;
using System.Collections.Generic;
using FairyGUI;
using PluginSet.Core;

namespace PluginSet.FGUI
{
    public class PackageLoaderImpl: IPackageLoader
    {
        private Dictionary<string, Dictionary<string, AsyncOperationHandle>> _loadedPackages = new Dictionary<string, Dictionary<string, AsyncOperationHandle>>();
        
        public virtual string GetResourceName(string packageName)
        {
            return packageName;
        }
        
        public virtual string GetBundleName(string packageName)
        {
            return packageName;
        }

        public virtual string GetBundleName(string packageName, string branch)
        {
            return $"{GetBundleName(packageName)}_{branch}";
        }

        protected virtual object LoadPackageResource(string packageName, string fileName, string branch, Type type)
        {
            if (!_loadedPackages.TryGetValue(packageName, out var dict))
            {
                dict = new Dictionary<string, AsyncOperationHandle>();
                _loadedPackages.Add(packageName, dict);
            }
            
            var manager = ResourcesManager.Instance;
            var bundleName = GetBundleName(packageName);
            if (manager.ExistsBundle(bundleName))
            {
                if (!string.IsNullOrEmpty(branch))
                {
                    // 优先使用分支中的资源
                    var branchBundle = GetBundleName(packageName, branch);
                    if (manager.ExistsBundle(branchBundle))
                    {
                        if (!dict.ContainsKey(branchBundle))
                        {
                            manager.LoadBundle(branchBundle);
                            dict.Add(branchBundle, AsyncNonResultHandle<object>.Default);
                        }
                        
                        if (manager.ExistsAsset(branchBundle, fileName))
                            return manager.LoadAsset(branchBundle, fileName, type);
                    }
                }

                if (!dict.ContainsKey(bundleName))
                {
                    manager.LoadBundle(bundleName);
                    dict.Add(bundleName, AsyncNonResultHandle<object>.Default);
                }
                
                // 分支中无资源时，使用主干资源
                if (manager.ExistsAsset(bundleName, fileName))
                    return manager.LoadAsset(bundleName, fileName, type);
            }

            return manager.Load(fileName, type);
        }

        private UIPackage.LoadResource GetResourceLoader(string packageName)
        {
            return delegate(string name, string extension, string itemBranch, Type type, out DestroyMethod method)
            {
                method = DestroyMethod.Unload;
                return LoadPackageResource(packageName, name, itemBranch, type);
            };
        }

        public virtual AsyncOperationHandle PreparePackage(string packageName)
        {
            if (!_loadedPackages.TryGetValue(packageName, out var dict))
            {
                dict = new Dictionary<string, AsyncOperationHandle>();
                _loadedPackages.Add(packageName, dict);
            }
            
            var manager = ResourcesManager.Instance;
            var bundleName = GetBundleName(packageName);
            if (!dict.TryGetValue(bundleName, out var mainHandle))
            {
                if (manager.ExistsBundle(bundleName))
                    mainHandle = manager.LoadBundleAsync(bundleName);
                else
                    mainHandle = AsyncNonResultHandle<object>.Default;
                dict.Add(bundleName, mainHandle);
            }
            
            var curBranch = UIPackage.branch;
            if (!string.IsNullOrEmpty(curBranch))
            {
                var branchBundle = GetBundleName(packageName, curBranch);
                if (!dict.TryGetValue(branchBundle, out var branchHandle))
                {
                    if (manager.ExistsBundle(branchBundle))
                    {
                        branchHandle = manager.LoadBundleAsync(branchBundle);
                        dict.Add(branchBundle, branchHandle);
                    }
                }
                if (branchHandle != null)
                    return new MultiAsyncOperations(mainHandle, branchHandle);
            }
            
            return mainHandle;
        }

        public UIPackage LoadPackage(string packageName)
        {
            var package = UIPackage.GetByName(packageName);
            if (package != null)
            {
                ReloadPackage(package);
                return package;
            }
            
            return UIPackage.AddPackage(packageName, GetResourceLoader(packageName));
        }

        public void OnBranchChanged(UIPackage package, string fromBranch, string toBranch)
        {
            if (string.IsNullOrEmpty(fromBranch))
                return;
            
            // 分支改变时，修改分支相应的引用计数
            var packageName = package.name;
            if (_loadedPackages.TryGetValue(packageName, out var dict))
            {
                var branchBundle = GetBundleName(packageName, fromBranch);
                if (dict.Remove(branchBundle))
                    ResourcesManager.Instance.ReleaseBundle(branchBundle);
            }
        }

        public void ReloadPackage(UIPackage package)
        {
            package.ReloadAssets(null);
        }

        public virtual void ReleasePackage(UIPackage package, bool remove = false)
        {
            if (remove) // fix error: RemovePackage will call NTexture Dispose, it's necessary before bundle unload
                UIPackage.RemovePackage(package.id);
            
            var pkgName = package.name;
            if (_loadedPackages.TryGetValue(pkgName, out var dict))
            {
                var manager = ResourcesManager.Instance;
                foreach (var bundleName in dict.Keys)
                    manager.ReleaseBundle(bundleName);

                dict.Clear();
            }
        }
    }
}