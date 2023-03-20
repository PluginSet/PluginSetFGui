using PluginSet.Core.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PluginSet.FGUI.Editor
{
    [InitializeOnLoad]
    public class SpineLibImportedChecker
    {
        static SpineLibImportedChecker()
        {
            if (Application.isBatchMode)
                return;
            
            var buildChannels = EditorSetting.CurrentBuildChannel;
            var buildParams = buildChannels.Get<PluginSetFGUIBuildParams>();
            if (buildParams && buildParams.EnableSpine)
            {
                FairyGUIBuildTools.CheckSpineLibImported();
            }
        }
    }
}