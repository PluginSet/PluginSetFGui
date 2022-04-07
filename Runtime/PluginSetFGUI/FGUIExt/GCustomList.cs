using FairyGUI;
using UnityEngine;

namespace PluginSet.FGUI
{
    public class GCustomList: GList
    {
        protected bool _matchAlign = false;
        
        public void SetMatchAlign(bool match)
        {
            if (_matchAlign == match)
                return;
            
            _matchAlign = match;
            UpdateBounds();
        }
    
        protected override void UpdateBounds()
        {
            if (_virtual)
                return;

            if (!_matchAlign || _layout != ListLayoutType.FlowHorizontal && _layout != ListLayoutType.FlowVertical)
            {
                base.UpdateBounds();
                return;
            }
            
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
                            remainSize = viewHeight - (j - 1) * _lineGap;
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
                                    curY -= _lineGap;
                                else
                                    curY += child.height + _lineGap;

                                if (child.width > maxWidth)
                                    maxWidth = child.width;
                            }
                            //new line
                            curX += Mathf.CeilToInt(maxWidth) + _columnGap;
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
                            curY += _lineGap;

                        if (_lineCount != 0 && j >= _lineCount
                            || _lineCount == 0 && curY + child.height > viewHeight && maxWidth != 0)
                        {
                            curY = 0;
                            curX += Mathf.CeilToInt(maxWidth) + _columnGap;
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
                            remainSize = viewWidth - (j - 1) * _columnGap;
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
                                    curX -= _columnGap;
                                else
                                    curX += child.width + _columnGap;

                                if (child.height > maxHeight)
                                    maxHeight = child.height;
                            }
                            //new line
                            curY += Mathf.CeilToInt(maxHeight) + _lineGap;
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
                            curX += _columnGap;

                        if (_columnCount != 0 && j >= _columnCount
                            || _columnCount == 0 && curX + child.width > viewWidth && maxHeight != 0)
                        {
                            //new line
                            curX = 0;
                            curY += Mathf.CeilToInt(maxHeight) + _lineGap;
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
    }
}