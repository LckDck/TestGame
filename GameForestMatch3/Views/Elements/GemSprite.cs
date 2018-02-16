using System;
using CocosSharp;
using GameForestMatch3.Logic;

namespace GameForestMatch3.Views.Elements
{
    public class GemSprite : CellSprite
    {
        public Cell Cell { get; set; }

        public GemSprite(CCTexture2D texture = null, CCRect? texRectInPixels = null, bool rotated = false) : base (texture, texRectInPixels, rotated)
        {
            Opacity = 150;
        }
    }
}
