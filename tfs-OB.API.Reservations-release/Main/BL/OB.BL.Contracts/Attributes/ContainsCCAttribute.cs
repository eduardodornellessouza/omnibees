using System;

namespace OB.Reservation.BL.Contracts.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class ContainsCCAttribute : MaskFilterAttribute
    {
    }
}
