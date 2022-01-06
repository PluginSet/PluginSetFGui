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
        
        [Tooltip("FairyGUI的资源目录")]
        [FolderDrag(null, ".")]
        public string[] FairyGUIPaths;

        [Tooltip("是否自动设置Fairy资源的Bundle名称\n（将各个子目录中的所有资源打包在相应名称的bundle中） ")]
        public bool AutoSetFairyBundles;

        [VisibleCaseBoolValue("AutoSetFairyBundles", true)]
        [Tooltip("Bundle名称依据子文件夹名的格式化方式")]
        public string BundleNameFormatter = "fgui_{0}";
        
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