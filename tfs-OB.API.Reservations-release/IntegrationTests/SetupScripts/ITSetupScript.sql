use Omnibees
GO

-- Set IDs
DECLARE @dateUtcNow datetime = GETUTCDATE();
DECLARE @currencyId bigint = 34;
DECLARE @clientId bigint = 10169;
DECLARE @propertyId bigint = 30433;
DECLARE @roomTypeId1 bigint = 32486;
DECLARE @roomTypeId2 bigint = 32487;
DECLARE @otherPolicyId bigint = 33102;
DECLARE @depositPolicyId bigint = 40312;
DECLARE @cancelationPolicyId bigint = 38732;
DECLARE @taxPolicyId bigint = 30311;
DECLARE @channelPropertyId1 bigint = 182487;
DECLARE @channelPropertyId2 bigint = 182488;
DECLARE @childTermId bigint = 25313;
DECLARE @channelId_BE bigint = 1;
DECLARE @channelId_Expedia bigint = 2;
DECLARE @channelId_HoteisNet bigint = 30;
DECLARE @channelId_Atcbt bigint = 80;
DECLARE @channelId_Airbnb bigint = 463;
DECLARE @tpiId1 bigint = 8320;
DECLARE @tpiId2 bigint = 8321;
DECLARE @extraId bigint = 36142;
DECLARE @incentiveId bigint = 42974;
DECLARE @externalSourceId bigint = 9;
DECLARE @propertyExternalSourceId bigint = 931;
DECLARE @rateId bigint = 40537;
DECLARE @rateFolderId bigint = 1137;
DECLARE @rateRoomId bigint = 72996;
DECLARE @rateChannelId1 bigint = 445668;
DECLARE @rateChannelId2 bigint = 445667;
DECLARE @rateExtraId bigint = 51330;
DECLARE @rateDepositPolicyId bigint = 10708;
DECLARE @rateCancelationPolicyId bigint = 794;

SET QUOTED_IDENTIFIER ON

SET XACT_ABORT ON

BEGIN TRAN

-- Client
BEGIN
	update dbo.Client
	set 
		Name='Cliente dos testes de integração',
		CompanyName='testes de integração',
		Address1='rua dos testes',
		City='Abegoaria, Pedro Miguel',
		PostalCode=1234,
		Country_UID=157,
		Email='testes@testes.pt',
		IsActive=1,
		NumberOfHotel=20,
		VATNumber=123456789,
		State='Açores',
		State_UID=5617300,
		City_UID=47014,
		ContactEmail='testes@testes.pt'
	where UID=@clientId
END

-- Property
BEGIN
	update dbo.Properties
	set 
		Name='Propriedade 1 - testes de integração',
		City='Braga',
		State='Braga',
		InvoicingEmail='testes@testes.pt',
		AllowInfants=0,
		Client_UID=@clientId,
		BaseCurrency_UID=@currencyId,
		PropertyCategory_UID=1,
		IsActive=1,
		Country_UID=157,
		AccountManager_UID=12701,
		TimeZone_UID=33,
		PriceModel=0,
		TotalRooms=10,
		InvoicingName='Cliente dos testes de integração',
		InvoicingCompanyName='testes de integração',
		InvoicingAddress1='rua dos testes',
		InvoicingCity='Abegoaria, Pedro Miguel',
		InvoicingPostalCode=1234,
		InvoicingCountry_UID=157,
		InvoicingVATNumber='123456789',
		Chain_UID=0,
		BaseLanguage_UId=4,
		State_UID=1341879,
		City_UID=5604480,
		IsDemo=0,
		InvoicingState_UID=5617300,
		InvoicingCity_UID=47014
	where UID=@propertyId

	delete from dbo.PropertyLanguages where Property_UID=@propertyId
	insert INTO dbo.PropertyLanguages (Property_UID,Language_UID,Description,CreatedDate) values (@propertyId,8,'Propriedade 1 - testes de integração',@dateUtcNow)
END

