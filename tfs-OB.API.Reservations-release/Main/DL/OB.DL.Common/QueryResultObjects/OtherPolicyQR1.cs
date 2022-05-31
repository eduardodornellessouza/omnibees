using System;

namespace OB.DL.Common.QueryResultObjects
{
    //[MetadataTypeAttribute(typeof(OtherPolicy.OtherPolicyMetadata))]
    public partial class OtherPolicyQR1
    {
        public long UID { get; set; }

        public Nullable<long> Property_UID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string TranslatedName { get; set; }

        public string TranslatedDescription { get; set; }

        public string Language { get; set; }

        public Boolean IsDelete { get; set; }

        public Boolean IsSelected { get; set; }
    }
}