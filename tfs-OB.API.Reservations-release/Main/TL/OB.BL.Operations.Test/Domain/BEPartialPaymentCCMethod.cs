//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OB.BL.Operations.Test.Domain.CRM
{
    using System;
    using System.Collections.Generic;
    
    using OB.Domain;
    using System.CodeDom.Compiler;
        
    [GeneratedCode("OB TM T4 Domain Object template","1.0")]
    public partial class BEPartialPaymentCCMethod 
    {
        public long UID { get; set; }
        public int Parcel { get; set; }
        public Nullable<decimal> InterestRate { get; set; }
        public long Property_UID { get; set; }
        public Nullable<long> PaymentMethods_UID { get; set; }
        public Nullable<long> PaymentMethodTypeId { get; set; }
        public Nullable<decimal> PartialPaymentMinimumValue { get; set; }
    
        //public virtual PaymentMethod PaymentMethod { get; set; }
    }
}
