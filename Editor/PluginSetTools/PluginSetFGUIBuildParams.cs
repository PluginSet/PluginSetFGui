using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEngine;

namespace PluginSet.FGUI.Editor
{
    [BuildChannelsParams("FGUI", -900, "FGUI集成相关")]
    public class PluginSetFGUIBuildParams: ScriptableObject
    {
        [Tooltip("开启场景中包含的FairyGUI资源包，这些包不会在重启游戏时释放")]
        public string[] packagesInStartScene;
        
        [Tooltip("是否启用Spine")]
        public bool EnableSpine;

        [Tooltip("是否启动龙骨")]
        public bool EnableDragon;

        [Tooltip("是否启用TMPro")]
        public bool EnableTMPro;

        [Tooltip("运行时，是否将被点击的窗口提到最上层")]
        public bool bringWindowToFrontOnClick = false;

    }
    
}