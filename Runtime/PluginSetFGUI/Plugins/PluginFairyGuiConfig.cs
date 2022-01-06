using System.Collections.Generic;
using FairyGUI;
using PluginSet.Core;
using UnityEngine;

namespace PluginSet.FGUI
{
    [PluginSetConfig("FGUI")]
    public class PluginFairyGuiConfig: ScriptableObject
    {
        public string[] packagesInStartScene;
        
        public bool bringWindowToFrontOnClick = false;
        
        public List<UIConfig.ConfigValue> UIConfigItems = new List<UIConfig.ConfigValue>();
        
        /// <summary>
        /// 
        /// </summary>
        public FairyGUI.UIContentScaler.ScaleMode scaleMode;

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
    }
}