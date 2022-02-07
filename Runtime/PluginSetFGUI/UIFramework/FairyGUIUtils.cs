using System;
using System.Diagnostics;
using FairyGUI;
using LuaInterface;
using PluginSet.Core;
using UnityEngine;

namespace PluginSet.FGUI
{
    public static class FairyGUIUtils
    {
        private static IPackageLoader _packageLoader;

        public delegate void OnPackageLoadedOrUnload(string packageName);
        
        public static void AddOnBranchChange(this GObject gObject, EventCallback0 callback)
        {
            gObject.AddEventListener(UnionPackage.EVENT_ON_BRANCH_CHANGE, callback);
        }
        
        public static void AddOnBranchChange(this GObject gObject, EventCallback1 callback)
        {
            gObject.AddEventListener(UnionPackage.EVENT_ON_BRANCH_CHANGE, callback);
        }
        
        public static void RemoveOnBranchChange(this GObject gObject, EventCallback0 callback)
        {
            gObject.RemoveEventListener(UnionPackage.EVENT_ON_BRANCH_CHANGE, callback);
        }
        
        public static void RemoveBranchChange(this GObject gObject, EventCallback1 callback)
        {
            gObject.RemoveEventListener(UnionPackage.EVENT_ON_BRANCH_CHANGE, callback);
        }

        public static void SetPackageLoad(IPackageLoader loader)
        {
            _packageLoader = loader;
        }

        public static UISource CreatePackageSource(string packageName)
        {
            CheckPackageLoader();
            return new UISource(_packageLoader.PreparePackage, packageName);
        }

        public static AsyncOperationHandle PreparePackages(params string[] packageNames)
        {
            var count = packageNames.Length;
            if (count <= 0) 
                return AsyncNonResultHandle<object>.Default;
            
            CheckPackageLoader();
            var handles = new AsyncOperationHandle[count];
            for (int i = 0; i < count; i++)
            {
                handles[i] = _packageLoader.PreparePackage(packageNames[i]);
            }
            
            return new MultiAsyncOperations(handles);
        }

#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public static event OnPackageLoadedOrUnload OnPackageLoadedHandle;
#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public static event OnPackageLoadedOrUnload OnPackageUnloadHandle;

        public static UIPackage LoadPackage(string packageName)
        {
            CheckPackageLoader();
            var pkg = _packageLoader.LoadPackage(packageName);
            OnPackageLoadedHandle?.Invoke(packageName);
            return pkg;
        }

        public static void ReloadPackage(UIPackage package)
        {
            CheckPackageLoader();
            _packageLoader.ReloadPackage(package);
        }

        public static void ReleasePackage(UIPackage package, bool remove = false)
        {
            CheckPackageLoader();
            if (remove)
                OnPackageUnloadHandle?.Invoke(package.name);
            _packageLoader.ReleasePackage(package, remove);
        }

        public static void SetDefaultFont(string fontName)
        {
            if (fontName.Equals(UIConfig.defaultFont))
                return;

            UIConfig.defaultFont = fontName;
            ApplyTextFieldFont(Stage.inst);
        }

        private static void ApplyTextFieldFont(DisplayObject parent)
        {
            if (parent == null)
                return;

            if (parent is TextField)
            {
                ((TextField) parent).ApplyFormat();
            }
            else if (parent is Container)
            {
                var children = ((Container) parent).GetChildren();
                foreach (var child in children)
                {
                    ApplyTextFieldFont(child);
                }
            }
        }

        public static void MakeFullScreen(GObject target)
        {
            var parent = target.parent;
            if (parent == null)
                return;

            var root = GRoot.inst;
            Vector2 leftUp = new Vector2(0, 0);
            leftUp = root.LocalToGlobal(leftUp);
            leftUp = parent.GlobalToLocal(leftUp);
            
            Vector2 rightBottom = new Vector2(root.width, root.height);
            rightBottom = root.LocalToGlobal(rightBottom);
            rightBottom = parent.GlobalToLocal(rightBottom);

            var width = rightBottom.x - leftUp.x;
            var height = rightBottom.y - leftUp.y;
            target.SetSize(width, height);
            
            if (target.pivotAsAnchor)
            {
                leftUp.x += width * target.pivotX;
                leftUp.y += height * target.pivotY;
            }
            target.SetXY(leftUp.x, leftUp.y);
        }

        [Conditional("UNITY_EDITOR")]
        private static void CheckPackageLoader()
        {
            if (_packageLoader == null)
                throw new Exception("Please set PackageLoader first!");
        }
    }
}