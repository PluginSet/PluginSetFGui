using System.Collections.Generic;
using FairyGUI;

namespace PluginSet.FGUI
{
    public class UIWindow: UIWindowBase
    {
        private static Dictionary<string, List<UIWindow>> s_instances = new Dictionary<string, List<UIWindow>>();

        public static UIWindow GetInstance(string name)
        {
            if (s_instances.TryGetValue(name, out var list))
            {
                if (list.Count > 0)
                    return list[0];
            }

            return null;
        }

        public static void AddInstance(string name, UIWindow window)
        {
            if (s_instances.TryGetValue(name, out var list))
            {
                list.Add(window);
            }
            else
            {
                s_instances.Add(name, new List<UIWindow>(){ window });
            }
        }

        public static void ClearInstances()
        {
            s_instances.Clear();
        }

        private static void RemoveInstance(string name, UIWindow window)
        {
            if (s_instances.TryGetValue(name, out var list))
            {
                list.Remove(window);
            }
        }
        
        private object[] _openArgs;

        private Dictionary<GComponent, List<EventCallback0>> _bindCallback0 = new Dictionary<GComponent, List<EventCallback0>>();
        private Dictionary<GComponent, List<EventCallback1>> _bindCallback1 = new Dictionary<GComponent, List<EventCallback1>>();
        
        public UIWindow(string url, string[] dependentPackages
            , bool asyncOpen, params object[] args)
            :base()
        {
            Constructor(url, dependentPackages, asyncOpen);
            _openArgs = args;
        }

        protected override void BeforeHide()
        {
            base.BeforeHide();
            RemoveInstance(name, this);
        }

        public void SetUITransition(Transition inTrans, Transition outTrans)
        {
            EnterTransition = inTrans;
            ExitTransition = outTrans;
        }

        protected override void OnCreate()
        {
            OnUIInit();

            if (_openArgs != null)
            {
                SetData(_openArgs);
                _openArgs = null;
            }
        }

        protected virtual void OnUIInit()
        {
            
        }

        public virtual void SetData(params object[] args)
        {
            
        }

        protected override void OnDispose()
        {
            ClearCallback();
            RemoveInstance(name, this);
        }

        public void OnClick(GComponent target, EventCallback0 click)
        {
            AddCallback0(target, click);
        }

        public void OnClick(GComponent target, EventCallback1 click)
        {
            AddCallback1(target, click);
        }

        public void OffClick(GComponent target, EventCallback0 click)
        {
            RemoveCallback0(target, click);
        }

        public void OffClick(GComponent target, EventCallback1 click)
        {
            RemoveCallback1(target, click);
        }

        public void OffClick(GComponent target)
        {
            RemoveCallback0(target);
            RemoveCallback1(target);
        }

        protected void AddCallback0(GComponent target, EventCallback0 callback)
        {
            target.onClick.Add(callback);
            
            if (_bindCallback0.TryGetValue(target, out var list))
            {
                list.Add(callback);
            }
            else
            {
                _bindCallback0.Add(target, new List<EventCallback0>(){callback});
            }
        }
        
        protected void AddCallback1(GComponent target, EventCallback1 callback)
        {
            target.onClick.Add(callback);
            
            if (_bindCallback1.TryGetValue(target, out var list))
            {
                list.Add(callback);
            }
            else
            {
                _bindCallback1.Add(target, new List<EventCallback1>(){callback});
            }
        }
        
        protected void RemoveCallback0(GComponent target, EventCallback0 callback)
        {
            target.onClick.Remove(callback);
            
            if (_bindCallback0.TryGetValue(target, out var list))
            {
                list.Remove(callback);
            }
        }
        
        protected void RemoveCallback1(GComponent target, EventCallback1 callback)
        {
            target.onClick.Remove(callback);
            
            if (_bindCallback1.TryGetValue(target, out var list))
            {
                list.Remove(callback);
            }
        }
        
        protected void RemoveCallback0(GComponent target)    {
            if (_bindCallback0.TryGetValue(target, out var list))
            {
                var listener = target.onClick;
                foreach (var del in list)
                {
                    listener.Remove(del);
                }
                list.Clear();
            }
        }
        
        protected void RemoveCallback1(GComponent target)    {
            if (_bindCallback1.TryGetValue(target, out var list))
            {
                var listener = target.onClick;
                foreach (var del in list)
                {
                    listener.Remove(del);
                }
                list.Clear();
            }
        }

        protected void ClearCallback()
        {
            foreach (var kv in _bindCallback0)
            {
                var listener = kv.Key.onClick;
                foreach (var del in kv.Value)
                {
                    listener.Remove(del);
                }
            }
            _bindCallback0.Clear();
            
            foreach (var kv in _bindCallback1)
            {
                var listener = kv.Key.onClick;
                foreach (var del in kv.Value)
                {
                    listener.Remove(del);
                }
            }
            _bindCallback1.Clear();
        }
    }
}