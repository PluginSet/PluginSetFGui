using FairyGUI;

namespace PluginSet.FGUI
{
    public class ToastBase: GLabel
    {
        public virtual int Tag { get; set; }

        public virtual void Show(float duration)
        {
        }

        public void Show()
        {
            Show(float.MaxValue);
        }

        public virtual void Hide()
        {
            
        }

        public virtual void Close()
        {
            RemoveFromParent();
        }
    }
}