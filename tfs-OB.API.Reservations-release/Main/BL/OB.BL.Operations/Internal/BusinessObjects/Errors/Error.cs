using System.ComponentModel.DataAnnotations;

namespace OB.BL.Operations.Internal.BusinessObjects.Errors
{
    public class Error
    {
        public string Type { get; }

        public int Code { get; }
        public string Description { get; }

        public Error(string type, int code, string description)
        {
            Type = type;
            Code = code;
            Description = description;
        }
    }

    public enum Errors
    {
        [Display(Description = "Live Unit Of Work is being passed to Background Thread. Method being called")]
        LiveUnitOfWorkOnBackgroundThread = -20000,

        [Display(Description = "Payment was authorized or captured but failed on antifraude analysis")]
        PaymentAuthorizedButFailedOnAntifraude = -20001,

        [Display(Description = "Error Sending Notification to MSMQ")]
        SendNotificationError = -20002,
    }
}
