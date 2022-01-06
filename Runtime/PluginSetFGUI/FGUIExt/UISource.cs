using FairyGUI;
using PluginSet.Core;

namespace PluginSet.FGUI
{
    public class UISource: IUISource
    {
        public delegate AsyncOperationHandle LoadFunc(string fileName);

        private string _fileName = null;

        private bool _loaded = false;

        public bool loaded
        {
            get
            {
                return _loaded;
            }
        }

        public string fileName
        {
            get
            {
                return _fileName;
            }
            set 
            {
                _fileName = value;
            }
        }

        private LoadFunc loaderFunc;
        private UILoadCallback loadedCallback = null; 
        
        
        public UISource(LoadFunc loadFunc)
            :this(loadFunc, string.Empty)
        {
        }
        
        public UISource(LoadFunc loadFunc, string file)
        {
            loaderFunc  = loadFunc;
            _fileName   = file;
        }

        public void Load(UILoadCallback callback)
        {
            loadedCallback = callback;
            var handle = loaderFunc.Invoke(_fileName);
            handle.completed += Completed;
        }

        private void Completed(AsyncOperationHandle handle)
        {
            handle.completed -= Completed;
            
            if (_loaded) return;
            _loaded = true;
            loadedCallback?.Invoke();
        }
    }
}