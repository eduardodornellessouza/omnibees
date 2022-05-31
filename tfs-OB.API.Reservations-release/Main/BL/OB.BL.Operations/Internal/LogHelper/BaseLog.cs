using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.LogHelper
{
    internal class BaseLog
    {
        public long ModifiedByPropertyUID { get; set; }
        public string ModifiedByPropertyName { get; set; }
        public long ModifiedByUserUID { get; set; }
        public string ModifiedByUserName { get; set; }
    }
}
