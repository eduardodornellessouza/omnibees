using OB.Api.Core;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Couchbase;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.DL.Common.Repositories.Interfaces.SqlServer;
using OB.Domain;

namespace OB.DL.Common.Interfaces
{
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Gets the SQL manager that allows query execution agains the given UnitOfWork.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        ISqlManager GetSqlManager(IUnitOfWork unitOfWork, DomainScope scope);

        ISqlManager GetSqlManager(string key);

        /// <summary>
        /// Gets a repository
        /// </summary>
        IRepository<T> GetRepository<T>(IUnitOfWork unitOfWork) where T : DomainObject;

        /// <summary>
        /// Gets the ReservationStatus specialized Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        IReservationStatusRepository GetReservationStatusRepository(IUnitOfWork unitOfWork);

        /// <summary>
        /// Gets the CancelReservationReason specialized Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        ICancelReservationReasonRepository GetCancelReservationReasonRepository(IUnitOfWork unitOfWork);

        /// <summary>
        /// Gets the Reservation specialized Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        IReservationsRepository GetReservationsRepository(IUnitOfWork unitOfWork);

        /// <summary>
        /// Gets the ReservationHistoryRepository specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        IReservationHistoryRepository GetReservationHistoryRepository(IUnitOfWork unitOfWork);

        /// <summary>
        /// Gets the ReservationRoom specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        IReservationRoomRepository GetReservationRoomRepository(IUnitOfWork unitOfWork);

        /// <summary>
        /// Gets the ReservationRoomDetail specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        IReservationRoomDetailRepository GetReservationRoomDetailRepository(IUnitOfWork unitOfWork);

        /// <summary>
        /// Gets the LostReservationDetailRepository specialized Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        ILostReservationDetailRepository GetLostReservationDetailRepository(IUnitOfWork unitOfWork);        

        /// <summary>
        /// Gets the GroupRules Cached Repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of Work</param>
        /// <returns></returns>
        IGroupRulesRepository GetGroupRulesRepository(IUnitOfWork unitOfWork);
        
        /// <summary>
        /// Gets the GetGuestCachedRepository specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        //IGuestCachedRepository GetGuestCachedRepository(IUnitOfWork unitOfWork);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        INotificationBaseRepository GetNotificationBaseRepository();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IReservationLogRepository GetReservationLogRepository();

        IPortalRepository GetPortalRepository(IUnitOfWork unitOfWork);

        IReservationsFilterRepository GetReservationsFilterRepository(IUnitOfWork unitOfWork);

        IReservationRoomFilterRepository GetReservationRoomFilterRepository(IUnitOfWork unitOfWork);

        ITokenizedCreditCardsReadsPerMonthRepository GetTokenizedCreditCardsReadsPerMonthRepository(IUnitOfWork unitOfWork);

        IVisualStateRepository GetVisualStateRepository(IUnitOfWork unitOfWork);

        IOBPromotionalCodeRepository GetOBPromotionalCodeRepository();

        IOBRateBuyerGroupRepository GetOBRateBuyerGroupRepository();

        IOBChildTermsRepository GetOBChildTermsRepository();

        IOBIncentiveRepository GetOBIncentiveRepository();

        IOBRateRoomDetailsForReservationRoomRepository GetOBRateRoomDetailsForReservationRoomRepository();

        IOBCurrencyRepository GetOBCurrencyRepository();

        IOBCancellationPolicyRepository GetOBCancellationPolicyRepository();

        IOBDepositPolicyRepository GetOBDepositPolicyRepository();

        IOBOtherPolicyRepository GetOBOtherPolicyRepository();

        IOBExtrasRepository GetOBExtrasRepository();

        IOBPaymentMethodTypeRepository GetOBPaymentMethodTypeRepository();

        IOBAppSettingRepository GetOBAppSettingRepository();

        IOBChannelRepository GetOBChannelRepository();
        IOBPropertyRepository GetOBPropertyRepository();
        IExternalSystemsRepository GetExternalSystemsRepository();

        IOBCRMRepository GetOBCRMRepository();

        IOBUserRepository GetOBUserRepository();

        IOBSecurityRepository GetOBSecurityRepository();

        IOBRateRepository GetOBRateRepository();

        IOBPropertyEventsRepository GetOBPropertyEventsRepository();

        IOBPMSRepository GetOBPMSRepository();

        IOBReservationLookupsRepository GetOBReservationLookupsRepository();

        IOBBePartialPaymentCcMethodRepository GetOBBePartialPaymentCcMethodRepository();

        IOBBeSettingsRepository GetOBBeSettingsRepository();

        /// <summary>
        /// Gets the PaymentGatewayTransactionRepository specialized Repository.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        IPaymentGatewayTransactionRepository GetPaymentGatewayTransactionRepository(IUnitOfWork unitOfWork);

        IThirdPartyIntermediarySqlRepository GetThirdPartyIntermediarySqlRepository(IUnitOfWork unitOfWork);

        IOperatorSqlRepository GetOperatorSqlRepository(IUnitOfWork unitOfWork);

        ILostReservationsRepository GetLostReservationsRepository(IUnitOfWork unitOfWork);
    }
}