using OB.BL.Operations.Interfaces;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using OB.REST.Services.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace OB.REST.Services.Controllers
{
    public class TokenizedCreditCardsReadsPerMonthController : ApiController
    {
        private ITokenizedCreditCardsReadsPerMonthManagerPOCO _tokenizedCreditCardsReadsPerMonthManagerPOCO;

        public TokenizedCreditCardsReadsPerMonthController(ITokenizedCreditCardsReadsPerMonthManagerPOCO tokenizedCreditCardsReadsPerMonthManagerPOCO)
        {
            _tokenizedCreditCardsReadsPerMonthManagerPOCO = tokenizedCreditCardsReadsPerMonthManagerPOCO;
        }

        [AcceptVerbs("POST")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public FindTokenizedCreditCardsReadsPerMonthByCriteriaResponse FindTokenizedCreditCardsReadsPerMonthByCriteria(FindTokenizedCreditCardsReadsPerMonthByCriteriaRequest request)
        {
            return _tokenizedCreditCardsReadsPerMonthManagerPOCO.FindTokenizedCreditCardsReadsPerMonthByCriteria(request);
        }

        [AcceptVerbs("POST")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IncrementTokenizedCreditCardsReadsPerMonthByCriteriaResponse IncrementTokenizedCreditCardsReadsPerMonthByCriteria(IncrementTokenizedCreditCardsReadsPerMonthByCriteriaRequest request)
        {
            return _tokenizedCreditCardsReadsPerMonthManagerPOCO.IncrementTokenizedCreditCardsReadsPerMonthByCriteria(request);
        }
    }
}
