using Dapper;
using OB.Api.Core;
using OB.BL;
using OB.DL.Common.Criteria;
using OB.DL.Common.Impl;
using OB.DL.Common.Infrastructure;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Transactions;
using domainReservation = OB.Domain.Reservations;

namespace OB.DL.Common.Repositories.Impl.Entity
{
    internal class ReservationsRepository : Repository<domainReservation.Reservation>, IReservationsRepository
    {
        private static readonly Dictionary<long, List<uint>> __reservationsSeqCache = new Dictionary<long, List<uint>>();
        private static int __reservationNumberSeqRange = 10;
        private readonly IRepository<domainReservation.ReservationsAdditionalData> reservationAddicionalDataRepo;
        private static string DB_Name = ConfigurationManager.AppSettings["DB_Name"];

        #region SQLQueries

        static readonly string QUERY_FindPropertiesWithReservationsForChannelOrTPI = @"
                    IF @tpiUID IS NULL
                          BEGIN 
	                            SELECT UID, Name 
	                            FROM [" + DB_Name + @"].[dbo].Properties WITH(NOLOCK) WHERE UID IN (
		                            SELECT DISTINCT(Property_UID) FROM Reservations WITH(NOLOCK) WHERE Channel_UID = @channelUID and TPI_UID IS NULL {0}
		                            )
	                            ORDER BY Name
                          END 
                    ELSE 
                          BEGIN 
	                            SELECT UID, Name 
	                            FROM [" + DB_Name + @"].[dbo].Properties WITH(NOLOCK) WHERE UID IN (
		                            SELECT DISTINCT(Property_UID) FROM Reservations WITH(NOLOCK) WHERE Channel_UID = @channelUID and TPI_UID = @tpiUID {0}
		                            )
	                            ORDER BY Name 
                          END 
            ";

        //        private static readonly string QUERY_FindPropertiesWithReservationsForChannelsTpis = @"
        //                {0}
        //                {1}
        //                {2}
        //                DECLARE @result TABLE (UID BIGINT NOT NULL, Name NVARCHAR(200) NOT NULL);  -- RETURN THE RESULT HERE
        //
        //                DECLARE @i INT = 0;
        //                DECLARE @channelUid BIGINT;
        //                DECLARE @tpiUID BIGINT;
        //                WHILE(@i <= (SELECT COUNT(*) FROM @channelsTPIs))
        //                    BEGIN
        //	                    SET @channelUid = (SELECT ChannelUid FROM (SELECT ROW_NUMBER() OVER(ORDER BY ChannelUid) AS RowNum, ChannelUid FROM @channelsTPIs) AS tempT
        //						    WHERE tempT.RowNum = @i);
        //	                    SET @tpiUID = (SELECT TPI_UID FROM (SELECT ROW_NUMBER() OVER(ORDER BY ChannelUid) AS RowNum, TPI_UID FROM @channelsTPIs) AS tempT
        //						    WHERE tempT.RowNum = @i);
        //
        //	                    -- for each channel-tpi
        //	                    IF @tpiUID IS NULL
        //	                    BEGIN 
        //		                    INSERT INTO @result 
        //			                    SELECT UID, Name 
        //			                    FROM Properties WITH(NOLOCK) WHERE UID IN (
        //				                    SELECT DISTINCT(Property_UID) FROM Reservations WITH(NOLOCK) WHERE Channel_UID = @channelUID and TPI_UID IS NULL
        //					                {3}
        //			                    )
        //			                    ORDER BY Name
        //	                    END
        //	                ELSE
        //	                BEGIN 
        //		                INSERT INTO @result
        //			                SELECT UID, Name 
        //			                FROM Properties WITH(NOLOCK) WHERE UID IN (
        //				                SELECT DISTINCT(Property_UID) FROM Reservations WITH(NOLOCK) WHERE Channel_UID = @channelUID and TPI_UID = @tpiUID
        //					                {3}
        //			                    )
        //			                ORDER BY Name 
        //	                END
        //
        //	                SET @i = @i + 1;
        //                END
        //
        //            -- Return to the repository
        //            SELECT DISTINCT * FROM @result
        //            ";
        //        private static readonly string QUERY_DeclareVars = @"
        //            DECLARE @channelsTPIs TABLE(ChannelUid BIGINT NOT NULL, TPI_UID BIGINT NULL);
        //            DECLARE @properties TABLE (PropUid BIGINT);
        //        ";
        //        private static readonly string Template_AddValuesChannelsAndTpis = @"
        //            INSERT INTO @channelsTPIs VALUES({0}, {1});
        //        ";
        //        private static readonly string Template_AddValuesProps = @"
        //            INSERT INTO @properties VALUES({0});
        //        ";

        static readonly string QUERY_FindPropertiesNameUidProps = @"
            select UID, Name from [" + DB_Name + @"].[dbo].Properties
            where UID IN ({0})
            order by Name
        ";
        static readonly string QUERY_FindPropertiesNameUid = @"
            select UID, Name from [" + DB_Name + @"].[dbo].Properties
            where IsActive = 1
            order by Name
        ";

        static readonly string CREATE_SEQUENCE_SCRIPT = @"
            --IF NOT EXISTS(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RESERVATION_NUMBER_SEQ_PROP_UID_{0}]') AND type = 'SO')
            --BEGIN
            CREATE SEQUENCE [dbo].[RESERVATION_NUMBER_SEQ_PROP_UID_{0}]
                START WITH {1}
	            INCREMENT BY 1
	            CYCLE
	            MINVALUE 1
                MAXVALUE 999999
	            CACHE 32
            --END";

        static readonly string SEQUENCE_SCRIPT = @"
            EXEC sp_sequence_get_range
            @sequence_name = N'dbo.RESERVATION_NUMBER_SEQ_PROP_UID_{0}'
            , @range_size = {1}
            , @range_first_value = @range_first_value_output OUTPUT ;

        ";

        static readonly string GET_SEQUENCE_COUNT_SCRIPT = @"SELECT count(name) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RESERVATION_NUMBER_SEQ_PROP_UID_{0}]') AND type = 'SO'";

        static readonly string QUERY_GetReservationBasicInfoForTransactionID = @"
                SELECT TOP 1 UID, ReservationCurrency_UID, Channel_UID from Reservations WITH(NOLOCK) 
                WHERE Property_UID = @PropertyUID and PaymentGatewayTransactionID = @PaymentGatewayTransactionID
            ";

        static readonly string QUERY_GetReservationTransactionStatusBasicInfoForReservationUID = @"
                SELECT TOP 1 TransactionUID, TransactionState, HangfireId from ReservationTransactionStatus WITH(NOLOCK) 
                WHERE ReservationUID = @ReservationUID
            ";

        static readonly string QUERY_FindRoomTypesBy_ReservationUID_And_SystemEventCode = $@"
                    select top 1 rt.UID
                    from [{ DB_Name }].[dbo].RoomTypes rt WITH(NOLOCK)
                    join ReservationRooms rr WITH(NOLOCK) on rt.UID = rr.RoomType_UID
                    join ReservationRoomDetails rrd WITH(NOLOCK) on rrd.ReservationRoom_UID = rr.UID
                    join Reservations r WITH(NOLOCK)  on rr.Reservation_UID = r.UID
                    join [{ DB_Name }].[dbo].PropertyEvents pe WITH(NOLOCK) on rt.Property_UID = pe.Property_UID
                    join [{ DB_Name }].[dbo].SystemEvents se WITH(NOLOCK) on pe.SystemEvent_UID = se.UID
                    join [{ DB_Name }].[dbo].PropertyEventOccupancyAlerts peoa WITH(NOLOCK) on pe.UID = peoa.PropertyEvent_UID
                    join [{ DB_Name }].[dbo].Inventory i WITH(NOLOCK) on rt.UID = i.RoomType_UID
                    left join [{ DB_Name }].[dbo].RoomTypePeriods rtp WITH(NOLOCK) on rtp.RoomType_UID = rt.UID and rrd.[Date] >= rtp.DateFrom and rrd.[Date] <= rtp.DateTo
                    where r.UID = @p0 and peoa.RoomType_UID = rt.UID
                    and se.Code = @p1 and pe.IsDelete = 0
                    and rr.DateFrom is not null and rr.DateTo is not null
                    and i.Date >= rr.DateFrom and i.Date < rr.DateTo
                    and peoa.IsNotify = 1
                    and (ISNULL(rtp.Qty, rt.Qty) - i.QtyRoomOccupied) < peoa.NotifyOn";

