using System;
namespace OB.DL.Common.QueryResultObjects
{
    public class RateBuyerGroupQR1
    {
        public long UID { get; set; }
        public long Rate_UID { get; set; }
        public Nullable<long> BuyerGroup_UID { get; set; }
        public Nullable<long> TPI_UID { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsValueDecrease { get; set; }
        public Nullable<decimal> Value { get; set; }
        public Nullable<long> TPIAgencyCompany_UID { get; set; }
        public byte[] Revision { get; set; }
        public Nullable<decimal> GDSValue { get; set; }
        public bool GDSValueIsPercentage { get; set; }
        public bool GDSValueIsDecrease { get; set; }
        public int TPIType { get; set; }
    }
}