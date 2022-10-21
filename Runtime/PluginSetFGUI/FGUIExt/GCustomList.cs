using System;
using FairyGUI;
using FairyGUI.Utils;
using UnityEngine;

namespace PluginSet.FGUI
{
    public abstract class CustomListDelegate
    {
        public CustomListDelegate()
        {
        }
        
        internal abstract void OnDelegateRemove(GCustomList list);
        
        internal abstract void OnListSetupAfterAdd(GCustomList list);
        
        internal abstract void OnListUpdateBounds(GCustomList list);
    }
    
    public class GCustomList: GList
    {
        protected CustomListDelegate _listDelegate;
        
        protected bool _matchAlign = false;

        protected bool _sizeLimit = false;
        protected float _maxGap;
        protected float _maxSize;

        public bool listSizeFollowContainer = false;

        private float _limitedLineGap;
        private float _limitedColumnGap;

        protected float usingLineGap => _sizeLimit ? _limitedLineGap : _lineGap;
        protected float usingColummGap => _sizeLimit ? _limitedColumnGap : _columnGap;

        public void SetMatchAlign(bool match)
        {
            if (_matchAlign == match)
                return;
            
            _matchAlign = match;
            UpdateBounds();
        }

        public void SetCustomListDelegate(CustomListDelegate @delegate)
        {
            _listDelegate?.OnDelegateRemove(this);
            _listDelegate = @delegate;
            if (this.parent == null)
                return;
            
            _listDelegate?.OnListSetupAfterAdd(this);
            _listDelegate?.OnListUpdateBounds(this);
        }

        public void SetSizeLimit(float maxSize, float maxGap)
        {
            _sizeLimit = true;
            _maxSize = maxSize;
            _maxGap = maxGap;
            _limitedLineGap = _lineGap;
            _limitedColumnGap = _columnGap;
        }

        public void ClearSizeLimit()
        {
            _sizeLimit = false;
        }

        public override void Setup_AfterAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_AfterAdd(buffer, beginPos);
            _listDelegate?.OnListSetupAfterAdd(this);
        }
        
        protected override void UpdateBounds()
        {
            if (_virtual)
                return;

            UpdateBoundsInternal();
            _listDelegate?.OnListUpdateBounds(this);
        }

        protected void UpdateBoundsInternal()
        {
            if (!_matchAlign || _layout != ListLayoutType.FlowHorizontal && _layout != ListLayoutType.FlowVertical)
            {
                if (!_autoResizeItem && _sizeLimit && (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.SingleRow))
                    CheckSizeLimit();
                
                BaseUpdateBounds();
                return;
            }
            
            UpdateBoundsMatchAlign();
        }
        
