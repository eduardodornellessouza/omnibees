using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Interfaces
{
    public interface ILogsManagerPOCO : IBusinessPOCOBase
    {
        ListLogsResponse ListLogs(ListLogsRequest request);

        ListReservationGridLogsResponse ListReservationGridLogs(ListFriendlyLogsRequest request);

    }
}
