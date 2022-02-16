using System.Collections.Generic;
using System.IO;
using System.Linq;
using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.FGUI.Editor
{
    [BuildTools]
    public static class FairyGUIBuildTools
    {
        [UnityEditor.Callbacks.PostProcessScene]
        public static void BeforeAppRun()
        {
            BuildChannels buildChannels;
            var context = BuildProcessorContext.Current;
            if (context != null)
                buildChannels = context.BuildChannels;
            else
                buildChannels = EditorSetting.CurrentBuildChannel;
            
            var buildParams = buildChannels.Get<PluginSetFGUIBuildParams>("FGUI");
            if (buildParams.EnableDragon && buildParams.FairyGUIPaths != null)
            {
                foreach (var path in buildParams.FairyGUIPaths)
                {
                    CheckAndCreateBoneDatas(path);
                }
            }
        }
        
        public static void CheckAndCreateBoneDatas(string fguiPath)
        {
            if (string.IsNullOrEmpty(fguiPath))
                return;

#if FAIRYGUI_DRAGONBONES
            DragonBones.UnityEditor.CreateUnityDatas(Path.Combine(Application.dataPath, "..", fguiPath));
#endif
#if FAIRYGUI_SPINE
#endif
            AssetDatabase.Refresh();
        }

        [OnSyncEditorSetting]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
//            context.Symbols.Add("FAIRYGUI_PUERTS");
            context.AddLinkAssembly("PluginSet.FairyGUI");
            
            var buildParams = context.BuildChannels.Get<PluginSetFGUIBuildParams>("FGUI");
            if (buildParams.EnableSpine)
                context.Symbols.Add("FAIRYGUI_SPINE");
                
            if (buildParams.EnableDragon)
                context.Symbols.Add("FAIRYGUI_DRAGONBONES");
            
            if (buildParams.EnableTMPro)
                context.Symbols.Add("FAIRYGUI_TMPRO");

            if (buildParams.EnableDragon && buildParams.FairyGUIPaths != null)
            {
                foreach (var path in buildParams.FairyGUIPaths)
                {
                    CheckAndCreateBoneDatas(path);
                }
            }
            
            var pluginConfig = context.Get<PluginSetConfig>("pluginsConfig");
            var config = pluginConfig.Get<PluginFairyGuiConfig>("FGUI");
            config.packagesInStartScene = buildParams.packagesInStartScene;
            config.bringWindowToFrontOnClick = buildParams.bringWindowToFrontOnClick;
        }

        [OnBuildBundles(int.MinValue)]
        [OnBuildPatches(int.MinValue)]
        public static void PrepareFairyPaths(BuildProcessorContext context)
        {
            var fairyGuiPaths = context.TryGet<List<string>>("fairyGuiPaths", null);
            if (fairyGuiPaths != null)
                return;
            
            var buildParams = context.BuildChannels.Get<PluginSetFGUIBuildParams>("FGUI");
            if (!buildParams.AutoSetFairyBundles)
                return;
            
            if (buildParams.FairyGUIPaths == null || buildParams.FairyGUIPaths.Length <= 0)
                return;
            
            context.Set<List<string>>("fairyGuiPaths", buildParams.FairyGUIPaths.ToList());
        }

        [OnBuildBundles]
        [OnBuildPatches]
        public static void BeforeBuildBundles(BuildProcessorContext context)
        {
            var fairyGuiPaths = context.TryGet<List<string>>("fairyGuiPaths", null);
            if (fairyGuiPaths == null)
                return;

            List<string> buildPaths = BuildGlobal.CollectBuildPaths(context, fairyGuiPaths);
            if (buildPaths == null || buildPaths.Count <= 0)
                return;
            
            var buildParams = context.BuildChannels.Get<PluginSetFGUIBuildParams>("FGUI");
            var bundleNameFormatter = buildParams.BundleNameFormatter;
            var rootPath = Path.Combine(Application.dataPath, "..");
            foreach (var buildPath in buildPaths)
            {
                FairyGUIPathImport.WalkDirectory(Path.Combine(rootPath, buildPath), bundleNameFormatter,
                    delegate(string file, string bundleName)
                    {
                        context.AddBuildBundle(bundleName, file.ToLower());
                    });
            }
        }
    }
}