        static readonly string FIND_OCCUPANCY_ALERTS_BY_ROOMTYPEUID_CODE_PROPERTYUID_MULTIPLEDATES = $@"
                    select 
	                    peoa.UID as UID, 
	                    i.Date as [Date], 
	                    rmt.UID as RoomType_UID, 
	                    peoa.IsActivateCloseSalesInBE as IsActivateCloseSalesInBE,
                        rmt.Property_UID as Property_UID,
                        rr.Rate_UID as Rate_UID, 
                        se.Code
                    from [{ DB_Name }].[dbo].PropertyEventOccupancyAlerts peoa WITH (NOLOCK)
                    join [{ DB_Name }].[dbo].PropertyEvents pe  WITH (NOLOCK) on peoa.PropertyEvent_UID = pe.UID
                    join [{ DB_Name }].[dbo].SystemEvents se WITH (NOLOCK) on pe.SystemEvent_UID = se.UID
                    join [{ DB_Name }].[dbo].RoomTypes rmt WITH (NOLOCK) on peoa.RoomType_UID = rmt.UID
                    join [{ DB_Name }].[dbo].RateRooms rr WITH (NOLOCK) on rmt.UID = rr.RoomType_UID
                    join [{ DB_Name }].[dbo].Inventory i WITH (NOLOCK) on rmt.UID = i.RoomType_UID
                    left join [{ DB_Name }].[dbo].RoomTypePeriods rtp WITH (NOLOCK) on rtp.RoomType_UID = rmt.UID and i.[Date] >= rtp.DateFrom and i.[Date] <= rtp.DateTo
                    where
                    peoa.RoomType_UID = @RoomType_UID
                    and se.Code = @StrCode
                    and pe.Property_UID = @Property_UID
                    and pe.IsDelete = 0
                    and (ISNULL(rtp.Qty, rmt.Qty) - i.QtyRoomOccupied) < peoa.CloseSalesOn
                    and peoa.IsActivateCloseSales = 1
                    and rmt.IsDeleted = 0
                    and rr.IsDeleted = 0
                    and i.Date IN ({{0}})
                    ";
        static readonly string QUERY_BOOKINGENGINE_CHANNELUID = @"
            SELECT UID from [" + DB_Name + @"].[dbo].Channels with(nolock)
            WHERE IsBookingEngine = 1";

        static readonly string UPDATE_ALLOTMENT_BATCH_QUERY_WITH_VALIDATION = @"
            UPDATE {3}.dbo.RateRoomDetails
            SET {3}.dbo.RateRoomDetails.AllotmentUsed = (CASE WHEN ISNULL({3}.dbo.RateRoomDetails.AllotmentUsed, 0) + {1} < 0 THEN 0 ELSE (ISNULL({3}.dbo.RateRoomDetails.AllotmentUsed,0) + {1}) END),
            {3}.dbo.RateRoomDetails.ModifyDate = GETDATE(),
            {3}.dbo.RateRoomDetails.correlationID = '{2}'
            WHERE {3}.dbo.RateRoomDetails.UID = {0}
            AND (({3}.dbo.RateRoomDetails.AllotmentUsed is null AND ({3}.dbo.RateRoomDetails.Allotment - {1}) >= 0)
                  OR
                 ({3}.dbo.RateRoomDetails.Allotment - {3}.dbo.RateRoomDetails.AllotmentUsed - {1}) >= 0)";

        static readonly string UPDATE_ALLOTMENT_BATCH_QUERY = @"
            UPDATE {3}.dbo.RateRoomDetails
            SET {3}.dbo.RateRoomDetails.AllotmentUsed = (CASE WHEN ISNULL({3}.dbo.RateRoomDetails.AllotmentUsed, 0) + {1} < 0 THEN 0 ELSE (ISNULL({3}.dbo.RateRoomDetails.AllotmentUsed,0) + {1}) END),
            {3}.dbo.RateRoomDetails.ModifyDate = GETDATE(),
            {3}.dbo.RateRoomDetails.correlationID = '{2}'
            WHERE {3}.dbo.RateRoomDetails.UID = {0}";

        static readonly string UPDATE_VCN_QUERY = @"
            IF EXISTS(SELECT 1 FROM Reservations WITH(NOLOCK) WHERE UID = @reservationId)
            BEGIN
	            DECLARE @dateNow datetime = GETUTCDATE();

	            -- Insert or Update the VCNReservationId and VCNToken
	            MERGE dbo.ReservationPaymentDetails AS target  
	            USING (SELECT 
				            @reservationId, 
				            @vcnReservationId, 
				            @vcnToken,
				            @cvv,
				            @cardHolderName,
				            @cardNumber,
				            @cardExpireDate,
                            @cardHashCode,
				            @dateNow, 
				            @dateNow,
				            0,
				            0) 
		            AS source (
				            Reservation_UID, 
				            VCNReservationId, 
				            VCNToken, 
				            CVV,
				            CardName,
				            CardNumber,
				            ExpirationDate,
                            HashCode,
				            CreatedDate, 
				            ModifiedDate, 
				            PaymentGatewayTokenizationIsActive, 
				            OBTokenizationIsActive)  
	            ON (target.Reservation_UID = source.Reservation_UID)  
	            WHEN MATCHED THEN
		            UPDATE SET 
			            VCNReservationId = source.VCNReservationId,
			            VCNToken = source.VCNToken,
			            CVV = source.CVV,
			            CardName = source.CardName,
			            CardNumber = source.CardNumber,
			            ExpirationDate = source.ExpirationDate,
                        HashCode = source.HashCode,
			            ModifiedDate = source.ModifiedDate
	            WHEN NOT MATCHED THEN
		            INSERT ([Reservation_UID]
			            ,[CreatedDate]
			            ,[ModifiedDate]
			            ,[VCNReservationId]
			            ,[VCNToken]
			            ,[CVV]
			            ,[CardName]
			            ,[CardNumber]
			            ,[ExpirationDate]
                        ,[HashCode]
			            ,[PaymentGatewayTokenizationIsActive]
			            ,[OBTokenizationIsActive])
		            VALUES (
			             source.[Reservation_UID]
			            ,source.[CreatedDate]
			            ,source.[ModifiedDate]
			            ,source.[VCNReservationId]
			            ,source.[VCNToken]
			            ,source.[CVV]
			            ,source.[CardName]
			            ,source.[CardNumber]
			            ,source.[ExpirationDate]
                        ,source.[HashCode]
			            ,source.[PaymentGatewayTokenizationIsActive]
			            ,source.[OBTokenizationIsActive]); 

	            -- Updates modified date of Reservation
	            UPDATE dbo.Reservations
	            SET dbo.Reservations.ModifyDate = @dateNow
	            WHERE UID = @reservationId
            END";

        #endregion

        public DbContext dbContext { get; set; }
        public IOBRateRepository rateRepo { get; set; }

        static ReservationsRepository()
        {
        }

        public ReservationsRepository(IObjectContext context, IRepository<domainReservation.ReservationsAdditionalData> additionalDataRepo, IOBRateRepository OBrateRepo)
            : base(context)
        {
            reservationAddicionalDataRepo = additionalDataRepo;
            dbContext = _context.Context;
            rateRepo = OBrateRepo;
        }

        public void SetSequenceReservationNumberRange(int interval)
        {
            Contract.Assert(interval > 0);
            __reservationNumberSeqRange = interval;
        }

