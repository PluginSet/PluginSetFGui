using System.Collections.Generic;
using FairyGUI;

namespace PluginSet.FGUI
{
    /// <summary>
    /// 动态分支资源需要使用组合包
    /// </summary>
    public class UnionPackage
    {
        private static Dictionary<string, UnionPackage> _loadedPackages = new Dictionary<string, UnionPackage>();
        
        private static Dictionary<string, Dictionary<string, string>> _unionBranches = new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, string> EmptyMap = new Dictionary<string, string>();

        private static string _currentBranch;
        private static string[] _currentBranches;
        private static Dictionary<string, string> _currentBranchMap = EmptyMap;
        private static List<string> _allBranchPackages = new List<string>();

        private static string _defaultBranch = "DEFAULT";

        public static void SetDefaultBranch(string defaultBranch)
        {
            _defaultBranch = defaultBranch;
        }

        public static void SetCurrentBranch(string branch)
        {
            if (_currentBranch == branch)
                return;

            _currentBranch = branch;
            _currentBranches = new [] {branch};
            if (_unionBranches.TryGetValue(branch, out var map))
            {
                _currentBranchMap = map;
            }
            else
            {
                _currentBranchMap = _unionBranches[_defaultBranch] ?? EmptyMap;
            }

            foreach (var pkg in _loadedPackages.Values)
            {
                ApplyPackageBranch(pkg);
            }
        }

        public static void AddUnionBranch(string mainPackageName, string branch, string branchPackageName)
        {
            if (_unionBranches.TryGetValue(branch, out var branchMap))
            {
                branchMap[mainPackageName] = branchPackageName;
            }
            else
            {
                _unionBranches.Add(branch, new Dictionary<string, string>{{mainPackageName, branchPackageName}});
            }
            
            if (branchPackageName != mainPackageName && !_allBranchPackages.Contains(branchPackageName))
                _allBranchPackages.Add(branchPackageName);
        }

        public static bool IsBranchPackage(string packageName)
        {
            return _allBranchPackages.Contains(packageName);
        }

        public static UnionPackage RetainUnionPackage(string name)
        {
            if (_loadedPackages.TryGetValue(name, out var loaded))
            {
                loaded.Retain();
                return loaded;
            }
            
            var package = new UnionPackage(name);
            package.Retain();
            ApplyPackageBranch(package);
            _loadedPackages.Add(name, package);

            return package;
        }

        public static void ReleaseUnionPackage(UnionPackage package)
        {
            if (package.Release())
                _loadedPackages.Remove(package._name);
        }

        public static void ReleaseUnionPackage(string name)
        {
            if (_loadedPackages.TryGetValue(name, out var pkg))
            {
                ReleaseUnionPackage(pkg);
            }
        }

        public static void DontReleasePackage(string name)
        {
            var package = RetainUnionPackage(name);
            package.DontRelease();
        }

        public static void ReleaseAll(bool skipDontRelease = false)
        {
            List<UnionPackage> notReleased = new List<UnionPackage>();
            foreach (var pkg in _loadedPackages.Values)
            {
                if (skipDontRelease && pkg.dontRelease)
                {
                    notReleased.Add(pkg);
                    continue;
                }
                
                pkg.ReleasePackages();
            }
            
            _loadedPackages.Clear();
            foreach (var pkg in notReleased)
            {
                _loadedPackages.Add(pkg._name, pkg);
            }
        }

        private static void ApplyPackageBranch(UnionPackage package)
        {
            if (_currentBranchMap.TryGetValue(package._name, out var branchName))
            {
                package.SetBranchPackage(branchName);
                package.SetBranches(_currentBranches);
            }
        }
        
        private bool _isMainLoaded;
        private string _name;
        private UIPackage _mainPackage;
        
        private UIPackage _branchPackage;

        private int _retainCount = 0;
        private bool dontRelease = false;
        
        private List<PackageItem> _virtualItems = new List<PackageItem>();

        private UnionPackage(string packageName)
            :this(packageName, FairyGUIUtils.LoadPackage(packageName), true)
        {
        }
        
