using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.QueryResultObjects
{
    public class BESpecialRequestQR1
    {
        public long UID { get; set; }
        public string Name { get; set; }
        public long Property_UID { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<BESpecialRequestsLanguageQR1> BESpecialRequestsLanguages { get; set; }
    }
}
