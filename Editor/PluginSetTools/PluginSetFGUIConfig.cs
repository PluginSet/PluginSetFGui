using System.Collections.Generic;
using FairyGUI;
using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEngine;

namespace PluginSet.FGUI.Editor
{
    [BuildTools]
    [BuildChannelsParams("FGUIConfig", -901, "FairyGUI UIConfig配置")]
    public class PluginSetFGUIConfig: ScriptableObject
    {
        [Tooltip("FairyGUI配置")]
        public List<UIConfig.ConfigValue> Items = new List<UIConfig.ConfigValue>();
        
        /// <summary>
        /// 
        /// </summary>
        public UIContentScaler.ScaleMode scaleMode;

        /// <summary>
        /// 
        /// </summary>
        public UIContentScaler.ScreenMatchMode screenMatchMode;

        /// <summary>
        /// 
        /// </summary>
        public int designResolutionX;

        /// <summary>
        /// 
        /// </summary>
        public int designResolutionY;

        /// <summary>
        /// 
        /// </summary>
        public int fallbackScreenDPI = 96;

        /// <summary>
        /// 
        /// </summary>
        public int defaultSpriteDPI = 96;

        /// <summary>
        /// 
        /// </summary>
        public float constantScaleFactor = 1;

        /// <summary>
        /// 当false时，计算比例时会考虑designResolutionX/Y的设置是针对横屏还是竖屏。否则不考虑。
        /// </summary>
        public bool ignoreOrientation = false;

        [OnSyncEditorSetting]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
            var buildParams = context.BuildChannels.Get<PluginSetFGUIConfig>();
            
            var pluginConfig = context.Get<PluginSetConfig>("pluginsConfig");
            var config = pluginConfig.AddConfig<PluginFairyGuiConfig>("FGUI");
            config.UIConfigItems = buildParams.Items;
            config.scaleMode = buildParams.scaleMode;
            config.screenMatchMode = buildParams.screenMatchMode;
            config.designResolutionX = buildParams.designResolutionX;
            config.designResolutionY = buildParams.designResolutionY;
            config.fallbackScreenDPI = buildParams.fallbackScreenDPI;
            config.defaultSpriteDPI = buildParams.defaultSpriteDPI;
            config.constantScaleFactor = buildParams.constantScaleFactor;
            config.ignoreOrientation = buildParams.ignoreOrientation;
        }
    }
}