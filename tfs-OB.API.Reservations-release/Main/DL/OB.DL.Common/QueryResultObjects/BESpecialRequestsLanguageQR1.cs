using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.QueryResultObjects
{
    public class BESpecialRequestsLanguageQR1
    {
        public long UID { get; set; }
        public string Name { get; set; }
        public long Language_UID { get; set; }
        public long BESpecialRequest_UID { get; set; }

        public virtual BESpecialRequestQR1 BESpecialRequest { get; set; }
    }
}
