using System.Collections.Generic;
using System.IO;
using System.Linq;
using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PluginSet.FGUI.Editor
{
    [BuildTools]
    public static class FairyGUIBuildTools
    {
        [OnSyncEditorSetting]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
//            context.Symbols.Add("FAIRYGUI_PUERTS");
            context.AddLinkAssembly("PluginSet.FairyGUI");
            
            var buildParams = context.BuildChannels.Get<PluginSetFGUIBuildParams>("FGUI");
            if (buildParams.EnableSpine)
                if (CheckSpineLibImported())
                    context.Symbols.Add("FAIRYGUI_SPINE");
                
            if (buildParams.EnableDragon)
                context.Symbols.Add("FAIRYGUI_DRAGONBONES");
            
            if (buildParams.EnableTMPro)
                context.Symbols.Add("FAIRYGUI_TMPRO");
            
            if (buildParams.DisableWarpWorlds)
                context.Symbols.Add("DISABLE_WARP_WORLD");
        
            var pluginConfig = context.Get<PluginSetConfig>("pluginsConfig");
            var config = pluginConfig.Get<PluginFairyGuiConfig>("FGUI");
            config.packagesInStartScene = buildParams.packagesInStartScene;
            config.bringWindowToFrontOnClick = buildParams.bringWindowToFrontOnClick;
        }
        
        public static bool CheckSpineLibImported()
        {
            var spineLibName = "com.esotericsoftware.spine.spine-unity";
            var spineGitAddress = "https://github.com/LyneXiao/Spine-Runtime-Unity.git";
            return Global.CheckGitLibImported(spineLibName, spineGitAddress);
        }
    }
}