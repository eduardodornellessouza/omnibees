using OB.BL.Operations.Interfaces;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.REST.Services.Attributes;
using System.Web.Http;
using System.Security.Permissions;

namespace OB.REST.Services.Controllers
{
    /// <summary>
    /// OmniBees Reservation RESTful Service that provide operations for:
    /// <para/>InsertPaymentGatewayTransation
    /// <para/>UpdatePaymentGatewayTransation
    /// <para/>InsertPaymentGatewayTransationDetails
    /// <para/>UpdatePaymentGatewayTransationDetails
    /// </summary>  
    public class PaymentGatewayTransactionsController : BaseController
    {
        private IPaymentGatewayManagerPOCO _paymentGatewayManager;

        public PaymentGatewayTransactionsController(IPaymentGatewayManagerPOCO paymentGatewayManager)
        {
            _paymentGatewayManager = paymentGatewayManager;
        }

        [AcceptVerbs("POST")]
        [RequiresPermission("Admin")]
        [System.Web.Http.Description.ApiExplorerSettings(IgnoreApi = true)]
        public InsertPaymentGatewayTransationResponse InsertPaymentGatewayTransation(InsertPaymentGatewayTransationRequest request)
        {
            return _paymentGatewayManager.InsertPaymentGatewayTransation(request);
        }

        [AcceptVerbs("POST")]
        [RequiresPermission("Admin")]
        [System.Web.Http.Description.ApiExplorerSettings(IgnoreApi = true)]
        public UpdatePaymentGatewayTransationResponse UpdatePaymentGatewayTransation(UpdatePaymentGatewayTransationRequest request)
        {
            return _paymentGatewayManager.UpdatePaymentGatewayTransation(request);
        }

        [AcceptVerbs("POST")]
        [RequiresPermission("Admin")]
        [System.Web.Http.Description.ApiExplorerSettings(IgnoreApi = true)]
        public InsertPaymentGatewayTransationDetailsResponse InsertPaymentGatewayTransationDetails(InsertPaymentGatewayTransationDetailsRequest request)
        {
            return _paymentGatewayManager.InsertPaymentGatewayTransationDetails(request);
        }

        [AcceptVerbs("POST")]
        [RequiresPermission("Admin")]
        [System.Web.Http.Description.ApiExplorerSettings(IgnoreApi = true)]
        public UpdatePaymentGatewayTransationDetailsResponse UpdatePaymentGatewayTransationDetails(UpdatePaymentGatewayTransationDetailsRequest request)
        {
            return _paymentGatewayManager.UpdatePaymentGatewayTransationDetails(request);
        }
    }
}