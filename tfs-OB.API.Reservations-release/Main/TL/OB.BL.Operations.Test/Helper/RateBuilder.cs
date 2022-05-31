using OB.BL.Contracts.Data.Rates;
using OB.DL.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.Api.Core;

namespace OB.BL.Operations.Test.Helper
{
    public class RateBuilder
    {
        public static RateRoomDetail GetRateRoomDetail(long rateRoomDetailUid, IRepositoryFactory repoFactory)
        {
            var rateRepo = repoFactory.GetOBRateRoomDetailsForReservationRoomRepository();
            return rateRepo.ListRateRoomDetails(new Contracts.Requests.ListRateRoomDetailsRequest() { UIDs = new List<long>() { rateRoomDetailUid } }).FirstOrDefault();
        }
        
        public static RateRoomDetail ChangeRateRoomDetailAllotment(long rateRoomDetailId, int allotment, IUnitOfWork unitOfWork, IRepositoryFactory repoFactory)
        {
            // TODO: CORRIGIR. Este metodo nao funciona, pois está a modificar contracts em vez de domains.

            var rateRepo = repoFactory.GetOBRateRoomDetailsForReservationRoomRepository();
            var rateRoomDetail = rateRepo.ListRateRoomDetails(new Contracts.Requests.ListRateRoomDetailsRequest() { UIDs = new List<long>() { 5137349 } }).FirstOrDefault();
            rateRoomDetail.Allotment = allotment;
            unitOfWork.Save();
            return rateRoomDetail;
        }
    }
}
