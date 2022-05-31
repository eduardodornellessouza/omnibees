

namespace OB.BL.Operations.Test.Domain.CRM
{
    using System;
    using System.Collections.Generic;
    
    using OB.Domain;
    using System.CodeDom.Compiler;
        
    [GeneratedCode("OB TM T4 Domain Object template","1.0")]
    public partial class SystemEvent : DomainObject
    {
    
        public long UID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public Nullable<bool> IsProactiveActionHidden { get; set; }
        public int SEOrder { get; set; }
    
        public virtual List<PropertyEvent> PropertyEvents { get; set; }
        public virtual List<SystemDefaultEvent> SystemDefaultEvents { get; set; }
        public virtual List<SystemEventsLanguage> SystemEventsLanguages { get; set; }
    }
}
