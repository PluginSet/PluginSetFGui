using FairyGUI;

namespace PluginSet.FGUI
{
    public class ToastBase: GLabel
    {
        public virtual int Tag { get; set; }

        public virtual void Show()
        {
            
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