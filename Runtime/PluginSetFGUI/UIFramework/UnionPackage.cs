using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

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

        private static Dictionary<string, string> _currentBranchMap = EmptyMap;
        private static List<string> _allBranchPackages = new List<string>();

        private static string _defaultBranch = string.Empty;

        public const string EVENT_ON_BRANCH_CHANGE = "onBranchChange";

        public static void SetDefaultBranch(string defaultBranch)
        {
            _defaultBranch = defaultBranch;
        }

        public static void SetCurrentBranch(string branch)
        {
            if (branch == _defaultBranch)
                branch = string.Empty;
            
            if (_unionBranches.TryGetValue(branch, out var map))
            {
                _currentBranchMap = map;
            }
            else
            {
                _currentBranchMap = EmptyMap;
            }

            // 先设置UIPackage.branch再调用ApplyPackageBranch，ApplyPackageBranch方法依赖UIPackage.branch的当前值
            UIPackage.branch = branch;
            
            foreach (var pkg in _loadedPackages.Values)
            {
                ApplyPackageBranch(pkg);
            }
            
            Stage.inst.BroadcastEvent(EVENT_ON_BRANCH_CHANGE, branch);
            UIObjectPool.ClearAll();
        }

        public static void AddUnionBranch(string branch, string mainPackageName, string branchPackageName)
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

        public static bool IsUnionPackageLoaded(string name)
        {
            return _loadedPackages.ContainsKey(name);
        }

        public static bool RetainIfLoaded(string name)
        {
            if (_loadedPackages.TryGetValue(name, out var loaded))
            {
                loaded.Retain();
                return true;
            }

            return false;
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

        public static void ReleaseAll(bool removePackages, bool skipDontRelease = false)
        {
            List<UnionPackage> notReleased = new List<UnionPackage>();
            foreach (var pkg in _loadedPackages.Values)
            {
                if (skipDontRelease && pkg.dontRelease)
                {
                    notReleased.Add(pkg);
                    continue;
                }
                
                pkg.ReleasePackages(removePackages);
            }
            
            _loadedPackages.Clear();
            foreach (var pkg in notReleased)
            {
                _loadedPackages.Add(pkg._name, pkg);
            }
        }

        private static void ApplyPackageBranch(UnionPackage package)
        {
            if (_currentBranchMap.TryGetValue(package._name, out var branchPackageName))
            {
                package.SetBranchPackage(UIPackage.branch, branchPackageName);
            }
            else
            {
                package.SetBranchPackage(UIPackage.branch);
            }
        }
        
        private bool _isMainLoaded;
        private string _name;
        private UIPackage _mainPackage;

        private string _branch;
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
        }

        private void SetBranchPackage(string branch)
        {
            SetBranchPackageInternal(branch, null);
            FairyGUIUtils.ReloadPackage(_mainPackage);
            _mainPackage.SetBranch(branch);
        }

        private void SetBranchPackage(string branch, string packageName)
        {
            var branchPackage = FairyGUIUtils.LoadPackage(packageName);
            var isFull = SetBranchPackageInternal(branch, branchPackage);
            if (isFull && _isMainLoaded)
            {
                FairyGUIUtils.ReleasePackage(_mainPackage);
                FairyGUIUtils.ReloadPackage(_branchPackage);
                _isMainLoaded = false;
            }
            else if (!isFull && !_isMainLoaded)
            {
                FairyGUIUtils.ReloadPackage(_mainPackage);
                FairyGUIUtils.ReloadPackage(_branchPackage);
                _isMainLoaded = true;
            }
            _mainPackage.SetBranch(branch);
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
                ReleasePackages(true);
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

        public void ChangeBranch(string branch)
        {
            FairyGUIUtils.OnPackageBranchChanged(_mainPackage, _branch, branch);
            _branch = branch;
        }

        private void ClearAllBranchAt(int index)
        {
            foreach (var item in _mainPackage.GetItems())
            {
                if (!string.IsNullOrEmpty(item.name))
                {
                    ClearItemBranchAt(item, index);
                }
            }
        }

        private void UpdateItemBranchAt(PackageItem item, string itemId, int index)
        {
            if (item.branches == null)
            {
                var branches = new string[index + 1];
                branches[index] = itemId;
                item.branches = branches;
            }
            else
            {
                var branches = item.branches;
                var count = branches.Length;
                if (count > index)
                {
                    branches[index] = itemId;
                }
                else
                {
                    var newBranches = new string[index + 1];
                    Array.Copy(branches, newBranches, count);
                    newBranches[index] = itemId;
                    item.branches = newBranches;
                }
            }
        }

        private void ClearItemBranchAt(PackageItem item, int index)
        {
            var branches = item.branches;
            if (branches != null && branches.Length > index)
                branches[index] = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="branchPackage"></param>
        /// <returns>分支包是否包含所有资源</returns>
        private bool SetBranchPackageInternal(string branch, UIPackage branchPackage)
        {
            ClearVirtual();

            if (branch != _branch)
            {
                if (!string.IsNullOrEmpty(_branch) && _branchPackage != null)
                {
                    var oldIndex = _mainPackage.AddBranch(_branch);
                    ClearAllBranchAt(oldIndex);
                }
                ChangeBranch(branch);
            }
            
            if (_branchPackage != null)
            {
                ReleasePackage(_branchPackage, true);
                _branchPackage = null;
            }
            
            if (string.IsNullOrEmpty(branch) || branchPackage == null)
                return false;
            
            var index = _mainPackage.AddBranch(branch);
            bool isFull = true;
            foreach (var item in _mainPackage.GetItems())
            {
                if (!string.IsNullOrEmpty(item.name))
                {
                    var branchItem = branchPackage.GetItemByName(item.name);
                    if (branchItem != null && branchItem.exported)
                    {
                        UpdateItemBranchAt(item, branchItem.id, index);
                        AddVirtual(branchItem);
                    }
                    else
                    {
                        ClearItemBranchAt(item, index);
                        isFull = false;
                    }
                }
            }

            _branchPackage = branchPackage;
            
            return isFull;
        }

        private void ReleasePackage(UIPackage package, bool remove = false)
        {
            FairyGUIUtils.ReleasePackage(package, remove);
        }

        private void ReleasePackages(bool remove = false)
        {
            if (_branchPackage != null)
            {
                ReleasePackage(_branchPackage, true);
                _branchPackage = null;
            }

            if (_mainPackage != null)
            {
                ReleasePackage(_mainPackage, remove);
                _mainPackage = null;
            }
            else
            {
                Debug.LogError($"ReleasePackages: main package is null in {_name}");
            }
        }
        
        private void AddVirtual(PackageItem item)
        {
            _mainPackage.VirtualAdd(item, _branch);
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