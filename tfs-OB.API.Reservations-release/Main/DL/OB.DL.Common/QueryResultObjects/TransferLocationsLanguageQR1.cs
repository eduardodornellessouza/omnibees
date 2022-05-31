using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.QueryResultObjects
{
    public class TransferLocationsLanguageQR1
    {
        public long UID { get; set; }
        public long TransferLocations_UID { get; set; }
        public long Languages_UID { get; set; }
        public string Name { get; set; }

        public virtual TransferLocationQR1 TransferLocation { get; set; }
    }
}
