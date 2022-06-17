using FairyGUI;
using UnityEngine;

namespace PluginSet.FGUI
{
    public class FanListDelegate: CustomListDelegate
    {
        private GCustomList _list;
        private float _radius;
        private bool _updateWhenScroll;

        private Vector2 _itemForward;
        private Vector2 _sectorCenter;
        private bool _sectorCenterDirty;

        private ListLayoutType _latestLayoutType;
        private AlignType _latestAlign;
        private VertAlignType _latestVertAlign;

        public float Radius
        {
            get => _radius;

            set
            {
                _radius = value;
                _sectorCenterDirty = true;
                _list?.SetBoundsChangedFlag();
            }
        }

        private void UpdateRadiusInTweener(GTweener tweener)
        {
            Radius = tweener.value.x;
        }

        private void SetRadiusOnTweenerCompleted(GTweener tweener)
        {
            Radius = tweener.endValue.x;
        }

        public GTweener RadiusTo(float from, float to, float duration)
        {
            GTween.Kill(this);

            return GTween.To(from, to, duration)
                .SetTarget(this)
                .OnUpdate(UpdateRadiusInTweener)
                .OnComplete(SetRadiusOnTweenerCompleted);
        }

        public GTweener RadiusTo(float to, float duration)
        {
            return RadiusTo(_radius, to, duration);
        }

        public FanListDelegate(float radius, bool updateWhenScroll)
        {
            _radius = radius;
            _updateWhenScroll = updateWhenScroll;
            _sectorCenterDirty = true;
        }

        public FanListDelegate(float radius)
            :this(radius, false)
        {
            
        }

        internal override void OnDelegateRemove(GCustomList list)
        {
            _list = null;
            GTween.Kill(this);
            
            if (_updateWhenScroll)
                list.scrollPane?.onScroll.RemoveCapture(UpdateBoundsAfterScroll);
        }

        internal override void OnListSetupAfterAdd(GCustomList list)
        {
            _list = list;
            
            if (list.layout == ListLayoutType.FlowHorizontal || list.layout == ListLayoutType.SingleRow)
            {
                list.align = AlignType.Center;
            }
            else
            {
                list.verticalAlign = VertAlignType.Middle;
            }

            if (_updateWhenScroll)
                list.scrollPane?.onScroll.AddCapture(UpdateBoundsAfterScroll);

            _latestLayoutType = list.layout;
            _latestAlign = list.align;
            _latestVertAlign = list.verticalAlign;
            _sectorCenterDirty = true;
        }

        internal override void OnListUpdateBounds(GCustomList list)
        {
            var count = list.numItems;
            if (count < 0)
                return;

            Origin360 origin;
            var layout = list.layout;
            if (layout != _latestLayoutType)
            {
                _sectorCenterDirty = true;
                _latestLayoutType = layout;
            }
            
            if (layout == ListLayoutType.FlowHorizontal || layout == ListLayoutType.SingleRow)
            {
                if (list.verticalAlign == VertAlignType.Top)
                    origin = Origin360.Top;
                else
                    origin = Origin360.Bottom;

                if (list.verticalAlign != _latestVertAlign)
                {
                    _latestVertAlign = list.verticalAlign;
                    _sectorCenterDirty = true;
                }
            }
            else
            {
                if (list.align == AlignType.Right)
                    origin = Origin360.Right;
                else
                    origin = Origin360.Left;

                if (list.align != _latestAlign)
                {
                    _latestAlign = list.align;
                    _sectorCenterDirty = true;
                }
            }

            var size = list.size;
            
            if (_sectorCenterDirty)
            {
                CalculateSectorCenter(in size, in origin);
                _sectorCenterDirty = false;
            }

            var foldInvisibleItems = list.foldInvisibleItems;
            var offset = list.container.xy;
            
            foreach (var child in list._children)
            {
                if (foldInvisibleItems && !child.visible)
                    continue;

                SetItemPositionAndRotation(child, in origin, in offset, in size);
            }
        }

        private void CalculateSectorCenter(in Vector2 size, in Origin360 origin)
        {
            float width = size.x;
            float height = size.y;
            
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            
            float wSqr = halfWidth * halfWidth;
            float hSqr = halfHeight * halfHeight;
            float rSqr = _radius * _radius;
            
            switch (origin)
            {
                case Origin360.Left:
                    _itemForward = Vector2.left;
                    _sectorCenter = new Vector2(- Mathf.Sqrt(rSqr - hSqr), halfHeight);
                    break;
                case Origin360.Right:
                    _itemForward = Vector2.right;
                    _sectorCenter = new Vector2(width + Mathf.Sqrt(rSqr - hSqr), halfHeight);
                    break;
                case Origin360.Top:
                    _itemForward = Vector2.up;
                    _sectorCenter = new Vector2(halfWidth, - Mathf.Sqrt(rSqr - wSqr));
                    break;
                case Origin360.Bottom:
                default:
                    _itemForward = Vector2.down;
                    _sectorCenter = new Vector2(halfWidth, height + Mathf.Sqrt(rSqr - wSqr));
                    break;
            }
        }

        private void SetItemPositionAndRotation(GObject item, in Origin360 origin, in Vector2 offset, in Vector2 viewSize)
        {
            float width = item.width;
            float height = item.height;
            
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            
            Vector2 pos = item.xy;
            Vector2 size = item.size;
            Vector2 pivot = item.pivot;
            Vector2 anchor = Vector2.Scale(size, pivot);
            Vector2 leftTop = pos;
            if (item.pivotAsAnchor)
                leftTop -= anchor;
            
            Vector2 referPoint;
            float radius = _radius;
            
            switch (origin)
            {
                case Origin360.Left:
                    radius += leftTop.x + offset.x;
                    referPoint = new Vector2(0f, halfHeight);
                    break;
                case Origin360.Right:
                    radius += viewSize.x - leftTop.x - width - offset.x;
                    referPoint = new Vector2(width, halfHeight);
                    break;
                case Origin360.Top:
                    radius += leftTop.y + offset.y;
                    referPoint = new Vector2(halfWidth, 0f);
                    break;
                case Origin360.Bottom:
                default:
                    radius += viewSize.y - leftTop.y - height - offset.y;
                    referPoint = new Vector2(halfWidth, height);
                    break;
            }

            Vector2 referGlobal = offset + pos + referPoint;
            if (item.pivotAsAnchor)
                referGlobal -= anchor;
            
            Vector2 transform = referGlobal - _sectorCenter;
            
            var rotation = Vector2.SignedAngle(_itemForward, transform);
            item.rotation = rotation;

            transform = transform.normalized * radius - transform;
            var referTrans = Rotate(referPoint, anchor, rotation * Mathf.Deg2Rad);
            item.xy = pos + transform + referPoint - referTrans;
        }

        private static void UpdateBoundsAfterScroll(EventContext context)
        {
            var scrollPanel = context.sender as ScrollPane;
            scrollPanel?.owner.SetBoundsChangedFlag();
        }

        private static Vector2 Rotate(in Vector2 vector, in Vector2 center, float radian)
        {
            float x = vector.x - center.x;
            float y = vector.y - center.y;
            
            float cos = Mathf.Cos(radian);
            float sin = Mathf.Sin(radian);
            
            return new Vector2(x * cos - y * sin, x * sin + y * cos) + center;
        }
    }
}