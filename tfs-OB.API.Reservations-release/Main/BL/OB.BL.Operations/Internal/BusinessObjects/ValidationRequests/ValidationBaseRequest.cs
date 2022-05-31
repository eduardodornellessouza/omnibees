using OB.BL.Contracts.Data.General;
using OB.Domain.Reservations;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public abstract class ValidationBaseRequest
    {
        public GroupRule GroupRule { get; set; }

        public string RequestId { get; set; }
    }
}
