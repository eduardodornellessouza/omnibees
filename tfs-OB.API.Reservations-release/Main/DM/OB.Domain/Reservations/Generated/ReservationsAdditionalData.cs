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
    public partial class ReservationsAdditionalData : DomainObject
    {
    	public static readonly DomainScope DomainScope = DomainScopes.Reservations;
    
    	public static readonly string PROP_NAME_UID = "UID";
    	public static readonly string PROP_NAME_RESERVATION_UID = "Reservation_UID";
    	public static readonly string PROP_NAME_RESERVATIONADDITIONALDATAJSON = "ReservationAdditionalDataJSON";
    	public static readonly string PROP_NAME_ISFROMNEWINSERT = "IsFromNewInsert";
    	public static readonly string PROP_NAME_CHANNELPARTNERID = "ChannelPartnerID";
    	public static readonly string PROP_NAME_PARTNERRESERVATIONNUMBER = "PartnerReservationNumber";
    	public static readonly string PROP_NAME_REVISION = "Revision";
    	public static readonly string PROP_NAME_RESERVATIONDOMAIN = "ReservationDomain";
    	public static readonly string PROP_NAME_BOOKINGENGINETEMPLATE = "BookingEngineTemplate";
    	public static readonly string PROP_NAME_ISDIRECTRESERVATION = "isDirectReservation";
    	public static readonly string PROP_NAME_BOOKERISGENIUS = "BookerIsGenius";
    	public static readonly string PROP_NAME_BIGPULLAUTHREQUESTOR_UID = "BigPullAuthRequestor_UID";
    	public static readonly string PROP_NAME_BIGPULLAUTHOWNER_UID = "BigPullAuthOwner_UID";
    
        public long UID { get; set; }
        public long Reservation_UID { get; set; }
        public string ReservationAdditionalDataJSON { get; set; }
        public Nullable<bool> IsFromNewInsert { get; set; }
        public Nullable<int> ChannelPartnerID { get; set; }
        public string PartnerReservationNumber { get; set; }
        public byte[] Revision { get; set; }
        public string ReservationDomain { get; set; }
        public string BookingEngineTemplate { get; set; }
        public Nullable<bool> isDirectReservation { get; set; }
        public Nullable<bool> BookerIsGenius { get; set; }
        public Nullable<long> BigPullAuthRequestor_UID { get; set; }
        public Nullable<long> BigPullAuthOwner_UID { get; set; }
    }
}