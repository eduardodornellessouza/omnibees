using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.BL.Contracts.Data.Properties;

namespace OB.DL.Common.QueryResultObjects
{
    public class TransferLocationQR1
    {
        public long UID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public Nullable<decimal> Price { get; set; }
        public long Property_UID { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<TransferLocationsLanguageQR1> TransferLocationsLanguages { get; set; }
        public virtual PropertyLight Property { get; set; }
    }
}
