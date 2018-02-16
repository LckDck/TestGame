using System;
namespace GameForestMatch3.Logic
{
    public class Cell : Object
    {
        public CellType Type;
        public int Col { get; set; }
        public int Row { get; set; }

        public bool IsCenter { get; set; }
        public bool IsTouched { get; set; }

        public virtual int Points 
        { 
            get 
            {
                return 100;
            }
        }

        //public bool IsDeleted { get; set; }

        //public void Dispose()
        //{
        //    IsDeleted = true;
        //}
    }
}
