using FairyGUI;
using PluginSet.Core;
using UnityEngine;
using Logger = PluginSet.Core.Logger;

namespace PluginSet.FGUI
{
    public class GCustomLoader3D: GLoader3D
    {
        private static readonly Logger Logger = LoggerManager.GetLogger("GCustomLoader3D");
        
        private static readonly Vector3 DefaultScale = new Vector3(100, 100, 100);
        private static readonly Vector2 DefaultAnchor = new Vector2(0.5f, 0.5f);

        public Transform WrapTransform
        {
            get
            {
                var target = _content.wrapTarget;
                if (target == null)
                    return null;
                
                return target.transform;
            }
        }
        
        public GCustomLoader3D()
        {
            AddEventListener(PluginFairyGUIConst.EVENT_ON_BRANCH_CHANGE, CheckReload);
            onAddedToStage.Add(LoadContent);
            onRemovedFromStage.Add(ClearContent);
        }
        
        public override void Dispose()
        {
            onAddedToStage.Remove(LoadContent);
            onRemovedFromStage.Remove(ClearContent);
            RemoveEventListeners(PluginFairyGUIConst.EVENT_ON_BRANCH_CHANGE);
            base.Dispose();
        }
        
        private void CheckReload()
        {
            if (_contentItem == null)
                return;

            if (_contentItem.getBranch() == _contentItem)
                return;
            
            LoadContent();
        }

        public void SetWrapScale(Vector3 wScale)
        {
            var trans = WrapTransform;
            if (trans == null)
                return;

            trans.localScale = Vector3.Scale(wScale, DefaultScale);
        }

        public void SetWrapRotation(Vector3 wRotation)
        {
            var trans = WrapTransform;
            if (trans == null)
                return;

            trans.localRotation = Quaternion.Euler(wRotation);
        }

        public void SetWrapRotation(Quaternion wRotation)
        {
            var trans = WrapTransform;
            if (trans == null)
                return;

            trans.localRotation = wRotation;
        }

        public void SetWrapPosition(Vector2 pos)
        {
            var trans = WrapTransform;
            if (trans == null)
                return;

            trans.localPosition = new Vector3(pos.x , - pos.y, 0);
        }

        protected override void LoadExternal()
        {
            FreeExternal();
            
            var prefab = ResourcesManager.Instance.Load<GameObject>(url);
            if (prefab == null)
                return;

            var instance = Object.Instantiate(prefab);
            var helper = instance.GetComponent<WrapHelper>();
            var trans = instance.transform;

            var anchor = DefaultAnchor;
            var hWidth = width;
            var hHeight = height;
            
            if (helper != null)
            {
                anchor = helper.AnchorPoint;
                hWidth = helper.Width;
                hHeight = helper.Height;
            }
            
            trans.localScale = DefaultScale;
            trans.localPosition = new Vector3(anchor.x * hWidth, - anchor.y * hHeight, 0);
            
            if (helper != null)
            {
                helper.InitData(data);
                helper.Color = color;
            }

            SetWrapTarget(instance, true, (int)hWidth, (int)hHeight);
        }

        protected override void FreeExternal()
        {
            var target = _content.wrapTarget;
            if (target == null)
                return;

            _content.wrapTarget = null;
            
            if (Application.isPlaying)
                GameObject.Destroy(target);
            else
                GameObject.DestroyImmediate(target);
        }

        protected override void OnChange(string propertyName)
        {
            base.OnChange(propertyName);

            if (_contentItem != null)
                return;

            var target = _content.wrapTarget;
            if (target == null)
                return;

            var wrapHelper = target.GetComponent<WrapHelper>();
            if (wrapHelper == null)
                return;
            
            if (propertyName.Equals("color"))
            {
                wrapHelper.Color = color;
            }
        }
    }
}