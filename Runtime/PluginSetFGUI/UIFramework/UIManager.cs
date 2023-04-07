using System;
using System.Collections.Generic;
using FairyGUI;
using PluginSet.Core;
using UnityEngine;
using Logger = PluginSet.Core.Logger;

namespace PluginSet.FGUI
{
    public static class UIManager
    {
        private static readonly Logger Logger = LoggerManager.GetLogger("UIManager");
        
        private struct WindowInfo
        {
            public Type WindowType;
            public bool IsSingleton;
            public int Tag;
        }

        private static readonly Dictionary<string, WindowInfo> _windowInfos = new Dictionary<string, WindowInfo>();
        
        private static List<UILayer> _layers = new List<UILayer>();

        public static readonly int ViewLayerOrder = 0;
        public static readonly int ModalLayerOrder = 1000;
        public static readonly int ToastLayerOrder = 2000;
        public static readonly int LoadingLayerOrder = 3000;

        public static UILayer LayerView;
        public static UILayer LayerModal;
        public static UILayer LayerToast;
        public static UILayer LayerLoading;

        private static bool _inited = false;
        private static string _defaultToastUrl;

        private static int _viewRotation = 0;
        
        private static void Init()
        {
            if (_inited) return;
            
            _inited = true;
            LayerView = AddLayer("LayerView", ViewLayerOrder);
            LayerModal = AddLayer("LayerModal", ModalLayerOrder);
            LayerToast = AddLayer("LayerToast", ToastLayerOrder);
            LayerLoading = AddLayer("LayerLoading", LoadingLayerOrder);
        }

        public static void InitWithSafeArea(float x, float y, float w, float h)
        {
            UILayer.SetSafeArea(x, y, w, h);
            foreach (var layer in _layers)
            {
                layer.ApplySafeArea();
            }
            
            Init();
        }

        /// <summary>
        /// rotate stage view
        /// </summary>
        /// <param name="rotation">90 * n</param>
        public static void ApplyViewRotation(int rotation)
        {
            if (_viewRotation == rotation)
                return;

            _viewRotation = rotation;
            Stage.inst.rotation = rotation;
            var transform = StageCamera.main.transform;
            var localPosition = transform.localPosition;
            var scaleX = 1;
            var scaleY = 1;
            var index = (rotation / 90) % 4;
            if (index < 0)
                index += 4;
            if (index >= 1 && index <= 2)
                scaleX = -1;
            if (index > 1)
                scaleY = -1;
            
            transform.localPosition = new Vector3(localPosition.x * scaleX, localPosition.y * scaleY, localPosition.z);
        }

        public static void Purge()
        {
            LayerView = null;
            LayerModal = null;
            LayerToast = null;
            LayerLoading = null;

            foreach (var layer in _layers)
            {
                layer.RemoveChildren(0, -1, true);
                layer.Dispose();
            }
            _layers.Clear();
            _inited = false;
        }

        public static void SetDefaultToastUrl(string url)
        {
            _defaultToastUrl = url;
        }
        
        public static void SetDefaultToastUrl<T>(string url) where T: UIToast
        {
            UIObjectFactory.SetPackageItemExtension(url, typeof(T));
            _defaultToastUrl = url;
        }

        public static UILayer AddLayer(string name, int sortingOrder = 0)
        {
            var layer = new UILayer(name, sortingOrder);
            _layers.Add(layer);
            return layer;
        }

        public static void RegisterWindow<T>(string name, bool isSingleton = true, int tag = 0) where T: UIWindow
        {
            if (_windowInfos.ContainsKey(name))
            {
                Logger.Warn("Repeated window name {0} with type {1}", name, typeof(T).FullName);
                return;
            }
            
            _windowInfos.Add(name, new WindowInfo()
            {
                WindowType = typeof(T),
                IsSingleton = isSingleton,
                Tag = tag,
            });
        }

        public static UIWindow FindWindow(string name)
        {
            return UIWindow.GetInstance(name);
        }

        public static bool IsWindowShown(string name, bool isStrict = true)
        {
            var window = FindWindow(name);
            if (window == null)
                return false;

            return !isStrict || window.IsShown;
        }

        public static UIWindow ShowOn(UILayer layer, string name, params object[] args)
        {
            if (!_windowInfos.TryGetValue(name, out var info))
            {
                Logger.Error("Cannot find window info with name {0}", name);
                return null;
            }

            UIWindow window = null;
            if (info.IsSingleton)
            {
                window = UIWindow.GetInstance(name);
            }

            if (window == null)
            {
                window = (UIWindow) Activator.CreateInstance(info.WindowType, args);
                window.name = name;
                window.Tag = info.Tag;
                UIWindow.AddInstance(name, window);
                window.ShowOn(layer);
            }
            else
            {
                window.SetData(args);
                window.BringToFront();
            }

            return window;
        }

        public static UIWindow ShowView(string name, params object[] args)
        {
            return ShowOn(LayerView, name, args);
        }

        public static UIWindow ShowModal(string name, params object[] args)
        {
            return ShowOn(LayerModal, name, args);
        }
        
