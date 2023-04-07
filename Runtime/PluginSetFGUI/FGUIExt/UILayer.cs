using FairyGUI;
using UnityEngine;

namespace PluginSet.FGUI
{
    public class UILayer: GRoot
    {
        private static float _areaWith;
        private static float _areaHeight;
        private static float _scale;
        private static float _offsetX;
        private static float _offsetY;

        private static bool _calculated = false;

        /// <summary>
        /// 设置安全区域
        /// </summary>
        /// <param name="x">屏幕左方偏移</param>
        /// <param name="y">屏幕上方偏移</param>
        /// <param name="w">屏幕宽度</param>
        /// <param name="h">屏幕高度</param>
        public static void SetSafeArea(float x, float y, float w, float h)
        {
            _calculated = false;
            CalculateSafeArea(x, y, w, h);
            _calculated = true;
        }

        private static void CalculateSafeArea(float x, float y, float w, float h)
        {
            _offsetX = x;
            _offsetY = y;

            var scaler = Stage.inst.gameObject.GetComponent<UIContentScaler>();
            if (scaler == null || scaler.scaleMode != UIContentScaler.ScaleMode.ScaleWithScreenSize)
            {
                _scale = 1f;
            }
            else
            {
                var screenWith = Screen.width;
                var screenHeight = Screen.height;
                var designWith = scaler.designResolutionX;
                var designHeight = scaler.designResolutionY;
                var matchMode = scaler.screenMatchMode;

                if (scaler.ignoreOrientation && (
                        (screenWith > screenHeight && designWith < designHeight) ||
                        (screenWith < screenHeight && designWith > designHeight)))
                {
                    var swap = designHeight;
                    designHeight = designWith;
                    designWith = swap;
                }

                switch (matchMode)
                {
                    case UIContentScaler.ScreenMatchMode.MatchWidthOrHeight:
                        _scale = Mathf.Min(w / designWith, h / designHeight);
                        break;
                    case UIContentScaler.ScreenMatchMode.MatchWidth:
                        _scale = w / designWith;
                        break;
                    case UIContentScaler.ScreenMatchMode.MatchHeight:
                        _scale = h / designHeight;
                        break;
                    default:
                        _scale = 1f;
                        break;
                }
            }
            
            _areaWith = Mathf.Ceil(w / _scale);
            _areaHeight = Mathf.Ceil(h / _scale);

            UIContentScaler.scaleFactor = _scale;
            var gRoot = GRoot.inst;
            gRoot.SetXY(_offsetX, _offsetY);
            gRoot.SetSize(_areaWith, _areaHeight);
            gRoot.ApplyContentScaleFactor();
        }

        private static void CheckSafeArea()
        {
            if (_calculated)
                return;
            
            CalculateSafeArea(0, 0, Screen.width, Screen.height);
            _calculated = true;
        }
        
        public UILayer(string name, int sortingOrder = 0)
        {
            CheckSafeArea();
            
            this.name = this.rootContainer.name = this.rootContainer.gameObject.name = name;
            this.sortingOrder = sortingOrder;
            rootContainer._panelOrder = sortingOrder;
            
            ApplySafeArea();
            Stage.inst.ApplyPanelOrder(rootContainer);
        }

        public void ApplySafeArea()
        {
            this.SetXY(_offsetX, _offsetY);
            this.SetSize(_areaWith, _areaHeight);
            this.SetScale(_scale, _scale);
            this.InitModalLayer();
        }

        private void InitModalLayer()
        {
            var layer = this.modalLayer;
            var notAdd = layer.parent == null;
            if (notAdd)
                AddChild(layer);
            FairyGUIUtils.MakeFullScreen(layer);
            if (notAdd)
                RemoveChild(layer);
        }
    }
}