using System;
namespace GameForestMatch3.Logic
{
    public class Cell : Object
    {
        public GemType Type;
        public int Col { get; set; }
        public int Row { get; set; }

        public Cell()
        {
        }
    }
}
