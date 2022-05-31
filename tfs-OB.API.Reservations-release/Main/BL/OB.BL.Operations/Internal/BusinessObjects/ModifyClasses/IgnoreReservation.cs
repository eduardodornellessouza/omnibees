using OB.Reservation.BL.Contracts.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using contractsProperties = OB.BL.Contracts.Data.Properties;
using contractsReservations = OB.Reservation.BL.Contracts.Data.Reservations;

namespace OB.BL.Operations.Internal.BusinessObjects.ModifyClasses
{
    public class IgnoreReservationResult
    {
        public IgnoreReservationResult()
        {
            OldReservation = null;
            FinalReservation = null;
            SendOperatorCreditLimitExcededEmail = false;
            SendTPICreditLimitExcededEmail = false;
            Log = null;
            AdditionalData = null;
            Inventories = new List<contractsProperties.Inventory>();
        }

        public contractsReservations.Reservation FinalReservation { get; set; }
        public contractsReservations.Reservation OldReservation { get; set; }
        public bool SendOperatorCreditLimitExcededEmail { get; set; }
        public bool SendTPICreditLimitExcededEmail { get; set; }
        public ReservationLoggingMessage Log { get; set; }
        public OB.Domain.Reservations.ReservationsAdditionalData AdditionalData { get; set; }
        public List<contractsProperties.Inventory> Inventories { get; set; }
    }
}