-- ROOM TYPES
BEGIN
	DELETE rtp
	FROM dbo.RoomTypePeriods rtp
	JOIN dbo.RoomTypes rt ON rtp.RoomType_UID = rt.UID
	WHERE (rt.Property_UID=@propertyId and rt.UID <> @roomTypeId1 and rt.UID <> @roomTypeId2)

	DELETE rcu
	FROM dbo.RateChannelUpdates rcu
	INNER JOIN dbo.RoomTypes rt ON rcu.RoomType_UID = rt.UID
	WHERE (rt.Property_UID=@propertyId and rt.UID <> @roomTypeId1 and rt.UID <> @roomTypeId2)

	delete from dbo.RoomTypes where (Property_UID=@propertyId and UID <> @roomTypeId1 and UID <> @roomTypeId2)

	--Room Type 1
	update dbo.RoomTypes
	set 
		Name='Room 1 para IT',
		Qty=1,
		Description='Room 1 para ser usado como defeito pelos IT',
		Property_UID=@propertyId,
		AdultMinOccupancy=1,
		AdultMaxOccupancy=2,
		IsDeleted=0
		where UID=@roomTypeId1
		--Room Type 2
		update dbo.RoomTypes
		set Name='Room 2 para IT',
		Qty=1,
		Description='Room 2 para ser usado como defeito pelos IT',
		Property_UID=@propertyId,
		AdultMinOccupancy=1,
		AdultMaxOccupancy=2,
		IsDeleted=0
		where UID=@roomTypeId2

	delete from dbo.RoomTypeLanguages where (RoomType_UID=@roomTypeId1 or RoomType_UID=@roomTypeId2)
	insert into dbo.RoomTypeLanguages (Name,Description,RoomType_UID,Language_UID,CreatedDate) 
	values 
	 ('Room 1 para IT','Room 1 para ser usado como defeito pelos IT',@roomTypeId1,4,@dateUtcNow)
	,('Room 1 para IT','Room 1 para ser usado como defeito pelos IT',@roomTypeId1,8,@dateUtcNow)
	,('Room 2 para IT','Room 2 para ser usado como defeito pelos IT',@roomTypeId2,4,@dateUtcNow)
	,('Room 2 para IT','Room 2 para ser usado como defeito pelos IT',@roomTypeId2,8,@dateUtcNow)
END

--PropertyCurrencies
BEGIN
	delete from dbo.PropertyCurrencies where Property_UID=@propertyId
	insert into dbo.PropertyCurrencies (Property_UID,Currency_UID,IsAutomaticExchangeRate,IsActive)
	values (@propertyId,1,1,1),
	(@propertyId,2,1,1),
	(@propertyId,3,1,1),
	(@propertyId,4,1,1)
END

