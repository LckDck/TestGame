using System;
using CocosSharp;

namespace GameForestMatch3.Views.Elements
{
    public class CellSprite : CCSprite
    {
        public int Col { get; set; }
        public int Row { get; set; }

        public CellSprite(CCTexture2D texture = null, CCRect? texRectInPixels = null, bool rotated = false) : base (texture, texRectInPixels, rotated)
        {
            
        }
    }
}
