using System;
using FairyGUI;

namespace PluginSet.FGUI
{
    public class UIModal: UIWindow
    {
        public static string ModalContentUrl = null;

        public override GComponent Component => _loader.component;

        private readonly string _contentUrl = null;
        private GLoader _loader = null;
        private bool _closeWhenClickFrame = false;
        private GComponent _modalFrame = null;
        
        public UIModal(string url
            , string[] dependentPackages
            , bool asyncOpen
            , params object[] args) : base(ModalContentUrl, dependentPackages, asyncOpen, args)
        {
            if (string.IsNullOrEmpty(ModalContentUrl))
                throw new Exception("Please set ModalContentUrl first!");
            _contentUrl = url;
            modal = true;
        }

        protected override void OnCreate()
        {
            _loader = contentPane.GetChild("content") as GLoader;
            if (_loader == null)
                throw new Exception("ModalContent need a Loader with name \"content\"!");
            _loader.url = _contentUrl;
            OnOffClickFrame(_closeWhenClickFrame);
            _modalFrame = contentPane.GetChild("frame") as GComponent;
            
            var contentFrame = _loader.component?.GetChild("frame") as GComponent;
            if (contentFrame != null)
            {
                this.closeButton = contentFrame.GetChild("closeButton");
                this.dragArea = contentFrame.GetChild("dragArea");
                this.contentArea = contentFrame.GetChild("contentArea");
            }
            
            SetSize(root.width, root.height);
            base.OnCreate();
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            var cmp = _loader.component;
            if (cmp != null)
            {
                ((Container) _loader.displayObject).RemoveChild(cmp.container);
                _loader.url = null;
            }
        }

        public void CloseWhenClickFrame(bool close)
        {
            if (_closeWhenClickFrame == close)
                return;

            _closeWhenClickFrame = close;
            OnOffClickFrame(close);
        }

        private void OnOffClickFrame(bool close)
        {
            if (_modalFrame == null)
                return;
                
            if (close)
                OnClick(_modalFrame, closeEventHandler);
            else
                OffClick(_modalFrame, closeEventHandler);
        }
    }
}