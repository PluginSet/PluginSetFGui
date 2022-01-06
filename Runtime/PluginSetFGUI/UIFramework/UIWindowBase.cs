using System;
using System.Collections.Generic;
using FairyGUI;

namespace PluginSet.FGUI
{
    public class UIWindowBase: Window
    {
        public Transition EnterTransition { get; set; }
        public Transition ExitTransition { get; set; }
        
        public Action<Action> EnterAction { get; set; }
        public Action<Action> ExitAction { get; set; }

        public virtual GComponent Component => contentPane;

#if FAIRYGUI_PUERTS
        public Action __beforeCreate;
        public Action __beforeHide;
#endif
        // 占用 GObject.data属性，不要再使用
        public int Tag
        {
            get => (int) data;
            set => data = value;
        }

        protected string _url;
        protected string[] _dependentPackages;

        private bool _asyncOpen;
        private bool _hided = false;

        private List<UnionPackage> _depencies;

        public UIWindowBase()
            :base()
        {
        }

        public void Constructor(string url
            , string[] dependentPackages
            , bool asyncOpen = true)
        {
            _url = url;
            _dependentPackages = dependentPackages;
            _asyncOpen = asyncOpen;

            Prepare();
        }

        protected virtual void BeforeCreate()
        {
#if FAIRYGUI_TOLUA
            CallLua("BeforeCreate");
#endif
#if FAIRYGUI_PUERTS
            if (__beforeCreate != null)
                __beforeCreate();
#endif
        }
        
        protected virtual void BeforeHide()
        {
#if FAIRYGUI_TOLUA
            CallLua("BeforeHide");
#endif
#if FAIRYGUI_PUERTS
            if (__beforeHide != null)
                __beforeHide();
#endif
        }

        protected override void OnInit()
        {
            DoCreate();
            OnCreate();
            base.OnInit();
        }

        protected virtual void OnCreate()
        {
            
        }

        protected override void DoShowAnimation()
        {
            if (EnterTransition != null)
            {
                EnterTransition.Play(OnShown);
            }
            else if (EnterAction != null)
            {
                EnterAction.Invoke(OnShown);
            }
            else
            {
                OnShown();
            }
        }

        protected override void OnShown()
        {
            _hided = false;
            DoEnter();
            base.OnShown();
        }

        protected override void OnHide()
        {
            if (!_hided)
                BeforeHide();
            _hided = true;
            base.OnHide();
            DoExit();
        }

        protected override void DoHideAnimation()
        {
            BeforeHide();
            _hided = true;
            if (ExitTransition != null)
            {
                ExitTransition.Play(HideImmediately);
            }
            else if (ExitAction != null)
            {
                ExitAction.Invoke(HideImmediately);
            }
            else
            {
                HideImmediately();
            }
        }

        public override void Dispose()
        {
            DoDispose();
            base.Dispose();
        }

        protected void Prepare()
        {
            if (_asyncOpen)
            {
                foreach (var pkgName in _dependentPackages)
                {
                    AddUISource(FairyGUIUtils.CreatePackageSource(pkgName));
                }
            }
        }

        protected virtual void DoCreate()
        {
            _depencies = new List<UnionPackage>();
            foreach (var pkgName in _dependentPackages)
            {
                if (UnionPackage.IsBranchPackage(pkgName))
                    continue;
                
                _depencies.Add(UnionPackage.RetainUnionPackage(pkgName));
            }
            
            BeforeCreate();

            var cmp = UIObjectPool.Instance.Get(_url);
            this.contentPane = (GComponent) cmp;
        }

        protected virtual void DoEnter()
        {
            
        }

        protected virtual void DoExit()
        {
            Dispose();
        }

        protected virtual void OnDispose()
        {
#if FAIRYGUI_TOLUA
            CallLua("OnDispose");
#endif
        }
        
        protected void DoDispose()
        {
            OnDispose();
            
            var cmp = this.contentPane;
            if (cmp != null && !cmp.isDisposed)
            {
                this.contentPane = null;
                UIObjectPool.Instance.Put(cmp);
            }

            foreach (var pkg in _depencies)
            {
                UnionPackage.ReleaseUnionPackage(pkg);
            }

        }
    }
}