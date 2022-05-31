using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.DL.Common.Criteria;
using domain = OB.Domain.Reservations;

namespace OB.DL.Common.Repositories.Interfaces.Entity
{
    public interface ILostReservationsRepository : IRepository<domain.LostReservation>
    {
        IQueryable<Domain.Reservations.LostReservation> FindByCriteria(ListLostReservationCriteria criteria);
    }
}
