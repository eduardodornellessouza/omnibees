//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OB.Domain.Reservations
{
    using System;
    using System.Collections.Generic;
    
    using OB.Domain;
    using System.CodeDom.Compiler;
    using OB.Api.Core;
        
    [GeneratedCode("OB TM T4 Domain Object template","1.0")]
    public partial class VisualState : DomainObject
    {
    	public static readonly DomainScope DomainScope = DomainScopes.Reservations;
    
    	public static readonly string PROP_NAME_UID = "UID";
    	public static readonly string PROP_NAME_STATETYPE = "StateType";
    	public static readonly string PROP_NAME_JSONDATA = "JSONData";
    	public static readonly string PROP_NAME_LOOKUPKEY_1 = "LookupKey_1";
    	public static readonly string PROP_NAME_LOOKUPKEY_2 = "LookupKey_2";
    	public static readonly string PROP_NAME_LOOKUPKEY_3 = "LookupKey_3";
    
        public long UID { get; set; }
        public string StateType { get; set; }
        public string JSONData { get; set; }
        public string LookupKey_1 { get; set; }
        public string LookupKey_2 { get; set; }
        public string LookupKey_3 { get; set; }
    }
}
