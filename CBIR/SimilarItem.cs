using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CBIR
{
    public class SimilarItem
    {
        public string FullPath { get; set; }
        public double Score { get; set; }

        //public int CompareTo(SimilarItem other)
        //{
        //    return this.Score.CompareTo(other.Score);
        //}
    }
}