-- POLICIES (Tax, Deposit, Cancelation and Other)
BEGIN 
	DELETE opl 
	FROM dbo.OtherPoliciesLanguages opl
	INNER JOIN dbo.OtherPolicies op ON opl.OtherPolicy_UID = op.UID
	WHERE (op.Property_UID=@propertyId and op.UID <> @otherPolicyId)

	delete from dbo.OtherPolicies where (Property_UID=@propertyId and UID <> @otherPolicyId)

	DELETE dpl
	FROM dbo.DepositPoliciesLanguages dpl
	INNER JOIN dbo.DepositPolicies dp ON dpl.DepositPolicy_UID = dp.UID
	WHERE (dp.Property_UID=@propertyId and dp.UID <> @depositPolicyId)

	DELETE dpgt
	FROM dbo.DepositPoliciesGuaranteeType dpgt
	INNER JOIN dbo.DepositPolicies dp ON dpgt.DepositPolicy_UID = dp.UID
	WHERE (dp.Property_UID=@propertyId and dp.UID <> @depositPolicyId)

	UPDATE dbo.Rates
	SET DepositPolicy_UID = NULL
	WHERE Property_UID = @propertyId AND DepositPolicy_UID <> @depositPolicyId

	delete from dbo.DepositPolicies where (Property_UID=@propertyId and UID <> @depositPolicyId)

	DELETE cpl 
	FROM dbo.CancellationPoliciesLanguages cpl
	INNER JOIN dbo.CancellationPolicies cp ON cpl.CancellationPolicies_UID = cp.UID
	WHERE (cp.Property_UID=@propertyId and cp.UID <> @cancelationPolicyId)

	DELETE cpc
	FROM dbo.CancellationPoliciesCurrencies cpc
	INNER JOIN dbo.CancellationPolicies cp ON cpc.CancellationPolicy_UID = cp.UID
	WHERE (cp.Property_UID=@propertyId and cp.UID <> @cancelationPolicyId)

	delete from dbo.CancellationPolicies where (Property_UID=@propertyId and UID <> @cancelationPolicyId)

	DELETE tpl 
	FROM dbo.TaxPoliciesLanguages tpl
	INNER JOIN dbo.TaxPolicies tp ON tpl.TaxPolicies_UID = tp.UID
	WHERE (tp.Property_UID=@propertyId and tp.UID <> @taxPolicyId)

	DELETE tpc
	FROM dbo.TaxPoliciesCurrencies tpc
	INNER JOIN dbo.TaxPolicies tp ON tpc.TaxPolicy_UID = tp.UID
	WHERE (tp.Property_UID=@propertyId and tp.UID <> @taxPolicyId)

	delete from dbo.TaxPolicies where (Property_UID=@propertyId and UID <> @taxPolicyId)

	update dbo.OtherPolicies
	set 
		OtherPolicy_Name='Politica geral para os IT',
		OtherPolicy_Description='Politica geral para ser usada como defeito nos IT',
		Property_UID=@propertyId,
		IsDeleted=0,
		OtherPolicyCategory_UID=NULL
	where UID=@otherPolicyId;
	update dbo.DepositPolicies
	set 
		Name='Politica de deposito para os IT',
		Description='Politica de deposito para ser usada como defeito nos IT.',
		Percentage=null,
		Property_UID=@propertyId,
		IsDeleted=0,
		PaymentModel=1
	where UID=@depositPolicyId
	update dbo.CancellationPolicies
	set 
		CancellationPolicy_Name='Politica de cancelamento para os IT',
		CancellationPolicy_Description='Politica de cancelamento para ser usada como defeito nos IT',
		Property_UID=@propertyId,
		IsDeleted=0,
		PaymentModel=1
	where UID=@cancelationPolicyId
	update TaxPolicies
	set 
		Name='Politica de taxa para os IT',
		Description='Politica de taxa para ser usada como defeito pelos IT',
		Property_UID=@propertyId,
		IsPercentage=1,
		IsDeleted=0
	where UID=@taxPolicyId	
	
	delete from dbo.OtherPoliciesLanguages where OtherPolicy_UID = @otherPolicyId
	insert into dbo.OtherPoliciesLanguages (Name,Description,OtherPolicy_UID,Language_UID,CreatedDate) 
	values 
	 ('Política geral para os IT','Política geral para ser usada como defeito nos IT',@otherPolicyId,8,@dateUtcNow)
	,('Política geral para os IT','Política geral para ser usada como defeito nos IT',@otherPolicyId,4,@dateUtcNow)
	
	delete from dbo.DepositPoliciesLanguages where DepositPolicy_UID = @depositPolicyId
	insert into dbo.DepositPoliciesLanguages(Name,Description,DepositPolicy_UID,Language_UID,CreatedDate) 
	values 
	 ('Política de depósito para os IT','Política de depósito para ser usada como defeito nos IT.',@depositPolicyId,4,@dateUtcNow)
	,('Política de depósito para os IT','Política de depósito para ser usada como defeito nos IT.',@depositPolicyId,8,@dateUtcNow)
	
	delete from dbo.CancellationPoliciesLanguages where CancellationPolicies_UID = @cancelationPolicyId
	insert into dbo.CancellationPoliciesLanguages(Name,Description,CancellationPolicies_UID,Language_UID,CreatedDate) 
	values 
	 ('Política de cancelamento para os IT','Política de cancelamento para ser usada como defeito nos IT',@cancelationPolicyId,4,@dateUtcNow)
	,('Política de cancelamento para os IT','Política de cancelamento para ser usada como defeito nos IT',@cancelationPolicyId,8,@dateUtcNow)
	
	delete from dbo.TaxPoliciesLanguages where TaxPolicies_UID = @taxPolicyId
	insert into dbo.TaxPoliciesLanguages(Name,Description,TaxPolicies_UID,Language_UID,CreatedDate) 
	values 
	 ('Política de taxa para os IT','Política de taxa para ser usada como defeito pelos IT',@taxPolicyId,4,@dateUtcNow)
	,('Política de taxa para os IT','Política de taxa para ser usada como defeito pelos IT',@taxPolicyId,8,@dateUtcNow)

