using System;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public partial class SystemTemplatesCustom
    {
        public int Code { get; set; }

        public string HtmlContent { get; set; }

        public string Name { get; set; }

        public long UID { get; set; }

        public bool IsDeleted { get; set; }

        public Nullable<long> SystemTemplatesCategory_UID { get; set; }

        public Nullable<long> Property_UID { get; set; }

    }

}
