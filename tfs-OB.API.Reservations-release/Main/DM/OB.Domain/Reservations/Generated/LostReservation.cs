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
    public partial class LostReservation : DomainObject
    {
    	public static readonly DomainScope DomainScope = DomainScopes.Reservations;
    
    	public static readonly string PROP_NAME_UID = "UID";
    	public static readonly string PROP_NAME_CHECKIN = "CheckIn";
    	public static readonly string PROP_NAME_CHECKOUT = "CheckOut";
    	public static readonly string PROP_NAME_GUESTNAME = "GuestName";
    	public static readonly string PROP_NAME_GUESTEMAIL = "GuestEmail";
    	public static readonly string PROP_NAME_NUMBEROFROOMS = "NumberOfRooms";
    	public static readonly string PROP_NAME_RESERVATIONTOTAL = "ReservationTotal";
    	public static readonly string PROP_NAME_COUCHBASEID = "CouchBaseId";
    	public static readonly string PROP_NAME_PROPERTY_UID = "Property_UID";
    	public static readonly string PROP_NAME_CREATEDDATE = "CreatedDate";
    	public static readonly string PROP_NAME_TOTALRESERVATION = "TotalReservation";
    
        public long UID { get; set; }
        public Nullable<System.DateTime> CheckIn { get; set; }
        public Nullable<System.DateTime> CheckOut { get; set; }
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public Nullable<int> NumberOfRooms { get; set; }
        public string ReservationTotal { get; set; }
        public string CouchBaseId { get; set; }
        public long Property_UID { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<decimal> TotalReservation { get; set; }
    }
}