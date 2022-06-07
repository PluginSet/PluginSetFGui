using FairyGUI;
using FairyGUI.Utils;

namespace PluginSet.FGUI
{
    public class UIToast: GLabel
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

        private static void CacheToast(object toast)
        {
            UIObjectPool.Instance.Put((UIToast) toast);
        }
        
        private Transition _tempTransition;
        
        public virtual int Tag { get; set; }
        
        public virtual Transition InTransition { get; set; }
        
        public virtual Transition OutTransition { get; set; }
        
        private UIToastShowRule _rule;
        private float _outDelay;
        private float? _presetDelay = null;

        public void SetRule(UIToastShowRule rule)
        {
            _rule = rule;
        }

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
            _rule?.OnToastAdded(this);
        }

        private void OnRemoveFromStage()
        {
            _presetDelay = null;
            Timers.inst.CallLater(CacheToast, this);
            _rule?.OnToastRemove(this);
        }
        
        public void Show()
        {
            _presetDelay = null;
            Show(float.MaxValue);
        }

        public void PresetDelay(float? delay)
        {
            _presetDelay = delay;
        }

        public void Show(float duration)
        {
            _outDelay = duration;
            if (_presetDelay.HasValue)
                _outDelay = _presetDelay.Value;
            
            this.visible = true;
            InTransition.Play(PlayDelayAnimation);
        }

        private void PlayDelayAnimation()
        {
            _tempTransition.Play(1, _outDelay, PlayOutAnimation);
        }

        private void PlayOutAnimation()
        {
            _rule?.OnToastOut(this);
            OutTransition.Play(RemoveFromParent);
        }
        
        internal void OutImmediately()
        {
            _outDelay = 0;
            if (InTransition.playing)
                return;
            
            if (_tempTransition.playing)
                _tempTransition.Stop();
                
            PlayOutAnimation();
        }

        public void Hide()
        {
            OutImmediately();
        }
        
        public void Close()
        {
            RemoveFromParent();
        }

        public override void Dispose()
        {
            _presetDelay = null;
            GTween.Kill(this);
            base.Dispose();
        }
    }
}