END

--Child Term
BEGIN
	delete ctl
	from dbo.ChildTermLanguages ctl 
	INNER JOIN dbo.ChildTerms ct ON ct.[UID] = ctl.ChildTerm_UID
	where ct.Property_UID = @propertyId
	
	delete from dbo.ChildTerms where (Property_UID=@propertyId and UID <> @childTermId)

	insert into dbo.ChildTermLanguages (Name,Description,ChildTerm_UID,Language_UID) 
	values 
	 ('Child Term para ser usado nos IT','Child Term para ser usado nos IT',@childTermId,4)
	,('Child Term para ser usado nos IT','Child Term para ser usado nos IT',@childTermId,8)

	update dbo.ChildTerms
	set 
		Name='Child Term para ser usado nos IT',
		AgeFrom=1,
		AgeTo=2,
		CountsAsAdult=0,
		Property_UID=@propertyId,
		IsDeleted=0,
		IsFree=1,
		Description='Child Term para ser usado nos IT'
	where UID=@childTermId
END

--HotéisNetOperators
BEGIN
	delete from dbo.ChannelsProperties where (Property_UID=@propertyId and UID <> @channelPropertyId1 and UID <> @channelPropertyId2)

	update dbo.ChannelsProperties
	set 
		Channel_UID=@channelId_HoteisNet,
		Property_UID=@propertyId,
		IsActive=1,
		Value=0,
		IsPercentage=1,
		IsDeleted=0,
		IsPendingRequest=0,
		IsOperatorsCreditLimit=0,
		IsActivePrePaymentCredit=0,
		IsOnRequestEnable=0
	where UID=@channelPropertyId1
	update dbo.ChannelsProperties
	set 
		Channel_UID=@channelId_Atcbt,
		Property_UID=@propertyId,
		IsActive=1,
		Value=0,
		IsPercentage=1,
		IsDeleted=0,
		IsPendingRequest=0,
		IsOperatorsCreditLimit=0,
		IsActivePrePaymentCredit=0,
		IsOnRequestEnable=0
	where UID=@channelPropertyId2
END

-- ThirdPartyIntermediaries (TPIs)
BEGIN
	DELETE tpigt
	FROM dbo.ThirdPartyIntermediariesGuaranteeType tpigt
	INNER JOIN dbo.ThirdPartyIntermediaries tpi ON tpigt.ThirdPartyIntermediaries_UID = tpi.UID
	WHERE (tpi.Property_UID=@propertyId and tpi.UID <> @tpiId1 and tpi.UID <> @tpiId2)

	DELETE stpic FROM dbo.SalesmanThirdPartyIntermediariesComissions stpic
	INNER JOIN dbo.ThirdPartyIntermediaries tpi ON stpic.ThirdPartyIntermediariesUID = tpi.UID
	WHERE (tpi.Property_UID=@propertyId and tpi.UID <> @tpiId1 and tpi.UID <> @tpiId2)

	DELETE t
	FROM dbo.TPIProperties t
	INNER JOIN dbo.ThirdPartyIntermediaries tpi ON t.TPI_UID = tpi.UID
	WHERE (tpi.Property_UID=@propertyId and tpi.UID <> @tpiId1 and tpi.UID <> @tpiId2)

	delete from dbo.ThirdPartyIntermediaries where (Property_UID=@propertyId and UID <> @tpiId1 and UID <> @tpiId2)

	--Travel Agents
	update dbo.ThirdPartyIntermediaries
	set 
		Name='Agência de Viagens para os IT',
		Language_UID=4,
		Currency_UID=@currencyId,
		Email='teste@teste.com',
		TPICategory_UID=null,
		IsDeleted=0,
		Property_UID=@propertyId,
		IsCompany=0,
		IsActive=1,
		TPIType=1,
		Client_UID=@clientId
	where UID=@tpiId1

	--Corporate
	update dbo.ThirdPartyIntermediaries
	set 
		Name='Empresa para os IT',
		Language_UID=4,
		Currency_UID=@currencyId,
		Email='teste@teste.pt',
		TPICategory_UID=null,
		IsDeleted=0,
		Property_UID=@propertyId,
		IsCompany=1,
		IsActive=1,
		TPIType=2,
		Client_UID=@clientId
	where UID=@tpiId2