        protected void BaseUpdateBounds()
        {
            if (_virtual)
                return;

            int cnt = _children.Count;
            int i;
            int j = 0;
            GObject child;
            float curX = 0;
            float curY = 0;
            float cw, ch;
            float maxWidth = 0;
            float maxHeight = 0;
            float viewWidth = this.viewWidth;
            float viewHeight = this.viewHeight;
            
            var _usingLineGap = usingLineGap;
            var _usingColumnGap = usingColummGap;

            if (_layout == ListLayoutType.SingleColumn)
            {
                for (i = 0; i < cnt; i++)
                {
                    child = GetChildAt(i);
                    if (foldInvisibleItems && !child.visible)
                        continue;

                    if (curY != 0)
                        curY += _usingLineGap;
//                    child.y = curY;
                    child.SetXY(0f, curY);
                    if (_autoResizeItem)
                        child.SetSize(viewWidth, child.height, true);
                    curY += Mathf.CeilToInt(child.height);
                    if (child.width > maxWidth)
                        maxWidth = child.width;
                }

                ch = curY;
                if (ch <= viewHeight && _autoResizeItem && scrollPane != null && scrollPane._displayInDemand && scrollPane.vtScrollBar != null)
                {
                    viewWidth += scrollPane.vtScrollBar.width;
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        child.SetSize(viewWidth, child.height, true);
                        if (child.width > maxWidth)
                            maxWidth = child.width;
                    }
                }
                cw = Mathf.CeilToInt(maxWidth);
            }
            else if (_layout == ListLayoutType.SingleRow)
            {
                for (i = 0; i < cnt; i++)
                {
                    child = GetChildAt(i);
                    if (foldInvisibleItems && !child.visible)
                        continue;

                    if (curX != 0)
                        curX += _usingColumnGap;
//                    child.x = curX;
                    child.SetXY(curX, 0f);
                    if (_autoResizeItem)
                        child.SetSize(child.width, viewHeight, true);
                    curX += Mathf.CeilToInt(child.width);
                    if (child.height > maxHeight)
                        maxHeight = child.height;
                }

                cw = curX;
                if (cw <= viewWidth && _autoResizeItem && scrollPane != null && scrollPane._displayInDemand && scrollPane.hzScrollBar != null)
                {
                    viewHeight += scrollPane.hzScrollBar.height;
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        child.SetSize(child.width, viewHeight, true);
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                    }
                }
                ch = Mathf.CeilToInt(maxHeight);
            }
            else if (_layout == ListLayoutType.FlowHorizontal)
            {
                if (_autoResizeItem && _columnCount > 0)
                {
                    float lineSize = 0;
                    int lineStart = 0;
                    float remainSize;
                    float remainPercent;

                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        lineSize += child.sourceWidth;
                        j++;
                        if (j == _columnCount || i == cnt - 1)
                        {
                            remainSize = viewWidth - (j - 1) * _usingColumnGap;
                            remainPercent = 1;
                            curX = 0;
                            for (j = lineStart; j <= i; j++)
                            {
                                child = GetChildAt(j);
                                if (foldInvisibleItems && !child.visible)
                                    continue;

                                child.SetXY(curX, curY);
                                float perc = child.sourceWidth / lineSize;
                                child.SetSize(Mathf.Round(perc / remainPercent * remainSize), child.height, true);
                                remainSize -= child.width;
                                remainPercent -= perc;
                                curX += child.width + _usingColumnGap;

                                if (child.height > maxHeight)
                                    maxHeight = child.height;
                            }
                            //new line
                            curY += Mathf.CeilToInt(maxHeight) + _usingLineGap;
                            maxHeight = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;
                        }
                    }
                    ch = curY + Mathf.CeilToInt(maxHeight);
                    cw = viewWidth;
                }
                else
                {
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (curX != 0)
                            curX += _usingColumnGap;

                        if (_columnCount != 0 && j >= _columnCount
                            || _columnCount == 0 && curX + child.width > viewWidth && maxHeight != 0)
                        {
                            //new line
                            curX = 0;
                            curY += Mathf.CeilToInt(maxHeight) + _usingLineGap;
                            maxHeight = 0;
                            j = 0;
                        }
                        child.SetXY(curX, curY);
                        curX += Mathf.CeilToInt(child.width);
                        if (curX > maxWidth)
                            maxWidth = curX;
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                        j++;
                    }
                    ch = curY + Mathf.CeilToInt(maxHeight);
                    cw = Mathf.CeilToInt(maxWidth);
                }
            }
            else if (_layout == ListLayoutType.FlowVertical)
            {
                if (_autoResizeItem && _lineCount > 0)
                {
                    float lineSize = 0;
                    int lineStart = 0;
                    float remainSize;
                    float remainPercent;

                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        lineSize += child.sourceHeight;
                        j++;
                        if (j == _lineCount || i == cnt - 1)
                        {
                            remainSize = viewHeight - (j - 1) * _usingLineGap;
                            remainPercent = 1;
                            curY = 0;
                            for (j = lineStart; j <= i; j++)
                            {
                                child = GetChildAt(j);
                                if (foldInvisibleItems && !child.visible)
                                    continue;

                                child.SetXY(curX, curY);
                                float perc = child.sourceHeight / lineSize;
                                child.SetSize(child.width, Mathf.Round(perc / remainPercent * remainSize), true);
                                remainSize -= child.height;
                                remainPercent -= perc;
                                curY += child.height + _usingLineGap;

                                if (child.width > maxWidth)
                                    maxWidth = child.width;
                            }
                            //new line
                            curX += Mathf.CeilToInt(maxWidth) + _usingColumnGap;
                            maxWidth = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;
                        }
                    }
                    cw = curX + Mathf.CeilToInt(maxWidth);
                    ch = viewHeight;
                }
                else
                {
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (curY != 0)
                            curY += _usingLineGap;

                        if (_lineCount != 0 && j >= _lineCount
                            || _lineCount == 0 && curY + child.height > viewHeight && maxWidth != 0)
                        {
                            curY = 0;
                            curX += Mathf.CeilToInt(maxWidth) + _usingColumnGap;
                            maxWidth = 0;
                            j = 0;
                        }
                        child.SetXY(curX, curY);
                        curY += child.height;
                        if (curY > maxHeight)
                            maxHeight = curY;
                        if (child.width > maxWidth)
                            maxWidth = child.width;
                        j++;
                    }
                    cw = curX + Mathf.CeilToInt(maxWidth);
                    ch = Mathf.CeilToInt(maxHeight);
                }
            }
            else //pagination
            {
                int page = 0;
                int k = 0;
                float eachHeight = 0;
                if (_autoResizeItem && _lineCount > 0)
                    eachHeight = Mathf.Floor((viewHeight - (_lineCount - 1) * _usingLineGap) / _lineCount);

                if (_autoResizeItem && _columnCount > 0)
                {
                    float lineSize = 0;
                    int lineStart = 0;
                    float remainSize;
                    float remainPercent;

                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (j == 0 && (_lineCount != 0 && k >= _lineCount
                            || _lineCount == 0 && curY + (_lineCount > 0 ? eachHeight : child.height) > viewHeight))
                        {
                            //new page
                            page++;
                            curY = 0;
                            k = 0;
                        }

                        lineSize += child.sourceWidth;
                        j++;
                        if (j == _columnCount || i == cnt - 1)
                        {
                            remainSize = viewWidth - (j - 1) * _usingColumnGap;
                            remainPercent = 1;
                            curX = 0;
                            for (j = lineStart; j <= i; j++)
                            {
                                child = GetChildAt(j);
                                if (foldInvisibleItems && !child.visible)
                                    continue;

                                child.SetXY(page * viewWidth + curX, curY);
                                float perc = child.sourceWidth / lineSize;
                                child.SetSize(Mathf.Round(perc / remainPercent * remainSize), _lineCount > 0 ? eachHeight : child.height, true);
                                remainSize -= child.width;
                                remainPercent -= perc;
                                curX += child.width + _usingColumnGap;

                                if (child.height > maxHeight)
                                    maxHeight = child.height;
                            }
                            //new line
                            curY += Mathf.CeilToInt(maxHeight) + _usingLineGap;
                            maxHeight = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;

                            k++;
                        }
                    }
                }
                else
                {
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (curX != 0)
                            curX += _usingColumnGap;

                        if (_autoResizeItem && _lineCount > 0)
                            child.SetSize(child.width, eachHeight, true);

                        if (_columnCount != 0 && j >= _columnCount
                            || _columnCount == 0 && curX + child.width > viewWidth && maxHeight != 0)
                        {
                            curX = 0;
                            curY += maxHeight + _usingLineGap;
                            maxHeight = 0;
                            j = 0;
                            k++;

                            if (_lineCount != 0 && k >= _lineCount
                                || _lineCount == 0 && curY + child.height > viewHeight && maxWidth != 0)//new page
                            {
                                page++;
                                curY = 0;
                                k = 0;
                            }
                        }
                        child.SetXY(page * viewWidth + curX, curY);
                        curX += Mathf.CeilToInt(child.width);
                        if (curX > maxWidth)
                            maxWidth = curX;
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                        j++;
                    }
                }
                ch = page > 0 ? viewHeight : (curY + Mathf.CeilToInt(maxHeight));
                cw = (page + 1) * viewWidth;
            }

            HandleAlign(cw, ch);
            SetBounds(0, 0, cw, ch);

            InvalidateBatchingState(true);
        }

        protected void CheckSizeLimit()
        {
            float itemTotalSize = 0;
            int itemCount = 0;
            foreach (var child in _children)
            {
                if (foldInvisibleItems && !child.visible)
                    continue;

                if (_layout == ListLayoutType.SingleColumn)
                    itemTotalSize += child.height;
                else if (_layout == ListLayoutType.SingleRow)
                    itemTotalSize += child.width;

                itemCount++;
            }

            if (itemCount < 1)
                return;

            var targetGap = _maxGap;
            if ((itemTotalSize + (itemCount - 1) * _maxGap) > _maxSize)
                targetGap = (_maxSize - itemTotalSize) / (itemCount - 1);

            if (_layout == ListLayoutType.SingleColumn && Math.Abs(_limitedLineGap - targetGap) > 0.1f)
            {
                _limitedLineGap = targetGap;
                lineGap = Mathf.FloorToInt(targetGap);
            }
            else if (_layout == ListLayoutType.SingleRow && Math.Abs(_limitedColumnGap - targetGap) > 0.1f)
            {
                _limitedColumnGap = targetGap;
                columnGap = Mathf.FloorToInt(targetGap);
            }
        }

        protected void UpdateBoundsMatchAlign()
        {
            int cnt = _children.Count;
            int i;
            int j = 0;
            GObject child;
            float curX = 0;
            float curY = 0;
            float cw, ch;
            float maxWidth = 0;
            float maxHeight = 0;
            float viewWidth = this.viewWidth;
            float viewHeight = this.viewHeight;

            var _usingLineGap = usingLineGap;
            var _usingColumnGap = usingColummGap;

            if (_layout == ListLayoutType.FlowVertical)
            {
                if (_autoResizeItem && _lineCount > 0)
                {
                    float lineSize = 0;
                    int lineStart = 0;
                    float remainSize;
                    float remainPercent;

                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        lineSize += child.sourceHeight;
                        j++;
                        if (j == _lineCount || i == cnt - 1)
                        {
                            remainSize = viewHeight - (j - 1) * _usingLineGap;
                            remainPercent = 1;
                            if (_verticalAlign == VertAlignType.Bottom)
                                curY = viewHeight;
                            else
                                curY = 0;
                            for (j = lineStart; j <= i; j++)
                            {
                                child = GetChildAt(j);
                                if (foldInvisibleItems && !child.visible)
                                    continue;

                                float perc = child.sourceHeight / lineSize;
                                child.SetSize(child.width, Mathf.Round(perc / remainPercent * remainSize), true);
                                if (_verticalAlign == VertAlignType.Bottom)
                                    curY -= Mathf.CeilToInt(child.height);
                                child.SetXY(curX, curY);
                                remainSize -= child.height;
                                remainPercent -= perc;
                                if (_verticalAlign == VertAlignType.Bottom)
                                    curY -= _usingLineGap;
                                else
                                    curY += child.height + _usingLineGap;

                                if (child.width > maxWidth)
                                    maxWidth = child.width;
                            }
                            //new line
                            curX += Mathf.CeilToInt(maxWidth) + _usingColumnGap;
                            maxWidth = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;
                        }
                    }
                    cw = curX + Mathf.CeilToInt(maxWidth);
                    ch = viewHeight;

                    if (_align == AlignType.Right)
                    {
                        for (i = 0; i < cnt; i++)
                        {
                            child = GetChildAt(i);
                            if (foldInvisibleItems && !child.visible)
                                continue;

                            child.x = cw - child.x - child.width;
                        }
                    }
                }
                else
                {
                    int lineStart = -1;
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (curY != 0)
                            curY += _usingLineGap;

                        if (_lineCount != 0 && j >= _lineCount
                            || _lineCount == 0 && curY + child.height > viewHeight && maxWidth != 0)
                        {
                            curY = 0;
                            curX += Mathf.CeilToInt(maxWidth) + _usingColumnGap;
                            maxWidth = 0;
                            j = 0;
                            lineStart = i;
                        }
                        child.SetXY(curX, curY);
                        curY += child.height;
                        if (curY > maxHeight)
                            maxHeight = curY;
                        if (child.width > maxWidth)
                            maxWidth = child.width;
                        j++;
                    }
                    cw = curX + Mathf.CeilToInt(maxWidth);
                    ch = Mathf.CeilToInt(maxHeight);

                    for (int ii = 0; ii < cnt; ii++)
                    {
                        child = GetChildAt(ii);
                        
                        if (_align == AlignType.Right)
                        {
                            child.x = cw - child.x - child.width;
                        }

                        if (_verticalAlign == VertAlignType.Bottom)
                        {
                            child.y = ch - child.y - child.height;
                        }
                        else if (_verticalAlign == VertAlignType.Middle)
                        {
                            if (ii >= lineStart)
                            {
                                child.y += (ch - curY) * 0.5f;
                            }
                        }
                    }
                }
                
            }
            else /*if (_layout == ListLayoutType.FlowHorizontal)*/
            {
                if (_autoResizeItem && _columnCount > 0)
                {
                    float lineSize = 0;
                    int lineStart = 0;
                    float remainSize;
                    float remainPercent;

                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        lineSize += child.sourceWidth;
                        j++;
                        if (j == _columnCount || i == cnt - 1)
                        {
                            remainSize = viewWidth - (j - 1) * _usingColumnGap;
                            remainPercent = 1;
                            if (_align == AlignType.Right)
                                curX = viewWidth;
                            else
                                curX = 0;
                            for (j = lineStart; j <= i; j++)
                            {
                                child = GetChildAt(j);
                                if (foldInvisibleItems && !child.visible)
                                    continue;

                                float perc = child.sourceWidth / lineSize;
                                child.SetSize(Mathf.Round(perc / remainPercent * remainSize), child.height, true);
                                if (_align == AlignType.Right)
                                    curX -= child.width;
                                child.SetXY(curX, curY);
                                remainSize -= child.width;
                                remainPercent -= perc;
                                if (_align == AlignType.Right)
                                    curX -= _usingColumnGap;
                                else
                                    curX += child.width + _usingColumnGap;

                                if (child.height > maxHeight)
                                    maxHeight = child.height;
                            }
                            //new line
                            curY += Mathf.CeilToInt(maxHeight) + _usingLineGap;
                            maxHeight = 0;
                            j = 0;
                            lineStart = i + 1;
                            lineSize = 0;
                        }
                    }
                    ch = curY + Mathf.CeilToInt(maxHeight);
                    cw = viewWidth;

                    if (_verticalAlign == VertAlignType.Bottom)
                    {
                        if (_align == AlignType.Right)
                        {
                            for (i = 0; i < cnt; i++)
                            {
                                child = GetChildAt(i);
                                if (foldInvisibleItems && !child.visible)
                                    continue;

                                child.y = ch - child.y - child.height;
                            }
                        }
                    }
                }
                else
                {
                    int columnStart = -1;
                    for (i = 0; i < cnt; i++)
                    {
                        child = GetChildAt(i);
                        if (foldInvisibleItems && !child.visible)
                            continue;

                        if (curX != 0)
                            curX += _usingColumnGap;

                        if (_columnCount != 0 && j >= _columnCount
                            || _columnCount == 0 && curX + child.width > viewWidth && maxHeight != 0)
                        {
                            //new line
                            curX = 0;
                            curY += Mathf.CeilToInt(maxHeight) + _usingLineGap;
                            maxHeight = 0;
                            j = 0;
                            columnStart = i;
                        }
                        child.SetXY(curX, curY);
                        curX += Mathf.CeilToInt(child.width);
                        if (curX > maxWidth)
                            maxWidth = curX;
                        if (child.height > maxHeight)
                            maxHeight = child.height;
                        j++;
                    }
                    ch = curY + Mathf.CeilToInt(maxHeight);
                    cw = Mathf.CeilToInt(maxWidth);
                    
                    for (int ii = 0; ii < cnt; ii++)
                    {
                        child = GetChildAt(ii);

                        if (_verticalAlign == VertAlignType.Bottom)
                        {
                            child.y = ch - child.y - child.height;
                        }
                        
                        if (_align == AlignType.Right)
                        {
                            child.x = cw - child.x - child.width;
                        }
                        else if (_align == AlignType.Center)
                        {
                            if (ii >= columnStart)
                            {
                                child.x += (cw - curX) * 0.5f;
                            }
                        }
                    }
                }
            }
            
            HandleAlign(cw, ch);
            SetBounds(0, 0, cw, ch);

            InvalidateBatchingState(true);
        }

        protected override void SetBounds(float ax, float ay, float aw, float ah)
        {
            base.SetBounds(ax, ay, aw, ah);
            if (listSizeFollowContainer)
            {
                this.SetSize(aw, ah);
            }
        }
    }
}