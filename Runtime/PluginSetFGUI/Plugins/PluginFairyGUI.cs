using System.Collections;
using System.Linq;
using FairyGUI;
using PluginSet.Core;
using UnityEngine;

namespace PluginSet.FGUI
{
    [PluginRegister]
    public class PluginFairyGUI: PluginBase, IStartPlugin
    {
        public override string Name => "FairyGUI";

        public int StartOrder => PluginsStartOrder.VirtualMachine + 1;
        
        public bool IsRunning { get; private set; }

        private string[] _packagesInStartScene;

        protected override void Init(PluginSetConfig config)
        {
            var setting = config.Get<PluginFairyGuiConfig>();

            var fguiObject = new GameObject("FairyGUI");
            fguiObject.SetActive(false);
            
            FairyGUIUtils.OnPackageLoadedHandle += UIObjectPool.OnPackageLoaded;
            FairyGUIUtils.OnPackageUnloadHandle += UIObjectPool.OnPackageUnload;

            var uiConfig = fguiObject.AddComponent<UIConfig>();
            uiConfig.Items = setting.UIConfigItems;

            var uiScaler = fguiObject.AddComponent<UIContentScaler>();
            uiScaler.scaleMode = setting.scaleMode;
            uiScaler.screenMatchMode = setting.screenMatchMode;
            uiScaler.designResolutionX = setting.designResolutionX;
            uiScaler.designResolutionY = setting.designResolutionY;
            uiScaler.fallbackScreenDPI = setting.fallbackScreenDPI;
            uiScaler.defaultSpriteDPI = setting.defaultSpriteDPI;
            uiScaler.constantScaleFactor = setting.constantScaleFactor;
            uiScaler.ignoreOrientation = setting.ignoreOrientation;
            
            fguiObject.SetActive(true);
            
            UIObjectFactory.SetListExtension(typeof(GCustomList));
            UIObjectFactory.SetLoaderExtension(typeof(GCustomLoader));
            UIObjectFactory.SetLoader3DExtension(typeof(GCustomLoader3D));
            
            UIConfig.bringWindowToFrontOnClick = setting.bringWindowToFrontOnClick;

            _packagesInStartScene = setting.packagesInStartScene;
        }
        
        public IEnumerator StartPlugin()
        {
            IsRunning = true;
            yield break;
        }

        public void DisposePlugin(bool isAppQuit = false)
        {
            IsRunning = false;

            if (isAppQuit) return;
            
            UIManager.CloseAll();
            UIManager.Purge();
            UIWindow.ClearInstances();
            TweenManager.KillAllTweens();
            UIObjectPool.ClearAll();
            UIObjectFactory.Clear();
            UnionPackage.ReleaseAll(true);

            if (_packagesInStartScene == null || _packagesInStartScene.Length <= 0)
            {
                UIPackage.RemoveAllPackages();
            }
            else
            {
                var allPackages = UIPackage.GetPackages().Select(pkg => pkg.name).ToArray();
                foreach (var pkg in allPackages)
                {
                    if (!_packagesInStartScene.Contains(pkg))
                        UIPackage.RemovePackage(pkg);
                }
            }
        }
    }
}