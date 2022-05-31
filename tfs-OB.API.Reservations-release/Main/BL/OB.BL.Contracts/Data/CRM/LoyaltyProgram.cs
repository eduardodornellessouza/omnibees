using  contractsGeneral = OB.Reservation.BL.Contracts.Data.General;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Data.CRM
{
    [DataContract]
    public class LoyaltyProgram : ContractBase
    {
        public LoyaltyProgram()
        { 
        }

        [DataMember]
        public long UID { get; set; }

        [DataMember]
        public long Client_UID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public byte[] AttachmentPDF { get; set; }

        [DataMember]
        public string AttachmentPDFName { get; set; }

        [DataMember]
        public byte[] BackgroundImage { get; set; }

        [DataMember]
        public string BackgroundImageName { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public bool OnlySearchesByLoyaltyRates { get; set; }

        [DataMember]
        public long CreatedBy { get; set; }

        [DataMember]
        public long ModifiedBy { get; set; }

        [DataMember]
        public System.DateTime CreatedDate { get; set; }

        [DataMember]
        public System.DateTime ModifiedDate { get; set; }

        [DataMember]
        public byte[] Revision { get; set; }

        [DataMember]
        public long CreatedByPropertyUID { get; set; }

        [DataMember]
        public string CreatedByUsername { get; set; }

        [DataMember]
        public string ModifiedByUsername { get; set; }
        
        [DataMember]
        public contractsGeneral.Currency DefaultCurrency { get; set; }

        [DataMember]
        public contractsGeneral.Language DefaultLanguage { get; set; }


        [DataMember]
        public string AttachmentPDFUrl { get; set; }

        [DataMember]
        public string BackgroundImageUrl { get; set; }


        [DataMember]
        public List<LoyaltyLevel> LoyaltyLevels { get; set; }

        [DataMember]
        public List<LoyaltyProgramsLanguage> LoyaltyProgramsLanguages { get; set; }
    }
}
