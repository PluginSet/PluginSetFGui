using System.Collections.Generic;
using FairyGUI;
using FairyGUI.Utils;

namespace PluginSet.FGUI
{
    public class UIToast: ToastBase
    {
        private static readonly ByteBuffer defautTransBuff;

        static UIToast()
        {
            byte[] bytes = new byte[]
            {
                255, 253,
                0, 0, 0, 0,
                0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0
            };
            defautTransBuff = new ByteBuffer(bytes);
        }

        private static Transition DefaultTransition(GComponent target)
        {
            var tran = new Transition(target);
            defautTransBuff.position = 0;
            tran.Setup(defautTransBuff);
            return tran;
        }
        
        // Toast 在进入/退出动画间隙停留时长
        public static float ToastStaySeconds = 3f;

        // 有新Toast当前Toast立即退出(播放完整动画，不停留)
        public static bool OutImmediatelyWhenNew = true;
        
        // 当上个Toast在播放退出动画时，就可以播放下个Toast
        public static bool EnableShowWhenLastOut = true;
            
        private static UIToast _currentToast;
        
        private static List<UIToast> _queue = new List<UIToast>();

        private static void ShowNextToast()
        {
            Timers.inst.CallLater(ShowNext);
        }

        private static void ShowNext(object _)
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
                if (toast.isDisposed) continue;
                
                ShowToast(toast);
                return;
            }
        }

        private static void ShowToast(UIToast toast)
        {
            _currentToast = toast;
            toast.visible = true;
            toast.Show(ToastStaySeconds);
        }

        private static void CacheToast(object toast)
        {
            UIObjectPool.Instance.Put((UIToast) toast);
        }

        private Transition _tempTransition;
        
        public virtual Transition InTransition { get; set; }
        
        public virtual Transition OutTransition { get; set; }

        private float _outDelay;

        protected override void ConstructExtension(ByteBuffer buffer)
        {
            base.ConstructExtension(buffer);
            
            onAddedToStage.Add(OnAddToStage);
            onRemovedFromStage.Add(OnRemoveFromStage);
            
            InitToast();
            _tempTransition = DefaultTransition(this);
            
            if (InTransition == null)
            {
                InTransition = DefaultTransition(this);
            }

            if (OutTransition == null)
            {
                OutTransition = DefaultTransition(this);
            }
        }

        protected virtual void InitToast()
        {
            InTransition = GetTransition("in");
            OutTransition = GetTransition("out");
        }

        private void OnAddToStage()
        {
            visible = false;
            _queue.Add(this);
            ShowNextToast();
        }

        private void OnRemoveFromStage()
        {
            Timers.inst.CallLater(CacheToast, this);
            if (_currentToast == this)
                _currentToast = null;

            if (!_queue.Remove(this))
                ShowNextToast();
        }

        public override void Show(float duration)
        {
            _outDelay = duration;
            InTransition.Play(PlayDelayAnimation);
        }

        private void PlayDelayAnimation()
        {
            _tempTransition.Play(1, _outDelay, PlayOutAnimation);
        }

        private void PlayOutAnimation()
        {
            if (EnableShowWhenLastOut)
            {
                _currentToast = null;
                ShowNextToast();
            }
            
            OutTransition.Play(RemoveFromParent);
        }
        
        private void OutImmediately()
        {
            _outDelay = 0;
            if (InTransition.playing)
                return;
            
            if (_tempTransition.playing)
                _tempTransition.Stop();
                
            PlayOutAnimation();
        }

        public override void Hide()
        {
            OutImmediately();
        }

        public override void Dispose()
        {
            GTween.Kill(this);
            base.Dispose();
        }
    }
}