        /// <summary>
        /// Call to the GetReservation detail SP
        /// </summary>
        /// <param name="reservationUID"></param>
        /// <param name="languageUID"></param>
        /// <param name="languageIso"></param>
        /// <returns></returns>
        public ReservationDetailSearchQR1 GetReservationDetail(long reservationUID, long languageUID, string languageIso)
        {
            ReservationDetailSearchQR1 result = new ReservationDetailSearchQR1();

            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@reservationId", reservationUID);
            parameters[1] = new SqlParameter("@languageId", languageUID);
            parameters[2] = new SqlParameter("@languageIso", languageIso);

            var connection = _context.Context.Database.Connection;
            using (connection.CreateConnectionScope())
            {
                var multipleResultSet = connection.QueryMultiple("[dbo].[GetReservation_GetReservationDetail]", parameters);

                // main search
                result.MainSearch = multipleResultSet.Read<ReservationDetailQR1>().FirstOrDefault();

                // policies
                result.Policies = multipleResultSet.Read<OtherPolicyQR1>().ToList();

                // GetAssignedActivitiesByGuest
                result.GuestActivities = multipleResultSet.Read<GuestActivityQR1>().ToList();

                multipleResultSet.Read<GuestActivityQR1>().ToList();
                // state translations

                try
                {
                    result.GuestStateName = multipleResultSet.Read<string>().FirstOrDefault();
                }
                catch
                {
                }

                multipleResultSet.Read<string>().FirstOrDefault();
                try
                {
                    result.BillingStateName = multipleResultSet.Read<string>().FirstOrDefault();
                }
                catch
                {
                }
            }
            return result;
        }

        public ReservationDataContext GetReservationContext(string existingReservationNumber, long channelUID, long tpiUID, long companyUID, long propertyUID,
            long reservationUID, long currencyUID, long paymentMethodTypeUID, IEnumerable<long> rateUIDs, string guestFirstName,
            string guestLastName, string guestEmail, string guestUsername, long? languageId, Guid? requestGuid = null)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@channelUID", channelUID, DbType.Int64);
            parameters.Add("@tpiUID", tpiUID, DbType.Int64);
            parameters.Add("@companyUID", companyUID, DbType.Int64);
            parameters.Add("@propertyUID", propertyUID, DbType.Int64);
            parameters.Add("@reservationUID", reservationUID, DbType.Int64);
            parameters.Add("@guestFirstName", guestFirstName, DbType.String);
            parameters.Add("@guestLastName", guestLastName, DbType.String);
            parameters.Add("@guestEmail", guestEmail, DbType.String);
            parameters.Add("@guestUserName", guestUsername, DbType.String);
            parameters.Add("@currencyUID", currencyUID, DbType.Int64);
            parameters.Add("@reservationNumber", existingReservationNumber, DbType.String);
            parameters.Add("@reservationLanguageId", languageId, DbType.Int64);
            parameters.Add("@paymentMethodTypeUID", paymentMethodTypeUID, DbType.Int64);

            string rates = string.Empty;
            if (rateUIDs != null && rateUIDs.Any())
            {
                rates = string.Join(",", rateUIDs.Distinct());
                parameters.Add("@rate_UIDs", rates, DbType.String, size: rates.Length);
            }
            else parameters.Add("@rate_UIDs", rates, DbType.String);

            var result = this.Context.Context.Database.Connection.Query<ReservationDataContext>("GetReservationInitialParameters",
                parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();

            // Get rates availability type
            if (rateUIDs != null && rateUIDs.Any())
            {
                var availabilityRequest = new OB.BL.Contracts.Requests.ListRateAvailabilityTypeRequest { RatesUIDs = rateUIDs.Distinct().ToList(), RequestGuid = requestGuid ?? Guid.Empty };
                result.RatesAvailabilityType = rateRepo.ListRatesAvailablityType(availabilityRequest);
            }

            if (result.ReservationUID == 0)
                result.ReservationUID = reservationUID;

            return result;
        }

        private static IDictionary<long, List<uint>> GetSequenceCache()
        {
            return __reservationsSeqCache;
        }

