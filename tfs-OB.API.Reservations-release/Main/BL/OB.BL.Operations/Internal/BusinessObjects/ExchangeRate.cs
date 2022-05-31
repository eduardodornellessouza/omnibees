using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class ExchangeRate
    {
        public long UID { get; set; }
        public long Currency_UID { get; set; }
        public decimal Rate { get; set; }
        public System.DateTime Date { get; set; }
        public System.DateTime UpdateDate { get; set; }

        public virtual OB.BL.Contracts.Data.General.Currency Currency { get; set; }
    }
}
