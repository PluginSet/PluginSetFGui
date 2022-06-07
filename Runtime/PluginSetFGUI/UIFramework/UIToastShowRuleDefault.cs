using System.Collections.Generic;
using FairyGUI;

namespace PluginSet.FGUI
{
    public class UIToastShowRuleDefault : UIToastShowRule
    {
        // Toast 在进入/退出动画间隙停留时长
        public float DefaultStaySeconds;

        // 有新Toast当前Toast立即退出(播放完整动画，不停留)
        public bool OutImmediatelyWhenNew;
        
        // 当上个Toast在播放退出动画时，就可以播放下个Toast
        public bool EnableShowWhenLastOut;
            
        private UIToast _currentToast;
        private List<UIToast> _queue;

        public UIToastShowRuleDefault(float defaultStaySeconds, bool outImmediatelyWhenNew, bool enableShowWhenLastOut)
        {
            DefaultStaySeconds = defaultStaySeconds;
            OutImmediatelyWhenNew = outImmediatelyWhenNew;
            EnableShowWhenLastOut = enableShowWhenLastOut;
            
            _currentToast = null;
            _queue = new List<UIToast>();
        }
        
        
        private void ShowNext(object _)
        {
            if (_currentToast != null && !_currentToast.isDisposed)
            {
                if (OutImmediatelyWhenNew && _queue.Count > 0)
                    _currentToast.OutImmediately();
                return;
            }

            _currentToast = null;
            while (_queue.Count > 0)
            {
                var toast = _queue[0];
                _queue.RemoveAt(0);
                if (toast == null || toast.isDisposed) continue;

                _currentToast = toast;
                toast.Show(DefaultStaySeconds);
                if (OutImmediatelyWhenNew && _queue.Count > 0)
                    toast.OutImmediately();
                return;
            }
        }

        internal override void OnToastAdded(UIToast toast)
        {
            _queue.Add(toast);
            Timers.inst.CallLater(ShowNext);
        }

        internal override void OnToastOut(UIToast toast)
        {
            if (EnableShowWhenLastOut)
            {
                _currentToast = null;
                Timers.inst.CallLater(ShowNext);
            }
        }

        internal override void OnToastRemove(UIToast toast)
        {
            if (_currentToast == toast)
                _currentToast = null;

            if (!_queue.Remove(toast))
                Timers.inst.CallLater(ShowNext);
        }
    }
}