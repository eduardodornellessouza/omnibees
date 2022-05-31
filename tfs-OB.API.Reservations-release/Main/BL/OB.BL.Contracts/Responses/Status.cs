using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Responses
{
    [DataContract]
    public enum Status
    {
        [EnumMember(Value = "Success")]
        Success = 0,

        [EnumMember(Value = "PartialSuccess")]
        PartialSuccess = 1,

        [EnumMember(Value = "Fail")]
        Fail = 2
    }
}