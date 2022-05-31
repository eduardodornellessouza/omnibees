using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OB.Reservation.BL.Contracts.Requests.Base
{
    [DataContract]
    public enum ApplicationEnum
    {
        Omnibees = 1,
        PortalOperadoras = 2
    }
}