END

--EXTRAS
BEGIN
	DELETE ebt 
	FROM dbo.ExtrasBillingTypes ebt
	INNER JOIN dbo.Extras e ON ebt.Extras_UID = e.UID
	WHERE (e.Property_UID=@propertyId and e.UID <> @extraId)

	DELETE ec 
	FROM dbo.ExtrasCurrencies ec
	INNER JOIN dbo.Extras e ON ec.Extra_UID = e.UID
	WHERE (e.Property_UID=@propertyId and e.UID <> @extraId)

	DELETE el
	FROM dbo.ExtrasLanguages el
	INNER JOIN dbo.Extras e ON el.Extra_UID = e.UID
	WHERE (e.Property_UID=@propertyId and e.UID <> @extraId) 

	delete from dbo.Extras where (Property_UID=@propertyId and UID <> @extraId)
	delete from dbo.ExtrasLanguages where Extra_UID=@extraId

	insert into dbo.ExtrasLanguages (Name,Description,Extra_UID,Language_UID,CreatedDate) 
	values 
	 ('Extra para os IT','Extra para ser usado como defeito nos IT',@extraId,4,@dateUtcNow)
	,('Extra para os IT','Extra para ser usado como defeito nos IT',@extraId,8,@dateUtcNow)

	update dbo.Extras
	set 
		Name='Extra para os IT',
		Description='Extra para ser usado como defeito nos IT',
		Value=10.00,
		Property_UID=@propertyId,
		IsDeleted=0,
		IsActive=1
	where UID=@extraId
END

--Incentives
BEGIN
	delete from dbo.IncentivePeriods where IncentiveUID in (select UID from dbo.Incentives where (Property_UID=@propertyId and UID <> @incentiveId))
	delete from dbo.Incentives where (Property_UID=@propertyId and UID <> @incentiveId);
	delete from dbo.IncentiveLanguages where Incentive_UID=@incentiveId

	insert into dbo.IncentiveLanguages (Name,Language_UID,Incentive_UID) 
	values 
	 ('Incentivo para os IT',8,@incentiveId)
	,('Incentivo para os IT',4,@incentiveId)

	update dbo.Incentives
	set 
		IncentiveType_UID=1,
		Property_UID=@propertyId,
		Name='Incentivo para os IT',
		IsDeleted=0
	where UID=@incentiveId
END

--ChannelsProperties
BEGIN
	delete from dbo.ChannelsProperties where Property_UID=@propertyId;
	insert into dbo.ChannelsProperties (Channel_UID,Property_UID,IsActive,Value,IsPercentage,IsDeleted,IsPendingRequest,IsOperatorsCreditLimit,IsActivePrePaymentCredit,IsOnRequestEnable) 
	Values 
	 (@channelId_BE,@propertyId,1,0,1,0,0,0,0,1)
	,(@channelId_Airbnb,@propertyId,1,0,1,0,0,0,0,0)
	,(@channelId_HoteisNet,@propertyId,1,0,1,0,0,0,0,0)
	,(@channelId_Atcbt,@propertyId,1,0,1,0,0,0,0,1)
	,(@channelId_Expedia,@propertyId,1,0,1,0,0,0,0,0)
END

--PropertiesExternalSources
BEGIN
	delete from dbo.PropertiesExternalSources where Property_UID=@propertyId and UID <> @propertyExternalSourceId

	update dbo.PropertiesExternalSources
	set 
		Property_UID=@propertyId,
		ExternalSource_UID=@externalSourceId,
		IsActive=1
	where UID=@propertyExternalSourceId
END

