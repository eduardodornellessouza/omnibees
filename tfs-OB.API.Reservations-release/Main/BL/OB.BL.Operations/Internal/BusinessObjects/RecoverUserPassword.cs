using static OB.BL.Constants;

namespace OB.BL.Operations.Internal.BusinessObjects
{
    public class RecoverUserPassword
    {
        public long PropertyId { get; set; }
        public BookingEngineUserType UserType { get; set; }
        public string Email { get; set; }
        public long LanguageId { get; set; }
        public long ClientId { get; set; }
    }
}
