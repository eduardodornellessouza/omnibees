using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ChildTermsOccupancy
    {
        public ChildTermsOccupancy() { PriceVariations = new List<ChildPriceVariation>(); }

        // price
        public ChildPriceType PriceType { get; set; }
        public List<ChildPriceVariation> PriceVariations { get; set; }

        // occupancy
        public bool IsPercentage { get; set; }
        public bool IsAccountableForOccupancy { get; set; }
        public int NumberOfChilds { get; set; }
    }

    public class ChildPriceVariation
    {
        public long? Currency_UID { get; set; }
        public decimal PriceVariation { get; set; }
        public bool IsPriceVariationPerc { get; set; }
        public bool IsValueDecrease { get; set; }
    }

    public enum ChildPriceType
    {
        Free = 0,
        Child = 1,
        Adult = 2
    }
}
