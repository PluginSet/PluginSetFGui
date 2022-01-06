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
                inst.Clear();
            }
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
            ReturnObject(obj);
        }

        public void Destroy()
        {
            Clear();
            Object.Destroy(_gameObject);
            _gameObject = null;
        }
    }
}