--Rates
BEGIN
	DELETE re 
	FROM dbo.RatesExtras re
	INNER JOIN dbo.Rates r ON re.Rate_UID = r.UID
	WHERE (r.Property_UID=@propertyId and r.UID <> @rateId)

	DELETE rtp
	FROM dbo.RatesTaxPolicies rtp
	INNER JOIN dbo.Rates r ON rtp.Rate_UID = r.UID
	WHERE (r.Property_UID=@propertyId and r.UID <> @rateId)

	DELETE rdp
	FROM dbo.RateDepositPolicies rdp
	INNER JOIN dbo.Rates r ON rdp.Rate_UID = r.UID
	WHERE (r.Property_UID=@propertyId and r.UID <> @rateId)

	DELETE rr
	FROM dbo.RateRooms rr
	INNER JOIN dbo.Rates r ON rr.Rate_UID = r.UID
	WHERE (r.Property_UID=@propertyId and r.UID <> @rateId)

	DELETE rcpm
	FROM dbo.RatesChannelsPaymentMethods rcpm
	INNER JOIN dbo.RatesChannels rc ON rcpm.RateChannel_UID = rc.UID
	INNER JOIN dbo.Rates r ON rc.Rate_UID = r.UID
	WHERE (r.Property_UID=@propertyId and r.UID <> @rateId)

	DELETE rc
	FROM dbo.RatesChannels rc
	INNER JOIN dbo.Rates r ON rc.Rate_UID = r.UID
	WHERE (r.Property_UID=@propertyId and r.UID <> @rateId)

	DELETE rcp
	FROM dbo.RateCancellationPolicies rcp
	INNER JOIN dbo.Rates r ON rcp.Rate_UID = r.UID
	WHERE (r.Property_UID=@propertyId and r.UID <> @rateId)

	delete from dbo.Rates where (Property_UID=@propertyId and UID <> @rateId)
	
	delete from dbo.RateLanguages where Rate_UID=@rateId
	insert into dbo.RateLanguages (Rate_UID,RateName,RateDescription,Language_UID,CreatedDate,ModifiedDate) 
	values (@rateId,'Rate para os IT','Rate para ser usada como defeito nos IT',8,@dateUtcNow,@dateUtcNow)

	update dbo.Rates
	set 
		Name='Rate para os IT',
		RateCategory_UID=1,
		IsPriceDerived=0,
		IsPercentage=0,
		IsValueDecrease=0,
		IsYielding=0,
		IsAvailableToTPI=0,
		IsDeleted=0,
		IsActive=1,
		DepositPolicy_UID=@depositPolicyId,
		CancellationPolicy_UID=@cancelationPolicyId,
		OtherPolicies=@otherPolicyId,
		Property_UID=@propertyId,
		IsParity=0,
		RateOrder=1,
		Description='Rate para ser usada como defeito nos IT',
		IsExclusiveForPackage=0,
		IsExclusiveForGroupCode=0,
		PriceModel=1,
		IsAllExtrasIncluded=0,
		Currency_UID=@currencyId,
		IsCurrencyChangeAllowed=0,
		AvailabilityType=1
	where UID=@rateId
END

--RateFolders
BEGIN
	delete from dbo.RateFolders where (Property_UID=@propertyId and UID <> @rateFolderId);

	update dbo.RateFolders
	set 
		Name='Rate folder para os IT',
		Property_UID=@propertyId,
		Sequence=1,
		IsDeleted=0,
		PriceModel=1,
		Type=0
	where UID=@rateFolderId
END

--RateRooms
BEGIN
	delete from dbo.RateRooms where (Rate_UID=@rateId and RoomType_UID <> @roomTypeId1)

	update dbo.RateRooms
	set 
		Rate_UID=@rateId,
		RoomType_UID=@roomTypeId1,
		Price=-1,
		Allotment=100,
		IsDeleted=0
	where UID=@rateRoomId
END

--RatesChannels
BEGIN
	delete from dbo.RatesChannels 
	where (Rate_UID=@rateId and Channel_UID <> @channelId_BE and Channel_UID <> @channelId_Atcbt)

	--Booking Engine
	update dbo.RatesChannels
	set 
		Rate_UID=@rateId,
		Channel_UID=1,
		Value=0,
		IsPercentage=1,
		RateModel_UID=1,
		ChannelCommissionCategory_UID=1,
		IsDeleted=0,
		PriceAddOnIsPercentage=0,
		PriceAddOnIsValueDecrease=0,
		IsBEMobileExclusive=0
	where UID=@rateChannelId1
	--4 Cantos
	update dbo.RatesChannels
	set 
		Rate_UID=@rateId,
		Channel_UID=@channelId_Atcbt,
		Value=0,
		IsPercentage=1,
		RateModel_UID=1,
		ChannelCommissionCategory_UID=1,
		IsDeleted=0,
		PriceAddOnIsPercentage=1,
		PriceAddOnIsValueDecrease=1,
		Commission=2,
		IsBEMobileExclusive=0
	where UID=@rateChannelId2
