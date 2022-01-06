using UnityEngine;

namespace PluginSet.FGUI
{
    public abstract class WrapHelper : MonoBehaviour
    {
        [Tooltip("显示宽度（像素）")]
        public float Width;
        [Tooltip("显示高度（像素）")]
        public float Height;
        [Tooltip("显示锚点")]
        public Vector2 AnchorPoint = new Vector2(0.5f ,0.5f);

        public abstract Color Color { get; set; }
        public abstract void InitData(object data);
    }
}