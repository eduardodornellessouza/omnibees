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
    public partial class PropertyQueue : DomainObject
    {

    
        public long UID { get; set; }
        public long Property_UID { get; set; }
        public Nullable<long> PropertyEvent_UID { get; set; }
        public System.DateTime Date { get; set; }
        public bool IsProcessed { get; set; }
        public Nullable<long> TaskType_UID { get; set; }
        public Nullable<bool> IsProcessing { get; set; }
        public Nullable<int> Retry { get; set; }
        public string ErrorList { get; set; }
        public Nullable<System.DateTime> LastProcessingDate { get; set; }
        public string MailTo { get; set; }
        public string MailFrom { get; set; }
        public string MailSubject { get; set; }
        public string MailBody { get; set; }
        public Nullable<long> SystemEvent_UID { get; set; }
        public Nullable<long> SystemTemplate_UID { get; set; }
        public Nullable<System.DateTime> ChannelActivityErrorDateFrom { get; set; }
        public Nullable<System.DateTime> ChannelActivityErrorDateTo { get; set; }
    
        public virtual PropertyEvent PropertyEvent { get; set; }
    }
}