        public domainReservation.Reservation FindByUIDEagerly(long uid)
        {
            return this._objectSet.Where(x => x.UID == uid)
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedPromotionalCodes)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomChilds))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasAvailableDates)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)))
                .Include(x => x.ReservationPaymentDetails)
                .Include(x => x.ReservationPartialPaymentDetails).FirstOrDefault();
        }

        public domainReservation.Reservation FindByUIDEagerly(long uid, long propertyUID)
        {
            return this._objectSet.Where(x => x.UID == uid && x.Property_UID == propertyUID)
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedPromotionalCodes)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomChilds))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasAvailableDates)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)))
                .Include(x => x.ReservationPaymentDetails)
                .Include(x => x.ReservationPartialPaymentDetails).FirstOrDefault();
        }

        public domainReservation.Reservation FindByNumberEagerly(string number)
        {
            return this._objectSet.Where(x => x.Number == number)
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedPromotionalCodes)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomChilds))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasAvailableDates)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)))
                .Include(x => x.ReservationPaymentDetails)
                .Include(x => x.ReservationPartialPaymentDetails).SingleOrDefault();
        }

        public domainReservation.Reservation FindByNumberEagerly(string number, long propertyUID)
        {
            return this._objectSet.Where(x => x.Number == number && x.Property_UID == propertyUID)
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedPromotionalCodes)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomChilds))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasAvailableDates)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)))
                .Include(x => x.ReservationPaymentDetails)
                .Include(x => x.ReservationPartialPaymentDetails).SingleOrDefault();
        }

        public domainReservation.Reservation FindByReservationNumberAndChannelUID(string reservationNumber, long channelUID)
        {
            return this._objectSet.Where(x => x.Number == reservationNumber && x.Channel_UID.HasValue && x.Channel_UID == channelUID)
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedPromotionalCodes)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomChilds))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasAvailableDates)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)))
                .Include(x => x.ReservationPaymentDetails)
                .Include(x => x.ReservationPartialPaymentDetails).SingleOrDefault();
        }

        public domainReservation.Reservation FindByReservationNumberAndChannelUIDAndPropertyUID(string reservationNumber, long channelUID, long propertyIUD)
        {
            return this._objectSet.Where(x => x.Number == reservationNumber && x.Channel_UID.HasValue && x.Channel_UID == channelUID && x.Property_UID == propertyIUD)
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedPromotionalCodes)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomChilds))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasAvailableDates)))
                .Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)))
                .Include(x => x.ReservationPaymentDetails)
                .Include(x => x.ReservationPartialPaymentDetails).SingleOrDefault();
        }

        public IEnumerable<domainReservation.Reservation> FindByReservationUIDs(IEnumerable<long> ids,
           bool includeReservationRooms = false,
           bool includeReservationRoomChilds = false,
           bool includeReservationRoomDetails = false,
           bool includeReservationRoomDetailsIncentives = false,
           bool includeReservationRoomExtras = false,
           bool includeReservationPaymentDetails = false,
           bool includeReservationPartialPaymentDetails = false,
           bool includeReservationRoomTaxPolicies = false,
           int pageIndex = -1, int pageSize = -1)
        {
            var query = this._objectSet.AsQueryable();

            if (ids != null && ids.Any())
                query = query.Where(x => ids.Contains(x.UID));

            if (pageIndex > 0 && pageSize > 0)
                query = query.OrderBy(x => x.UID).Skip(pageIndex * pageSize);

            if (pageSize > 0)
                query = query.Take(pageSize);

            //Don't call ToList because it will make a Database query at this point. Since this method is called by others in this class,
            //the query shouldn't execute at this point...
            return query;
        }

        public IEnumerable<domainReservation.Reservation> FindByCriteria(ListReservationCriteria request)
        {

            var query = FindByReservationUIDs(request.ReservationUIDs,
                                                request.IncludeReservationRooms,
                                                request.IncludeReservationRoomChilds,
                                                request.IncludeReservationRoomDetails,
                                                request.IncludeReservationRoomDetailsAppliedIncentives,
                                                request.IncludeReservationRoomExtras,
                                                request.IncludeReservationPaymentDetail,
                                                request.IncludeReservationPartialPaymentDetails,
                                                request.IncludeReservationRoomTaxPolicies,
                                                -1, -1) as IQueryable<domainReservation.Reservation>;//paging is done afterwards        

            if (request.Orders != null && request.Orders.Any())
            {
                TreatRequestOrders(request.Orders);
                query = query.OrderBy(request.Orders);
            }
            else
                query = query.OrderByDescending(x => x.UID);


            if (request.IncludeReservationRooms || request.IncludeRates || request.IncludeRoomTypes || request.IncludeCancelationCosts)
                query = query.Include(x => x.ReservationRooms);

            if (request.IncludeReservationRoomDetails || request.IncludeCancelationCosts)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails));

            if (request.IncludeReservationRoomDetailsAppliedIncentives || request.IncludeIncentives || request.IncludeReservationRoomIncentivePeriods)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)));

            if (request.IncludeReservationRoomDetailsAppliedPromotionalCode)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedPromotionalCodes)));

            if (request.IncludeReservationRoomChilds)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomChilds));

            if (request.IncludeReservationRoomExtras || request.IncludeExtras)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras));

            if (request.IncludeReservationRoomExtrasSchedules)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)));

            if (request.IncludeReservationRoomExtrasAvailableDates)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasAvailableDates)));

            if (request.IncludeReservationRoomTaxPolicies || request.IncludeTaxPolicies)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies));

            if (request.IncludeReservationPaymentDetail)
                query = query.Include(x => x.ReservationPaymentDetails);

            if (request.IncludeReservationPartialPaymentDetails)
                query = query.Include(x => x.ReservationPartialPaymentDetails);


            return query.ToList();
        }

        public IEnumerable<domainReservation.Reservation> FindByCriteria(out int totalRecords, ListReservationCriteria request, int pageIndex = -1, int pageSize = -1, bool returnTotal = false)
        {
            totalRecords = -1;

            IQueryable<domainReservation.ReservationsAdditionalData> addicionalDataQuery = null;

            if ((request.PartnerIds != null && request.PartnerIds.Any())
                || (request.PartnerReservationNumbers != null && request.PartnerReservationNumbers.Any()))
            {
                addicionalDataQuery = reservationAddicionalDataRepo.GetQuery();

                if (request.PartnerIds != null && request.PartnerIds.Any())
                    addicionalDataQuery = addicionalDataQuery.Where(x => x.ChannelPartnerID.HasValue && request.PartnerIds.Contains(x.ChannelPartnerID.Value));

                if (request.PartnerReservationNumbers != null && request.PartnerReservationNumbers.Any())
                    addicionalDataQuery = addicionalDataQuery.Where(x => request.PartnerReservationNumbers.Contains(x.PartnerReservationNumber));

                if (request.ReservationUIDs == null)
                    request.ReservationUIDs = new List<long>();

                request.ReservationUIDs.AddRange(addicionalDataQuery.Select(x => x.Reservation_UID).ToList());
                request.ReservationUIDs = request.ReservationUIDs.Distinct().ToList();

                if (!request.ReservationUIDs.Any())
                    return new List<domainReservation.Reservation>();
            }

            var query = FindByReservationUIDs(request.ReservationUIDs,
                                                request.IncludeReservationRooms,
                                                request.IncludeReservationRoomChilds,
                                                request.IncludeReservationRoomDetails,
                                                request.IncludeReservationRoomDetailsAppliedIncentives,
                                                request.IncludeReservationRoomExtras,
                                                request.IncludeReservationPaymentDetail,
                                                request.IncludeReservationPartialPaymentDetails,
                                                request.IncludeReservationRoomTaxPolicies,
                                                -1, -1) as IQueryable<domainReservation.Reservation>;//paging is done afterwards

            if (request.PropertyUIDs != null && request.PropertyUIDs.Count > 0)
                query = query.Where(x => request.PropertyUIDs.Contains(x.Property_UID));

            if (request.ChannelUIDs != null && request.ChannelUIDs.Count > 0)
                query = query.Where(x => x.Channel_UID.HasValue && request.ChannelUIDs.Contains(x.Channel_UID.Value));

            if (request.ReservationNumbers != null && request.ReservationNumbers.Count > 0)
            {
                if (request.ReservationNumbers.Count == 1)
                {
                    var reservationNumber = request.ReservationNumbers.First();
                    query = query.Where(x => x.Number == reservationNumber);
                }
                else query = query.Where(x => request.ReservationNumbers.Contains(x.Number));
            }

            if (request.ReservationStatusCodes != null && request.ReservationStatusCodes.Any())
                query = query.Where(x => request.ReservationStatusCodes.Contains(x.Status));

            if (request.DateFrom.HasValue)
                query = query.Where(x => x.CreatedDate.Value >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
            {
                request.DateTo = request.DateTo.Value.AddDays(1);
                query = query.Where(x => x.CreatedDate.Value < request.DateTo.Value);
            }

            if (request.ModifiedFrom.HasValue)
                query = query.Where(x => x.ModifyDate.Value >= request.ModifiedFrom.Value);

            if (request.ModifiedTo.HasValue)
            {
                request.ModifiedTo = request.ModifiedTo.Value.AddDays(1);
                query = query.Where(x => x.ModifyDate.Value < request.ModifiedTo.Value);
            }

            if (request.TpiIds != null && request.TpiIds.Any())
                query = query.Where(x => x.TPI_UID.HasValue && request.TpiIds.Contains(x.TPI_UID.Value));

            //Apply filters
            if (request.NestedFilters != null)
                query = query.FilterBy(request.NestedFilters);
            else if (request.Filters != null && request.Filters.Any())
                query = query.FilterBy(request.Filters);

            if (request.Orders != null && request.Orders.Any())
                query = query.OrderBy(request.Orders);
            else
                query = query.OrderByDescending(x => x.UID);

            if (request.IncludeReservationRooms || request.IncludeRates || request.IncludeRoomTypes || request.IncludeCancelationCosts)
                query = query.Include(x => x.ReservationRooms);

            if (request.CheckIn.HasValue)
                query = query.Where(x => x.ReservationRooms.Any(y => y.DateFrom >= request.CheckIn.Value));

            if (request.CheckOut.HasValue)
                query = query.Where(x => x.ReservationRooms.Any(y => y.DateTo <= request.CheckOut.Value));

            if (request.IncludeReservationRoomDetails || request.IncludeCancelationCosts)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails));

            if (request.IncludeReservationRoomDetailsAppliedIncentives || request.IncludeIncentives)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)));

            if (request.IncludeReservationRoomChilds)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomChilds));

            if (request.IncludeReservationRoomExtras || request.IncludeExtras)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras));

            if (request.IncludeReservationRoomExtrasSchedules)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasSchedules)));

            if (request.IncludeReservationRoomExtrasAvailableDates)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras.Select(z => z.ReservationRoomExtrasAvailableDates)));

            if (request.IncludeReservationRoomTaxPolicies || request.IncludeTaxPolicies)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies));

            if (request.IncludeReservationPaymentDetail)
                query = query.Include(x => x.ReservationPaymentDetails);

            if (request.IncludeReservationPartialPaymentDetails)
                query = query.Include(x => x.ReservationPartialPaymentDetails);

            if (returnTotal)
                totalRecords = query.Count();

            if (pageIndex > 0 && pageSize > 0)
                query = query.Skip(pageIndex * pageSize);

            if (pageSize > 0)
                query = query.Take(pageSize);

            return query.ToList();
        }

        public IEnumerable<domainReservation.Reservation> FindByCheckOut(out int totalRecords, List<long> reservationUIDs, List<long> propertyUIDs, List<long> channelUIDs, List<string> reservationNumbers,
            List<long> reservationStatusCodes,
            DateTime? checkOutFrom, DateTime? checkOutTo,
            bool includeReservationRooms = false,
            bool includeReservationRoomChilds = false,
            bool includeReservationRoomDetails = false,
            bool includeReservationRoomDetailsIncentives = false,
            bool includeReservationRoomExtras = false,
            bool includeReservationPaymentDetails = false,
            bool includeReservationPartialPaymentDetails = false,
            bool includeReservationRoomTaxPolicies = false,
            int pageIndex = -1, int pageSize = -1,
            bool returnTotal = false)
        {
            totalRecords = -1;

            var query = FindByReservationUIDs(reservationUIDs, includeReservationRooms,
                                                    includeReservationRoomChilds,
                                                    includeReservationRoomDetails,
                                                    includeReservationRoomDetailsIncentives,
                                                    includeReservationRoomExtras,
                                                    includeReservationPaymentDetails,
                                                    includeReservationPartialPaymentDetails,
                                                    includeReservationRoomTaxPolicies,
                                                    -1, -1) as IQueryable<domainReservation.Reservation>;//paging is done afterwards

            if (propertyUIDs != null && propertyUIDs.Count > 0)
                query = query.Where(x => propertyUIDs.Contains(x.Property_UID));

            if (channelUIDs != null && channelUIDs.Count > 0)
                query = query.Where(x => x.Channel_UID.HasValue && channelUIDs.Contains(x.Channel_UID.Value));

            if (reservationNumbers != null && reservationNumbers.Count > 0)
            {
                if (reservationNumbers.Count == 1)
                {
                    var reservationNumber = reservationNumbers.First();
                    query = query.Where(x => x.Number == reservationNumber);
                }
                else query = query.Where(x => reservationNumbers.Contains(x.Number));
            }

            if (reservationStatusCodes != null && reservationStatusCodes.Any())
                query = query.Where(x => reservationStatusCodes.Contains(x.Status));

            if (returnTotal)
                totalRecords = query.Count();

            if (includeReservationRooms)
                query = query.Include(x => x.ReservationRooms);

            if (checkOutFrom.HasValue)
                query = query.Where(x => x.ReservationRooms.Any(y => y.DateTo >= checkOutFrom.Value));

            if (checkOutTo.HasValue)
                query = query.Where(x => x.ReservationRooms.Any(y => y.DateTo <= checkOutTo.Value));

            if (includeReservationRoomDetails)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails));

            if (includeReservationRoomDetailsIncentives)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomDetails.Select(z => z.ReservationRoomDetailsAppliedIncentives)));

            if (includeReservationRoomChilds)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomChilds));

            if (includeReservationRoomExtras)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomExtras));

            if (includeReservationRoomTaxPolicies)
                query = query.Include(x => x.ReservationRooms.Select(y => y.ReservationRoomTaxPolicies));

            if (includeReservationPaymentDetails)
                query = query.Include(x => x.ReservationPaymentDetails);

            if (includeReservationPartialPaymentDetails)
                query = query.Include(x => x.ReservationPartialPaymentDetails);

            if (pageIndex > 0 && pageSize > 0)
                query = query.OrderBy(x => x.UID).Skip(pageIndex * pageSize);

            if (pageSize > 0)
                query = query.Take(pageSize);

            return query.ToList();
        }

        public IEnumerable<long> FindReservationUIDSByRateRoomsAndDateOfModifOrStay(List<long> propertyUIDs, List<long> rateUIDs, DateTime? dateFrom, DateTime? dateTo, bool isDateFindModifications = false, bool isDateFindArrivals = false, bool isDateFindStays = false)
        {
            IEnumerable<long> result;

            var query = GetQuery();

            if (propertyUIDs != null && propertyUIDs.Count > 0)
                query = query.Where(x => propertyUIDs.Contains(x.Property_UID));

            if (rateUIDs != null && rateUIDs.Count > 0)
            {
                var query1 = query.SelectMany(x => x.ReservationRooms).Where(y => y.RoomType_UID != null && y.Rate_UID != null);

                var filteredReservationRates = query1.Where(x => rateUIDs.Contains((long)x.Rate_UID)).Select(x => x.UID);

                query = query.Where(x => x.ReservationRooms.Any(y => filteredReservationRates.Contains(y.UID)));
            }

            if ((dateFrom != null && dateTo != null) && (isDateFindModifications == true || isDateFindArrivals == true || isDateFindStays == true))
            {
                List<long> partialResult = new List<long>();

                if ((isDateFindModifications == true))
                {
                    var query2 = query;

                    if (dateFrom != null)
                    {
                        query2 = query2.Where(x => x.CreatedDate >= dateFrom.Value || x.ModifyDate >= dateFrom.Value);
                    }
                    if (dateTo != null)
                    {
                        query2 = query2.Where(x => x.CreatedDate <= dateTo.Value || x.ModifyDate <= dateTo.Value);
                    }

                    partialResult.AddRange(query2.Select(x => x.UID).ToList());
                }

                if (isDateFindArrivals == true)
                {
                    var query3 = query;

                    if (dateFrom != null && dateTo != null)
                        query3 = query3.Where(x => x.ReservationRooms.Any(y => (y.DateFrom >= dateFrom.Value && y.DateFrom <= dateTo.Value)));

                    partialResult.AddRange(query3.Select(x => x.UID));
                }

                if (isDateFindStays == true)
                {
                    var query4 = query;

                    if (dateFrom != null && dateTo != null)
                        query4 = query4.Where(x => x.ReservationRooms.Any(y => y.DateFrom <= y.DateTo && y.DateFrom <= dateTo.Value && y.DateTo >= dateFrom.Value));

                    partialResult.AddRange(query4.Select(x => x.UID));
                }

                partialResult.Sort();
                result = partialResult.Distinct();
            }
            else
            {
                result = query.Select(x => x.UID);
            }

            return result;
        }

        public IEnumerable<domainReservation.Reservation> FindReservationsLightByCriteria(out int totalRecords, List<long> UIDs, List<long> channelUIDs, List<long> propertyUIDs, DateTime? dateFrom, DateTime? dateTo, bool isDateFindModifications = false, bool isDateFindArrivals = false, bool isDateFindStays = false, bool includeReservationRooms = false,
            int pageIndex = -1, int pageSize = -1, bool returnTotal = false)
        {
            IEnumerable<domainReservation.Reservation> result;
            totalRecords = -1;

            var query = GetQuery();

            if (UIDs != null && UIDs.Count > 0)
                query = query.Where(x => UIDs.Contains(x.UID));

            if (channelUIDs != null && channelUIDs.Count > 0)
                query = query.Where(x => x.Channel_UID != null && channelUIDs.Contains((long)x.Channel_UID));

            if (propertyUIDs != null && propertyUIDs.Count > 0)
                query = query.Where(x => propertyUIDs.Contains(x.Property_UID));

            if (includeReservationRooms)
                query = query.Include(x => x.ReservationRooms);

            // Filter by Dates
            if ((dateFrom != null && dateTo != null) && (isDateFindModifications == true || isDateFindArrivals == true || isDateFindStays == true))
            {
                List<domainReservation.Reservation> partialResult = new List<domainReservation.Reservation>();

                if ((isDateFindModifications == true))
                {
                    var query2 = query;

                    if (dateFrom != null)
                    {
                        query2 = query2.Where(x => x.CreatedDate >= dateFrom.Value || x.ModifyDate >= dateFrom.Value);
                    }
                    if (dateTo != null)
                    {
                        query2 = query2.Where(x => x.CreatedDate <= dateTo.Value || x.ModifyDate <= dateTo.Value);
                    }

                    partialResult.AddRange(query2);
                }

                if ((isDateFindArrivals == true))
                {
                    var query3 = query;

                    if (dateFrom != null && dateTo != null)
                    {
                        query3 = query3.Where(x => x.ReservationRooms.Any(y => (y.DateFrom >= dateFrom.Value && y.DateFrom <= dateTo.Value)));
                    }

                    partialResult.AddRange(query3);
                }

                if ((isDateFindStays == true))
                {
                    var query4 = query;

                    if (dateFrom != null && dateTo != null)
                    {
                        query4 = query4.Where(x => x.ReservationRooms.Any(y => (y.DateFrom <= dateFrom.Value && y.DateTo >= dateTo.Value) || (y.DateFrom >= dateFrom.Value && y.DateTo <= dateTo.Value) || (y.DateFrom <= dateFrom.Value && (y.DateTo >= dateFrom.Value && y.DateTo <= dateTo.Value)) || (y.DateFrom >= dateFrom.Value && (y.DateFrom <= dateTo.Value && y.DateTo >= dateFrom.Value))));
                    }

                    partialResult.AddRange(query4);
                }

                result = partialResult.Distinct();
            }
            else
            {
                result = query;
            }

            if (returnTotal)
                totalRecords = result.Count();

            if (pageIndex > 0 && pageSize > 0)
                result = result.OrderBy(x => x.UID).Skip(pageIndex * pageSize);

            if (pageSize > 0)
                result = result.Take(pageSize);

            return result;
        }


        public IEnumerable<PropertyWithReservationsForChannelOrTPIQR1> FindPropertiesWithReservationsForChannelOrTPI(long channelUID, long? tpiUID, List<long> propertyUIDs)
        {
            string propQuery = propertyUIDs != null && propertyUIDs.Any() ?
                        String.Format("and Property_UID in ({0})", String.Join(", ", propertyUIDs))
                        : String.Empty;

            string query = QUERY_FindPropertiesWithReservationsForChannelOrTPI;

            return _context.Context.Database.Connection.Query<PropertyWithReservationsForChannelOrTPIQR1>(
                String.Format(query, propQuery),
                new
                {
                    channelUID = channelUID,
                    tpiUID = tpiUID
                });
        }

        #region Used when we dont want to pass the channel

        public IEnumerable<PropertyWithReservationsForChannelOrTPIQR1> FindPropertiesWithReservationsForChannelsTpis(List<long> propertyUIDs)
        {
            string strProps = null;
            strProps = propertyUIDs != null && propertyUIDs.Count > 0 ? string.Join(",", propertyUIDs) : null;

            var finalQuery = (strProps != null ?
                string.Format(QUERY_FindPropertiesNameUidProps, strProps) :
                string.Format(QUERY_FindPropertiesNameUid, strProps));

            return _context.Context.Database.Connection.Query<PropertyWithReservationsForChannelOrTPIQR1>(finalQuery);
        }

        #endregion

        public string GenerateReservationNumber(long propertyId)
        {
            string number = string.Empty;

            var cache = GetSequenceCache();
            lock (cache)
            {
                List<uint> availableNumbers = null;

                if (!cache.TryGetValue(propertyId, out availableNumbers) || availableNumbers.Count == 0)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@range_first_value_output", dbType: DbType.Object, direction: ParameterDirection.Output);
                    string script = string.Format(SEQUENCE_SCRIPT, propertyId, __reservationNumberSeqRange);
                    string scriptExistSequence = string.Format(GET_SEQUENCE_COUNT_SCRIPT, propertyId);

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var countSequences = this._context.Context.Database.Connection.ExecuteScalar<int>(scriptExistSequence);

                        if (countSequences > 0)
                            this._context.Context.Database.Connection.Execute(script, parameters);
                        else
                        {
                            //TODO: Log this...
                            //Create the Sequence if it doesn't exist...
                            int currentCount = this.Count(x => x.Property_UID == propertyId) + 1;
                            this._context.Context.Database.Connection.Execute(string.Format(CREATE_SEQUENCE_SCRIPT, propertyId, currentCount));
                            this._context.Context.Database.Connection.Execute(script, parameters);
                        }

                        scope.Complete();
                    }

                    if (availableNumbers == null)
                    {
                        availableNumbers = new List<uint>(__reservationNumberSeqRange);
                        cache.Add(propertyId, availableNumbers);
                    }

                    var currentSeqNumber = (uint)parameters.Get<long>("@range_first_value_output");
                    var maxnNumber = __reservationNumberSeqRange + currentSeqNumber;

                    for (uint i = currentSeqNumber; i < maxnNumber; i++)
                        availableNumbers.Add(i);
                }

                var currentValue = availableNumbers.First();
                availableNumbers.RemoveAt(0);

                number = currentValue.ToString();
            }

            return "RES" + number.PadLeft(6, '0') + "-" + propertyId;
        }

        public string GenerateReservationNumber(long propertyId, ref IDbTransaction scope)
        {
            string number = string.Empty;

            var cache = GetSequenceCache();
            lock (cache)
            {
                List<uint> availableNumbers = null;

                if (!cache.TryGetValue(propertyId, out availableNumbers) || availableNumbers.Count == 0)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@range_first_value_output", dbType: DbType.Object, direction: ParameterDirection.Output);

                    string script = string.Format(SEQUENCE_SCRIPT, propertyId, __reservationNumberSeqRange);

                    string scriptExistSequence = string.Format(GET_SEQUENCE_COUNT_SCRIPT, propertyId);
                    var countSequences = this._context.Context.Database.Connection.ExecuteScalar<int>(scriptExistSequence, transaction: scope);

                    if (countSequences > 0)
                        this._context.Context.Database.Connection.Execute(script, parameters, scope);
                    else
                    {
                        //TODO: Log this...
                        //Create the Sequence if it doesn't exist...
                        int currentCount = this.Count(x => x.Property_UID == propertyId) + 1;
                        this._context.Context.Database.Connection.Execute(string.Format(CREATE_SEQUENCE_SCRIPT, propertyId, currentCount), null, scope);
                        this._context.Context.Database.Connection.Execute(script, parameters, scope);
                    }

                    if (availableNumbers == null)
                    {
                        availableNumbers = new List<uint>(__reservationNumberSeqRange);
                        cache.Add(propertyId, availableNumbers);
                    }

                    var currentSeqNumber = (uint)parameters.Get<long>("@range_first_value_output");
                    var maxnNumber = __reservationNumberSeqRange + currentSeqNumber;

                    for (uint i = currentSeqNumber; i < maxnNumber; i++)
                        availableNumbers.Add(i);
                }

                var currentValue = availableNumbers.First();
                availableNumbers.RemoveAt(0);

                number = currentValue.ToString();
            }

            return "RES" + number.PadLeft(6, '0') + "-" + propertyId;
        }


        /// <summary>
        /// Get Current Reservation Transaction State
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="channelId"></param>
        /// <param name="reservationId"></param>
        /// <param name="hangfireId"></param>
        /// <returns></returns>
        public int GetReservationTransactionState(string transactionId, long channelId, out long reservationId, out long hangfireId)
        {
            reservationId = 0;
            hangfireId = 0;
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@channelUID", channelId, DbType.Int64);
            parameters.Add("@transactionUID", transactionId, DbType.String);
            parameters.Add("@reservationId", reservationId, DbType.Int64, ParameterDirection.Output);
            parameters.Add("@hangfireId", hangfireId, DbType.Int64, ParameterDirection.Output);

            var result = _context.Context.Database.Connection.ExecuteScalar<int>("GetReservationTransactionState", parameters, null, null, CommandType.StoredProcedure);
            reservationId = parameters.Get<long?>("@reservationId") ?? 0;
            hangfireId = parameters.Get<long?>("@hangfireId") ?? 0;
            return result;
        }

        /// <summary>
        /// Update reservation/reservationrooms to new status
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="reservationStatus"></param>
        /// <returns></returns>
        public void UpdateReservationStatus(long reservationId, int reservationStatus, string transactionId, int transactionState, string reservationStatusName, long? userId, bool updateReservationHistory, string paymentGatewayOrderId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@reservationStatus", reservationStatus, DbType.Int64);
            parameters.Add("@reservationId", reservationId, DbType.Int64);
            parameters.Add("@transactionUID", transactionId, DbType.String);
            parameters.Add("@transactionState", transactionState, DbType.Int32);
            parameters.Add("@reservationStatusName", reservationStatusName, DbType.String);
            parameters.Add("@modifyBy", userId, DbType.Int64);
            parameters.Add("@updateHistory", updateReservationHistory, DbType.Boolean);
            parameters.Add("@paymentGatewayOrderId", paymentGatewayOrderId, DbType.String);

            Context.Context.Database.Connection.ExecuteScalar<int>("UpdateReservationStatus", parameters, null, null, CommandType.StoredProcedure);
        }

        /// <summary>
        ///  Get Current Basic Reservation Info by TransactionID and PropertyUID
        /// </summary>
        /// <param name="propertyUID"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public ReservationBasicInfoForTransactionIdQR1 GetReservationBasicInfoForPaymentGatewayForTransaction(long propertyUID, string transactionId)
        {
            return _context.Context.Database.Connection.Query<ReservationBasicInfoForTransactionIdQR1>(
                QUERY_GetReservationBasicInfoForTransactionID,
                new
                {
                    PropertyUID = propertyUID,
                    PaymentGatewayTransactionID = transactionId
                }).FirstOrDefault();
        }

        /// <summary>
        ///  Get Current Basic ReservationTransactionStatus Info by ReservationUID
        /// </summary>
        /// <param name="reservationUID"></param>
        /// <returns></returns>
        public ReservationTransactionStatusBasicInfoForReservationUidQR1 GetReservationTransactionStatusBasicInfoForReservationUID(long reservationUID)
        {
            return _context.Context.Database.Connection.Query<ReservationTransactionStatusBasicInfoForReservationUidQR1>(
                QUERY_GetReservationTransactionStatusBasicInfoForReservationUID,
                new
                {
                    ReservationUID = reservationUID
                }).FirstOrDefault();
        }

        #region Aux

        public bool FindAnyRoomTypesBy_ReservationUID_And_SystemEventCode(long reservationUID, string systemEventCode)
        {
            //Better Convert to SP or precompiled query :
            return _context.Context.Database.Connection.Query(
                QUERY_FindRoomTypesBy_ReservationUID_And_SystemEventCode,
                new
                {
                    P0 = reservationUID,
                    p1 = systemEventCode
                }).Any();
        }

        public IEnumerable<PEOccupancyAlertQR1> FindByRoomTypeUID_And_Code_And_PropertyUID_And_RoomTypeDate(long roomTypeUID,
          long propertyUID, List<DateTime> roomTypeDates, string code)
        {
            StringBuilder datesStr = new StringBuilder();
            foreach (var roomTypeDate in roomTypeDates)
            {
                if (datesStr.Length > 0)
                    datesStr.Append(",");
                datesStr.Append("'" + roomTypeDate.Date.ToString("yyyy-MM-dd") + "'");
            }
            //Better Convert to SP or precompiled query :
            return _context.Context.Database.Connection.Query<PEOccupancyAlertQR1>(
                string.Format(FIND_OCCUPANCY_ALERTS_BY_ROOMTYPEUID_CODE_PROPERTYUID_MULTIPLEDATES, datesStr.ToString()), new
                {
                    RoomType_UID = roomTypeUID,
                    StrCode = code,
                    Property_UID = propertyUID,
                });
        }

        /// <summary>
        /// Fetch IsBookingEngine ChannelUID
        /// </summary>
        /// <returns></returns>
        public long GetBookingEngineChannelUID()
        {
            return _context.Context.Database.Connection.Query<long>(QUERY_BOOKINGENGINE_CHANNELUID).FirstOrDefault();
        }

        public OB.BL.Contracts.Responses.ListReservationsExternalSourceResponse ListReservationsExternalSource(OB.BL.Contracts.Requests.ListReservationsExternalSourceRequest request)
        {
            var data = new OB.BL.Contracts.Responses.ListReservationsExternalSourceResponse();
            var response = RESTServicesFacade.Call<OB.BL.Contracts.Responses.ListReservationsExternalSourceResponse>(request, "ExternalSources", "ListReservationsExternalSource");

            if (response.Status == OB.BL.Contracts.Responses.Status.Success)
                data = response;

            return data;
        }

        public List<domainReservation.ReservationsAdditionalData> FindReservationsAdditionalDataByReservationsUIDs(List<long> reservationIds)
        {
            var query = string.Format("SELECT * FROM ReservationsAdditionalData WITH(NOLOCK) WHERE Reservation_UID IN ({0})", string.Join(",", reservationIds));

            var result = _connection.Query<domainReservation.ReservationsAdditionalData>(query, null);

            return result.ToList();
        }

        public List<Item> FindReservationTransactionStatusByReservationsUIDs(List<long> reservationIds)
        {
            var query = string.Format(@"SELECT ReservationUID, TransactionUID FROM ReservationTransactionStatus WITH(NOLOCK) WHERE ReservationUID IN ({0}) 
                                                GROUP BY ReservationUID, TransactionUID", string.Join(",", reservationIds));

            var result = _connection.Query<Item>(query, null);
            return result.ToList();
        }


        /// <summary>
        /// Deletes all Guest Activities for a given Guest.
        /// </summary>
        /// <param name="guestUID"></param>
        /// <returns></returns>
        public int DeleteAllActivitiesForGuestUID(long guestUID)
        {
            var parameter = new SqlParameter("guestUID", guestUID);
            return this._context.Context.Database.ExecuteSqlCommand("delete from [" + DB_Name + "].[dbo].GuestActivities where GuestActivities.Guest_UID = @guestUID", parameter);
        }

        public int InsertReservationTransaction(string transactionId, long reservationId, string reservationNumber, long reservationStatus, Constants.ReservationTransactionStatus transactionStatus, long channelId, long hangfireId, int retries)
        {
            string query = @"INSERT INTO [dbo].[ReservationTransactionStatus]
           ([TransactionUID]
           ,[ReservationUID]
           ,[ReservationNumber]
           ,[TransactionState]
           ,[ChannelUID]
           ,[HangfireId]
           ,[Retries])
     VALUES
           (@TransactionUID
           ,@ReservationUID
           ,@ReservationNumber
           ,@TransactionState
           ,@ChannelUID
           ,@HangfireId
           ,@Retries)";


            SqlParameter[] parameters = new SqlParameter[7];
            parameters[0] = new SqlParameter("TransactionUID", transactionId);
            parameters[1] = new SqlParameter("ReservationUID", reservationId);
            parameters[2] = new SqlParameter("ReservationNumber", reservationNumber);
            parameters[3] = new SqlParameter("TransactionState", (int)transactionStatus);
            parameters[4] = new SqlParameter("ChannelUID", channelId);
            parameters[5] = new SqlParameter("HangfireId", hangfireId);
            parameters[6] = new SqlParameter("Retries", retries);

            return dbContext.Database.ExecuteSqlCommand(query, parameters);
        }

        public decimal GetExchangeRateBetweenCurrenciesByPropertyId(long baseCurrencyId, long currencyId, long propertyId)
        {
            return dbContext.Database.SqlQuery<decimal>(string.Format("exec [{0}].[dbo].[GetExchangeRateBetweenCurrencies] {1}, {2}, {3}", DB_Name, baseCurrencyId, currencyId, propertyId)).First();
        }

        public int InsertReservationAdditionalDataJson(long reservationId, string reservationAdditionalDataJson)
        {
            string query = @"INSERT INTO [dbo].[ReservationsAdditionalData]
           ([Reservation_UID]
           ,[ReservationAdditionalDataJSON]
           ,[IsFromNewInsert]
           ,[ChannelPartnerID]
           ,[PartnerReservationNumber]
           ,[ReservationDomain]
           ,[BookingEngineTemplate]
           ,[isDirectReservation]
           ,[BookerIsGenius]
           ,[BigPullAuthRequestor_UID]
           ,[BigPullAuthOwner_UID])
            VALUES
           (@ReservationId, @ReservationAdditionalDataJson, 0, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL)";

            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("ReservationId", reservationId);
            parameters[1] = new SqlParameter("ReservationAdditionalDataJson", reservationAdditionalDataJson);


            return dbContext.Database.ExecuteSqlCommand(query, parameters);
        }

        public int UpdateReservationAdditionalDataJson(long id, string reservationAdditionalDataJson)
        {
            string query = @"UPDATE [dbo].[ReservationsAdditionalData]
                           SET [ReservationAdditionalDataJSON] = @ReservationAdditionalDataJson
                           WHERE [UID] = @Id";

            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("ReservationAdditionalDataJson", reservationAdditionalDataJson);
            parameters[1] = new SqlParameter("Id", id);

            return dbContext.Database.ExecuteSqlCommand(query, parameters);
        }

        public int UpdateRateRoomDetailAllotments(Dictionary<long, int> rateRoomDetailsVsAllotmentToAdd, bool validateAllotment, string correlationId = null)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            string query = validateAllotment ? UPDATE_ALLOTMENT_BATCH_QUERY_WITH_VALIDATION : UPDATE_ALLOTMENT_BATCH_QUERY;

            StringBuilder batchStatement = new StringBuilder();
            foreach (var rrdAllot in rateRoomDetailsVsAllotmentToAdd)
            {
                batchStatement.AppendFormat(query, rrdAllot.Key, rrdAllot.Value, correlationId, DB_Name);
                batchStatement.AppendLine();
            }

            return this._context.Context.Database.Connection.Execute(batchStatement.ToString());
        }
        #endregion

        private void TreatRequestOrders(List<Filter.SortByInfo> orders)
        {
            foreach (var ftr in orders)
            {
                switch (ftr.OrderBy)
                {
                    case "DateFrom":
                        ftr.OrderBy = "ReservationRooms.DateFrom";
                        break;

                    case "DateTo":
                        ftr.OrderBy = "ReservationRooms.DateTo";
                        break;
                }
            }
        }

        public List<ReservationBEOverviewQR1> ListMyAccountReservationsOverview(long UserUID, int UserType, DateTime? DateFrom, DateTime? DateTo)
        {
            // TODO : Review this repository
            if (DateFrom == null)
                DateFrom = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;

            if (DateTo == null)
                DateTo = DateTime.MaxValue;

            var parameters = new Dapper.DynamicParameters();
            parameters.Add("UserUID", UserUID, System.Data.DbType.Int64);
            parameters.Add("DateFrom", DateFrom, System.Data.DbType.DateTime);
            parameters.Add("DateTo", DateTo, System.Data.DbType.DateTime);

            string userFilter = string.Empty;
            if (UserType == (int)Constants.BookingEngineUserType.TravelAgent || UserType == (int)Constants.BookingEngineUserType.Corporate)
                userFilter = " AND r.TPI_UID = @UserUID";
            else if (UserType == (int)Constants.BookingEngineUserType.Guest)
                userFilter = " AND r.Guest_UID = @UserUID";
            else if (UserType == (int)Constants.BookingEngineUserType.Employee)
                userFilter = " AND r.Employee_UID = @UserUID";

            string queryCommand = @"SELECT DISTINCT       
                                    Reservation_UID = r.UID,
                                    ReservationRoom_UID = rr.UID, 
                                    rr.ReservationRoomNo, 
                                    rr.DateFrom,
                                    Nights = DATEDIFF(day, rr.DateFrom, rr.DateTo),
                                    ReservationTotal = rr.ReservationRoomsTotalAmount / r.PropertyBaseCurrencyExchangeRate,
                                    r.ReservationBaseCurrency_UID,
                                    r.ReservationCurrency_UID,
                                    r.ReservationCurrencyExchangeRate,
                                    r.PropertyBaseCurrencyExchangeRate,
                                    r.Property_UID,
                                    ReservationRoomStatus = rr.Status,
                                    ReservationStatus = r.Status,
                                    rr.CommissionValue,
                                    ReservationCommission = (r.TotalAmount * (rr.CommissionValue / 100)) / r.PropertyBaseCurrencyExchangeRate
                                    FROM Reservations AS r
                                    INNER JOIN ReservationRooms AS rr ON r.UID = rr.Reservation_UID
                                    WHERE rr.DateFrom >= @DateFrom AND rr.DateFrom <= @DateTo " + userFilter;

            return this.Context.Context.Database.Connection.Query<ReservationBEOverviewQR1>(queryCommand, parameters).ToList();
        }

        public int ValidateReservation(ValidateReservationCriteria criteria)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@checkIn", criteria.DateFrom, DbType.DateTime);
            parameters.Add("@checkOut", criteria.DateTo, DbType.DateTime);
            parameters.Add("@channel_UID", criteria.Channel_UID, DbType.Int64);
            parameters.Add("@property_UID", criteria.Property_UID, DbType.Int64);
            parameters.Add("@rate_UID", criteria.Rate_UID, DbType.Int64);
            parameters.Add("@roomType_UID", criteria.RoomType_UID, DbType.Int64);
            parameters.Add("@numAdults", criteria.NumAdults, DbType.Int32);
            parameters.Add("@numChilds", criteria.NumChilds, DbType.Int32);
            parameters.Add("@ChildAges", criteria.ChildAges, DbType.String);
            parameters.Add("@PCC", criteria.PCC, DbType.String);
            parameters.Add("@CompanyCode", criteria.CompanyCode, DbType.String);
            parameters.Add("@Currency", criteria.Currency, DbType.String);
            parameters.Add("@boardType_UID", criteria.BoardType_UID, DbType.Int64);
            parameters.Add("@numberOfRooms", criteria.NumberOfRooms, DbType.Int32);
            parameters.Add("@higherDayPrice", criteria.HigherDayPrice, DbType.Decimal);
            parameters.Add("@paymentMethodType", criteria.PaymentMethodType, DbType.Int64);
            parameters.Add("@groupCode", criteria.GroupCode, DbType.Int64);
            parameters.Add("@promoCode", criteria.PromoCode, DbType.Int64);
            parameters.Add("@validateAllotment", criteria.ValidateAllotment, DbType.Boolean);
            parameters.Add("@tpi_UID", criteria.Tpi_UID, DbType.Int64);

            var error = this.Context.Context.Database.Connection.Query<int>("ValidateReservationV2", parameters, commandType: CommandType.StoredProcedure).First();

            return error;
        }

        public int UpdateReservationTransactionStatus(string transactionId, long channelId, OB.Reservation.BL.Constants.ReservationTransactionStatus transactionStatus)
        {
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("transactionUID", transactionId);
            parameters[1] = new SqlParameter("channelId", channelId);
            parameters[2] = new SqlParameter("transactionState", (int)transactionStatus);

            return dbContext.Database.ExecuteSqlCommand("UpdateReservationTransactionStatus @transactionUID, @channelId, @transactionState", parameters);
        }

        public int UpdateReservationVcn(UpdateReservationVcnCriteria criteria)
        {
            return dbContext.Database.ExecuteSqlCommand(UPDATE_VCN_QUERY,
                    new SqlParameter("@reservationId", criteria.ReservationId),
                    new SqlParameter("@vcnReservationId", criteria.VcnReservationId),
                    new SqlParameter("@vcnToken", criteria.VcnToken),
                    new SqlParameter("@cvv", criteria.CreditCardCVV),
                    new SqlParameter("@cardHolderName", criteria.CreditCardHolderName),
                    new SqlParameter("@cardNumber", criteria.CreditCardNumber),
                    new SqlParameter("@cardExpireDate", criteria.CreditCardExpirationDate),
                    new SqlParameter("@cardHashCode", criteria.CreditCardHashCode));
        }

        public long GetPropertyIdByReservationId(long reservationId)
        {
            var query = GetQuery().Where(x => x.UID == reservationId);
            return query.Select(x => x.Property_UID).FirstOrDefault();
        }

        public List<domainReservation.Reservation> FindReservationByNumber(string reservationNumber)
        {
            var query = _objectSet.AsQueryable();

            query = query.Where(x => string.Equals(x.Number, reservationNumber));

            return query.ToList();
        }
    }
}