        public static UIWindow ShowLoading(string name, params object[] args)
        {
            return ShowOn(LayerLoading, name, args);
        }

        public static void CreateToastRule(string name, float defaultStaySeconds, bool outImmediatelyWhenNew, bool enableShowWhenLastOut)
        {
            UIToastShowRule.AddRule(name, new UIToastShowRuleDefault(defaultStaySeconds, outImmediatelyWhenNew, enableShowWhenLastOut));
        }

        public static UIToast ShowToast(string text, string icon = null, string url = null, int tag = 0)
        {
            if (string.IsNullOrEmpty(url))
                url = _defaultToastUrl;
            
            return ShowToast(text, icon, url, tag, url);
        }
        
        public static UIToast ShowToast(string text, string icon, string url, int tag, string rule, int sortingOrder = 0)
        {
            if (string.IsNullOrEmpty(url))
                url = _defaultToastUrl;

            var toast = UIObjectPool.Instance.Get(url) as UIToast;
            if (toast == null)
                return null;
            
            toast.title = text;
            if (!string.IsNullOrEmpty(icon))
                toast.icon = icon;

            toast.Tag = tag;
            toast.sortingOrder = sortingOrder;
            toast.SetRule(UIToastShowRule.GetRule(rule));
            
            LayerToast.AddChild(toast);
            return toast;
        }

        public static void HideAllToast()
        {
            LayerToast.RemoveChildren();
        }

        public static void HideToast(int tag)
        {
            foreach (var obj in LayerToast.GetChildren())
            {
                if (obj is UIToast toast)
                    if (toast.Tag == tag)
                        toast.Hide();
            }
        }

        public static void CloseToast(int tag)
        {
            foreach (var obj in LayerToast.GetChildren())
            {
                if (obj is UIToast toast)
                    if (toast.Tag == tag)
                        toast.Close();
            }
        }
        
        public static void ShowModalWait()
        {
            LayerLoading.ShowModalWait();
        }

        public static void HideModalWait()
        {
            LayerLoading.CloseModalWait();
        }
        
        public static void HideAllView()
        {
            HideAll(LayerView);
        }
        
        public static void HideAllModal()
        {
            HideAll(LayerModal);
        }
        
        public static void HideAllLoading()
        {
            HideAll(LayerLoading);
        }

        
        public static void CloseAllView()
        {
            CloseAll(LayerView);
        }
        
        public static void CloseAllModal()
        {
            CloseAll(LayerModal);
        }
        
        public static void CloseAllLoading()
        {
            CloseAll(LayerLoading);
        }

        public static void HideAll(UILayer layer)
        {
            HideAllWin(win => win.parent == layer);
        }
        
        public static void CloseAll()
        {
            CloseAllWin(val => true);
        }

        public static void CloseAll(UILayer layer)
        {
            CloseAllWin(win => win.parent == layer);
        }
        
        public static void HideAllView(int tag)
        {
            HideAllWin(win => tag.Equals(win.data) && win.parent == LayerView);
        }
        
        public static void HideAllModal(int tag)
        {
            HideAllWin(win => tag.Equals(win.data) && win.parent == LayerModal);
        }
        
        public static void HideAllToast(int tag)
        {
            HideAllWin(win => tag.Equals(win.data) && win.parent == LayerToast);
        }
        
        public static void HideAllLoading(int tag)
        {
            HideAllWin(win => tag.Equals(win.data) && win.parent == LayerLoading);
        }
        
        public static void CloseAllView(int tag)
        {
            CloseAllWin(win => tag.Equals(win.data) && win.parent == LayerView);
        }
        
        public static void CloseAllModal(int tag)
        {
            CloseAllWin(win => tag.Equals(win.data) && win.parent == LayerModal);
        }
        
        public static void CloseAllToast(int tag)
        {
            CloseAllWin(win => tag.Equals(win.data) && win.parent == LayerToast);
        }
        
        public static void CloseAllLoading(int tag)
        {
            CloseAllWin(win => tag.Equals(win.data) && win.parent == LayerLoading);
        }

        public static void HideAllWindow(string name)
        {
            HideAllWin(win => win.name.Equals(name));
        }

        public static void HideAllWindow(int tag)
        {
            HideAllWin(win => tag.Equals(win.data));
        }

        public static void CloseAllWindow(string name)
        {
            CloseAllWin(win => win.name.Equals(name));
        }

        public static void CloseAllWindow(int tag)
        {
            CloseAllWin(win => tag.Equals(win.data));
        }

        private static void HideAllWin(Func<Window, bool> match)
        {
            foreach (var layer in _layers)
            {
                var children = layer._children;
                var childCount = children.Count;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    if (children.Count <= i)
                        continue;
                    if (children[i] is Window win)
                        if (match.Invoke(win))
                            win.Hide();
                }
            }
        }
        
        private static void CloseAllWin(Func<Window, bool> match)
        {
            foreach (var layer in _layers)
            {
                var children = layer._children;
                var childCount = children.Count;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    if (children.Count <= i)
                        continue;
                    if (children[i] is Window win)
                        if (match.Invoke(win))
                            win.HideImmediately();
                }
            }
        }
    }
}