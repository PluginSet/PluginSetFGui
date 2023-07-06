using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace PluginSet.FGUI
{
    public class UIObjectPool : GObjectPool
    {
        private static readonly Dictionary<string, UIObjectPool> Instances = new Dictionary<string, UIObjectPool>();
        private static readonly Dictionary<string, List<string>> PackagePools = new Dictionary<string, List<string>>();
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
            if (Instances.TryGetValue(rootName, out var inst))
                return inst;
            
            var gameObject = new GameObject(rootName);
            Object.DontDestroyOnLoad(gameObject);
            var newInst = new UIObjectPool(gameObject);
            Instances.Add(rootName, newInst);
            return newInst;
        }

        public static void ClearAll()
        {
            foreach (var inst in Instances.Values)
            {
                inst.Clean();
            }
            PackagePools.Clear();
        }

        public event InitCallbackDelegate OnObjectInit;

        private GameObject _gameObject;

        private UIObjectPool(GameObject gameObject)
            :base(gameObject.transform)
        {
            _gameObject = gameObject;
            initCallback = OnObjectInit;
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
                if (PackagePools.TryGetValue(packageName, out var list))
                {
                    if (!list.Contains(url))
                        list.Add(url);
                }
                else
                {
                    // package not loaded, dispose object
                    obj.Dispose();
                    return;
                }
            }
            
            ReturnObject(obj);
        }

        public void Clean()
        {
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
            Object.Destroy(_gameObject);
            _gameObject = null;
        }

        internal static void OnPackageLoaded(string packageName)
        {
            if (PackagePools.ContainsKey(packageName))
                return;
            
            PackagePools.Add(packageName, new List<string>());
        }

        internal static void OnPackageUnload(string packageName)
        {
            if (PackagePools.TryGetValue(packageName, out var list))
            {
                foreach (var url in list)
                {
                    foreach (var pool in Instances.Values)
                    {
                        pool.ClearByUrl(url);
                    }
                }

                PackagePools.Remove(packageName);
            }
        }
    }
}