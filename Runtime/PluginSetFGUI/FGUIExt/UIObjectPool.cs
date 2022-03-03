using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace PluginSet.FGUI
{
    public class UIObjectPool : GObjectPool
    {
        private static Dictionary<string, UIObjectPool> _instances = new Dictionary<string, UIObjectPool>();
        private static UIObjectPool _instance;

        public static UIObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance("FairyGUIPool");
                }

                return _instance;
            }
        }

        public static UIObjectPool CreateInstance(string rootName)
        {
            if (_instances.TryGetValue(rootName, out var inst))
                return inst;
            
            var gameObject = new GameObject(rootName);
            Object.DontDestroyOnLoad(gameObject);
            var newInst = new UIObjectPool(gameObject);
            _instances.Add(rootName, newInst);
            return newInst;
        }

        public static void ClearAll()
        {
            foreach (var inst in _instances.Values)
            {
                inst.Clean();
            }
        }

        public event InitCallbackDelegate OnObjectInit;

        private GameObject _gameObject;
        
        private Dictionary<string, List<string>> packagePools = new Dictionary<string, List<string>>();

        private UIObjectPool(GameObject gameObject)
            :base(gameObject.transform)
        {
            _gameObject = gameObject;
            initCallback = OnObjectInit;
            FairyGUIUtils.OnPackageUnloadHandle += OnPackageUnloaded;
        }

        public GObject Get(string url)
        {
            return GetObject(url);
        }

        public void Put(GObject obj)
        {
            if (obj.displayObject.isDisposed)
                return;

            var item = obj.packageItem;
            if (item != null)
            {
                var packageName = item.owner.name;
                var url = obj.resourceURL;
                if (packagePools.TryGetValue(packageName, out var list))
                {
                    if (!list.Contains(url))
                        list.Add(url);
                }
                else
                {
                    list = new List<string>();
                    list.Add(url);
                    packagePools.Add(packageName, list);
                }
            }
            
            ReturnObject(obj);
        }

        public void Clean()
        {
            packagePools.Clear();
            Clear();
        }

        public void ClearByUrl(string url)
        {
            if (_pool.TryGetValue(url, out var queue))
            {
                foreach (GObject obj in queue)
                    obj.Dispose();

                _pool.Remove(url);
            }
        }

        public void Destroy()
        {
            Clean();
            FairyGUIUtils.OnPackageUnloadHandle -= OnPackageUnloaded;
            Object.Destroy(_gameObject);
            _gameObject = null;
        }

        private void OnPackageUnloaded(string packageName)
        {
            if (packagePools.TryGetValue(packageName, out var list))
            {
                foreach (var url in list)
                {
                    ClearByUrl(url);
                }
                
                list.Clear();
            }
        }
    }
}