END

--RateBuyerGroups
BEGIN
	delete from dbo.RateBuyerGroups where Rate_UID=@rateId

	insert into dbo.RateBuyerGroups (Rate_UID,TPI_UID,IsPercentage,IsValueDecrease,GDSValueIsPercentage,GDSValueIsDecrease) 
	values 
	 (@rateId,@tpiId1,0,0,0,0)
	,(@rateId,@tpiId2,0,0,0,0)
END

--RatesExtras
BEGIN
	delete from dbo.RatesExtras where (Rate_UID=@rateId and Extra_UID <> @extraId)

	update dbo.RatesExtras
	set 
		Rate_UID=@rateId,
		Extra_UID=@extraId,
		IsAvilableDiffPeriods=0,
		IsDeleted=0,
		IsIncluded=1
	where UID=@rateExtraId
END

--RatesIncentives
BEGIN
	delete from dbo.RatesIncentive where Rate_UID=@rateId

	insert into dbo.RatesIncentive (DateFrom,DateTo,Rate_UID,Incentive_UID,CreatedDate,ModifiedDate,IsCumulative,IsAvailableForDiferentPeriods) 
	values ('2019-01-01','2021-12-31',@rateId,@incentiveId,@dateUtcNow,@dateUtcNow,0,0)
END

--RateDepositPolicies
BEGIN
	delete from dbo.RateDepositPolicies where (Rate_UID=@rateId and DepositPolicy_UID <> @depositPolicyId)
	delete from dbo.RateDepositPoliciesPeriods where RateDepositPolicies_UID=@rateDepositPolicyId

	insert into dbo.RateDepositPoliciesPeriods (RateDepositPolicies_UID,DateFrom,DateTo) 
	values (@rateDepositPolicyId,'2019-01-01','2019-12-31')

	update dbo.RateDepositPolicies
	set 
		Rate_UID=@rateId,
		DepositPolicy_UID=@depositPolicyId,
		IsDeleted=0
	where UID=@rateDepositPolicyId
END

--RateCancellationPolicies
BEGIN
	delete from dbo.RateCancellationPolicies where (Rate_UID=@rateId and CancellationPolicy_UID <> @cancelationPolicyId)
	delete from dbo.RateCancellationPoliciesPeriods where RateCancellationPolicies_UID=@rateCancelationPolicyId

	insert into dbo.RateCancellationPoliciesPeriods (RateCancellationPolicies_UID,DateFrom,DateTo) 
	values (@rateCancelationPolicyId,'2019-01-01','2019-12-31')

	update dbo.RateCancellationPolicies
	set 
		Rate_UID=@rateId,
		CancellationPolicy_UID=@cancelationPolicyId,
		IsDeleted=0
	where UID=@rateCancelationPolicyId
END

--RateTaxPolicies
BEGIN
	delete from dbo.RatesTaxPolicies where Rate_UID=@rateId

	insert into dbo.RatesTaxPolicies (Rate_UID,TaxPolicy_UID,CreatedDate,ModifiedDate) 
	values (@rateId,@taxPolicyId,@dateUtcNow,@dateUtcNow)
END

--TermsAndCondition
BEGIN
	DELETE tacl
	FROM dbo.TermsAndConditionLanguages tacl
	INNER JOIN dbo.TermsAndCondition tac ON tacl.TermsAndCondition_UID = tac.UID
	WHERE tac.Property_UID=@propertyId

	delete from dbo.TermsAndCondition where Property_UID=@propertyId

	insert into dbo.TermsAndCondition (Description,Property_UID) 
	values ('Terms and Conditions para os IT',@propertyId)
END

COMMIT TRAN