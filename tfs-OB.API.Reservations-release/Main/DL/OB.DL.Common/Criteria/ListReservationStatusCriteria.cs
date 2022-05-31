using System.Collections.Generic;
using OB.DL.Common.Filter;
using OB.Domain.Reservations;


namespace OB.DL.Common.Criteria
{
    public class ListReservationStatusCriteria 
    {
        public IEnumerable<long> UIDs { get; set; }
        public bool IncludeLanguages { get; set; }

        public List<SortByInfo> Orders { get; set; } = new List<SortByInfo>
        {
            new SortByInfo
            {
                OrderBy = nameof(ReservationStatus.UID),
                Direction = SortDirection.Ascending
            }
        };

        public int TotalRecords { get; set; }
    }
}
