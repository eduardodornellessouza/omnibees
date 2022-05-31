
using OB.BL.Operations.Internal.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.Reservation.BL.Contracts.Requests;

namespace OB.BL.Operations.Helper
{
    public static class ManageFilteringHelper
    {
        /// <summary>
        /// Giving the request, sends out the new filter or the list of old filters (converted) and ready to use or to send to the repository.
        /// </summary>
        /// <param name="newFilter"></param>
        /// <param name="oldFilters"></param>
        /// <param name="request"></param>
        public static void ManageOldAndNewFilters(out Kendo.DynamicLinq.Filter newFilter, out List<DL.Common.Filter.FilterByInfo> oldFilters, GridPagedRequest request)
        {
            newFilter = null;
            oldFilters = null;

            if (request.NestedFilters != null)
            {
                newFilter = OtherConverter.Convert(request.NestedFilters);
            }
            else if (request.Filters != null && request.Filters.Any())
            {
                oldFilters = request.Filters.Select(x => OtherConverter.Convert(x)).ToList();
            }
        }
    }
}