        private UnionPackage(string name, UIPackage mainPackage, bool isMainLoaded)
        {
            _name = name;
            _mainPackage = mainPackage;
            _isMainLoaded = isMainLoaded;
            ResetAllBranches();
        }

        private void SetBranchPackage(string packageName)
        {
            var isFull = SetBranchPackage(FairyGUIUtils.LoadPackage(packageName));
            if (isFull && _isMainLoaded)
            {
                FairyGUIUtils.ReleasePackage(_mainPackage);
                _isMainLoaded = false;
            }
            else if (!isFull && !_isMainLoaded)
            {
                FairyGUIUtils.ReloadPackage(_mainPackage);
                _isMainLoaded = true;
            }
        }

        private void Retain()
        {
            _retainCount++;
        }

        private void DontRelease()
        {
            dontRelease = true;
        }

        private bool Release()
        {
            if (dontRelease)
                return false;
            
            _retainCount--;
            
            if (_retainCount <= 0)
            {
                ReleasePackages();
                return true;
            }

            return false;
        }

        public void LoadAll()
        {
            if (_isMainLoaded)
                _mainPackage.LoadAllAssets();
            _branchPackage?.LoadAllAssets();
        }

        public void Unload()
        {
            if (_isMainLoaded)
                _mainPackage.UnloadAssets();
            _branchPackage?.UnloadAssets();
        }
        
        public void Reload()
        {
            if (_isMainLoaded)
                FairyGUIUtils.ReloadPackage(_mainPackage);
            if (_branchPackage != null)
                FairyGUIUtils.ReloadPackage(_branchPackage);
        }
        
        private void SetBranches(string[] branches)
        {
            _mainPackage.SetBranches(branches);
            _branchPackage?.SetBranches(branches);
        }

        private void ResetAllBranches()
        {
            foreach (var item in _mainPackage.GetItems())
            {
                if (item.exported && !string.IsNullOrEmpty(item.name))
                {
                    UpdateItemBranches(item, item.branches, item.id);
                }
            }
        }

        private void UpdateItemBranches(PackageItem item, string[] branches, string itemId)
        {
            if (branches == null || branches[0] != itemId)
                item.branches = new[] {itemId};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="branchPackage"></param>
        /// <returns>分支包是否包含所有资源</returns>
        private bool SetBranchPackage(UIPackage branchPackage)
        {
            ClearVirtual();

            if (_branchPackage != null)
            {
                ReleasePackage(_branchPackage);
                _branchPackage = null;
            }
            
            if (branchPackage == _mainPackage)
            {
                ResetAllBranches();
                return false;
            }
            
            bool isFull = true;
            foreach (var item in _mainPackage.GetItems())
            {
                if (item.exported && !string.IsNullOrEmpty(item.name))
                {
                    var branchItem = branchPackage.GetItemByName(item.name);
                    if (branchItem == null)
                    {
                        UpdateItemBranches(item, item.branches, item.id);
                        isFull = false;
                    }
                    else
                    {
                        UpdateItemBranches(item, item.branches, branchItem.id);
                        AddVirtual(branchItem);
                    }
                }
            }

            _branchPackage = branchPackage;
            
            return isFull;
        }

        private void ReleasePackage(UIPackage package)
        {
            FairyGUIUtils.ReleasePackage(package);
        }

        private void ReleasePackages()
        {
            if (_branchPackage != null)
            {
                ReleasePackage(_branchPackage);
                _branchPackage = null;
            }
            
            ReleasePackage(_mainPackage);
            _mainPackage = null;
        }
        
        private void AddVirtual(PackageItem item)
        {
            _mainPackage.VirtualAdd(item);
            _virtualItems.Add(item);
        }

        private void ClearVirtual()
        {
            foreach (var pi in _virtualItems)
            {
                if (pi.type == PackageItemType.Atlas)
                {
                    if (pi.texture != null)
                        pi.texture.Unload();
                }
                else if (pi.type == PackageItemType.Sound)
                {
                    if (pi.audioClip != null)
                        pi.audioClip.Unload();
                }

                _mainPackage.VirtualRemove(pi);
            }
            
            _virtualItems.Clear();
        }
    }
    
}