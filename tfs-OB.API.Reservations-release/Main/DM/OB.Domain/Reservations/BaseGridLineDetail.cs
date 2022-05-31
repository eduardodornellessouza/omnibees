using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Domain.Reservations
{
    public class BaseGridLineDetail : DomainObject
    {
        public BaseGridLineDetail()
        {
        }

        public int Action { get; set; }
        public string CreatedByUsername { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public string GridLineId { get; set; }
        public string LogId { get; set; }
        public long PropertyId { get; set; }
        public List<int> SubActions { get; set; }
    }
}
