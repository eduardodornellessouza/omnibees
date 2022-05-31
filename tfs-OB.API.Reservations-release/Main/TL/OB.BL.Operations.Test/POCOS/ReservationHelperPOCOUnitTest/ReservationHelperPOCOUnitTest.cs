using Dapper;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.BL.Operations.Exceptions;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.BL.Operations.Internal.TypeConverters;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.DL.Common.QueryResultObjects;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Data.CRM;
using PO.BL.Contracts.Data.OperatorMarkupCommission;
using PO.BL.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using domainReservation = OB.Domain.Reservations;
using ExternalRatesTypeTarget = PO.BL.Contracts.Data.OperatorMarkupCommission.ExternalRatesTypeTarget;
using OBcontractsRates = OB.BL.Contracts.Data.Rates;
using reservationContracts = OB.Reservation.BL.Contracts.Data.Reservations;
using Version = OB.Reservation.BL.Contracts.Data.Version;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class ReservationHelperPOCOUnitTest : UnitBaseTest
    {
        private reservationContracts.Reservation inputReservation;
        private reservationContracts.Reservation expectedReservation;
        private ReservationDataContext inputReservationDataContext;
        private IReservationHelperPOCO reservationHelperPOCO;
        private decimal failMargin = 0.01M;

        private Mock<IPortalRepository> _portalMock = null;
        private Mock<ISqlManager> _sqlManagerRepoMock = null;
        private Mock<IOBPromotionalCodeRepository> _promoCodesRepo = null;
        private Mock<IOBIncentiveRepository> _incentivesRepoMock = null;
        private Mock<IOBRateRoomDetailsForReservationRoomRepository> _rrdRepoMock = null;
        private Mock<IOBChildTermsRepository> _obChildTermsRepoMock = null;
        private Mock<IOBAppSettingRepository> _obAppSettingsRepoMock = null;
        private Mock<IOBExtrasRepository> _obExtraRepoMock = null;
        private Mock<IOBOtherPolicyRepository> _obOtherPolicyRepoMock = null;
        private Mock<IGroupRulesRepository> _groupRulesRepoMock = null;
        private Mock<IReservationsRepository> _reservationsRepoMock = null;

        private Mock<IReservationPricesCalculationPOCO> _reservationPricesPOCOMock = null;

        private List<SellRule> sellRulesList = null;
        private List<OBcontractsRates.RateRoomDetailReservation> rrdReservations = null;
        private List<Incentive> incentivesList = null;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var sessionFactoryMock = new Mock<ISessionFactory>();
            var repoFactoryMock = new Mock<IRepositoryFactory>();
            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);

            //Mock sqlmanager
            _sqlManagerRepoMock = new Mock<ISqlManager>(MockBehavior.Default);

            this.Container = this.Container.RegisterInstance(sessionFactoryMock.Object)
            .RegisterInstance(repoFactoryMock.Object)
            .AddExtension(new BusinessLayerModule())
            .RegisterInstance<ISqlManager>(_sqlManagerRepoMock.Object,
                new TransientLifetimeManager());

            #region Mock

            //Mock Portal rules
            _portalMock = new Mock<IPortalRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetPortalRepository(It.IsAny<IUnitOfWork>()))
                           .Returns(_portalMock.Object);

            //Mock getsqlmanager
            repoFactoryMock.Setup(x => x.GetSqlManager(It.IsAny<IUnitOfWork>(), OB.Domain.Reservations.Reservation.DomainScope))
                           .Returns(_sqlManagerRepoMock.Object);

            //Mock PromoCodes
            _promoCodesRepo = new Mock<IOBPromotionalCodeRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetOBPromotionalCodeRepository())
                        .Returns(_promoCodesRepo.Object);

            //Mock Incentives
            _incentivesRepoMock = new Mock<IOBIncentiveRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetOBIncentiveRepository())
                .Returns(_incentivesRepoMock.Object);

            //Mock RateRoomDetails for reservations
            _rrdRepoMock = new Mock<IOBRateRoomDetailsForReservationRoomRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetOBRateRoomDetailsForReservationRoomRepository())
                     .Returns(_rrdRepoMock.Object);

            //Mock childterms
            _obChildTermsRepoMock = new Mock<IOBChildTermsRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetOBChildTermsRepository())
                            .Returns(_obChildTermsRepoMock.Object);

            // Mock AppSettings
            _obAppSettingsRepoMock = new Mock<IOBAppSettingRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetOBAppSettingRepository())
                            .Returns(_obAppSettingsRepoMock.Object);

            // Mock OtherPolicies
            _obOtherPolicyRepoMock = new Mock<IOBOtherPolicyRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetOBOtherPolicyRepository())
                            .Returns(_obOtherPolicyRepoMock.Object);

            // Mock Extras
            _obExtraRepoMock = new Mock<IOBExtrasRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetOBExtrasRepository())
                            .Returns(_obExtraRepoMock.Object);

            //Mock GroupRules
            _groupRulesRepoMock = new Mock<IGroupRulesRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetGroupRulesRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_groupRulesRepoMock.Object);

            _reservationsRepoMock = new Mock<IReservationsRepository>(MockBehavior.Default);
            repoFactoryMock.Setup(x => x.GetReservationsRepository(It.IsAny<IUnitOfWork>()))
                            .Returns(_reservationsRepoMock.Object);

            #endregion

            this.reservationHelperPOCO = this.Container.Resolve<IReservationHelperPOCO>();

            FillGroupRulesMock();
            MockGroupRulesRepo();
        }

        #region Buidler

        private void CreateReservation()
        {
            inputReservation = new reservationContracts.Reservation()
            {
                PropertyBaseCurrencyExchangeRate = 0.5M,
                RoomsExtras = 1,
                RoomsPriceSum = 2,
                RoomsTax = 3,
                RoomsTotalAmount = 4,
                Tax = 5,
                TotalAmount = 6,
                TotalTax = 7,
                PaymentAmountCaptured = 8,
                ChannelProperties_Value = 9,
                InstallmentAmount = 10,
                SalesmanCommission = 11
            };

            expectedReservation = new reservationContracts.Reservation()
            {
                PropertyBaseCurrencyExchangeRate = 0.5M,
                RoomsExtras = 2,
                RoomsPriceSum = 4,
                RoomsTax = 6,
                RoomsTotalAmount = 8,
                Tax = 10,
                TotalAmount = 12,
                TotalTax = 14,
                PaymentAmountCaptured = 16,
                ChannelProperties_Value = 18,
                InstallmentAmount = 20,
                SalesmanCommission = 22
            };
        }

        private void WithRoom(bool withReservationRoomDetails, bool withReservationRoomExtras, bool withReservationRoomTaxPolicies)
        {
            var newRoom = new reservationContracts.ReservationRoom()
            {
                UID = 1,
                CancellationValue = 1,
                ReservationRoomsExtrasSum = 2,
                ReservationRoomsPriceSum = 3,
                ReservationRoomsTotalAmount = 4,
                TPIDiscountValue = 5,
                TotalTax = 6,
                Rate_UID = 1,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(3)
            };

            var expectedRoom = new reservationContracts.ReservationRoom()
            {
                UID = 1,
                CancellationValue = 2,
                ReservationRoomsExtrasSum = 4,
                ReservationRoomsPriceSum = 6,
                ReservationRoomsTotalAmount = 8,
                TPIDiscountValue = 10,
                TotalTax = 12,
                Rate_UID = 1,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(3)
            };

            if (withReservationRoomDetails)
            {
                newRoom.ReservationRoomDetails = new List<reservationContracts.ReservationRoomDetail>()
                {
                    new reservationContracts.ReservationRoomDetail()
                    {
                        ReservationRoom_UID = 1,
                        AdultPrice = 7,
                        ChildPrice = 8,
                        Price = 9
                    }
                };

                expectedRoom.ReservationRoomDetails = new List<reservationContracts.ReservationRoomDetail>()
                {
                    new reservationContracts.ReservationRoomDetail()
                    {
                        ReservationRoom_UID = 1,
                        AdultPrice = 14,
                        ChildPrice = 16,
                        Price = 18
                    }
                };


                newRoom.ReservationRoomDetails[0].ReservationRoomDetailsAppliedIncentives = new List<reservationContracts.ReservationRoomDetailsAppliedIncentive>()
                {
                    new reservationContracts.ReservationRoomDetailsAppliedIncentive()
                    {
                        DiscountValue = 1
                    }
                };

                expectedRoom.ReservationRoomDetails[0].ReservationRoomDetailsAppliedIncentives = new List<reservationContracts.ReservationRoomDetailsAppliedIncentive>()
                {
                    new reservationContracts.ReservationRoomDetailsAppliedIncentive()
                    {
                        DiscountValue = 2
                    }
                };
                
            }

            if (withReservationRoomExtras)
            {
                newRoom.ReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
                {
                    new reservationContracts.ReservationRoomExtra()
                    {
                        ReservationRoom_UID = 1,
                        Total_Price = 10,
                        Total_VAT = 11
                    }
                };

                expectedRoom.ReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
                {
                    new reservationContracts.ReservationRoomExtra()
                    {
                        ReservationRoom_UID = 1,
                        Total_Price = 20,
                        Total_VAT = 22
                    }
                };
            }

            if (withReservationRoomTaxPolicies)
            {
                newRoom.ReservationRoomTaxPolicies = new List<reservationContracts.ReservationRoomTaxPolicy>()
                {
                    new reservationContracts.ReservationRoomTaxPolicy()
                    {
                        TaxDefaultValue = 12,
                        TaxCalculatedValue = 13
                    }
                };

                expectedRoom.ReservationRoomTaxPolicies = new List<reservationContracts.ReservationRoomTaxPolicy>()
                {
                    new reservationContracts.ReservationRoomTaxPolicy()
                    {
                        TaxDefaultValue = 24,
                        TaxCalculatedValue = 26
                    }
                };
            }

            inputReservation.ReservationRooms = new List<reservationContracts.ReservationRoom>() { newRoom };
            expectedReservation.ReservationRooms = new List<reservationContracts.ReservationRoom>() { expectedRoom };
        }

        private void WithGuest()
        {
            inputReservation.Guest = new Guest()
            {
                UID = 1,
                FirstName = "teste1",
                LastName = "teste2",
                UserName = "teste12",
                LoyaltyLevel_UID = null,
                Client_UID = 1,
                Property_UID = 1635
            };

            expectedReservation.Guest = new Guest()
            {
                UID = 1,
                FirstName = "teste1",
                LastName = "teste2",
                UserName = "teste12",
                LoyaltyLevel_UID = null,
                Client_UID = 1,
                Property_UID = 1635
            };
        }

        private void WithPaymentDetail()
        {
            inputReservation.ReservationPaymentDetail = new reservationContracts.ReservationPaymentDetail()
            {
                Amount = 1
            };

            expectedReservation.ReservationPaymentDetail = new reservationContracts.ReservationPaymentDetail()
            {
                Amount = 2
            };
        }

        private void WithPartialPaymentDetails()
        {
            inputReservation.ReservationPartialPaymentDetails = new List<reservationContracts.ReservationPartialPaymentDetail>()
            {
                new reservationContracts.ReservationPartialPaymentDetail()
                {
                    Amount = 1
                }
            };

            expectedReservation.ReservationPartialPaymentDetails = new List<reservationContracts.ReservationPartialPaymentDetail>()
            {
                new reservationContracts.ReservationPartialPaymentDetail()
                {
                    Amount = 2
                }
            };
        }

        private void WithAdditionalData(bool withExternalSellingReservation, bool withReservationRoomList)
        {
            inputReservation.ReservationAdditionalData = new reservationContracts.ReservationsAdditionalData();
            expectedReservation.ReservationAdditionalData = new reservationContracts.ReservationsAdditionalData();

            if (withExternalSellingReservation)
            {
                inputReservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule = new List<reservationContracts.ExternalSellingReservationInformation>();

                inputReservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule.Add(new reservationContracts.ExternalSellingReservationInformation()
                {
                    KeeperType = Reservation.BL.Constants.PO_KeeperType.Channel,
                    RoomsPriceSum = 1,
                    RoomsTotalAmount = 2,
                    TotalAmount = 3,
                    TotalTax = 4
                });

                expectedReservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule = new List<reservationContracts.ExternalSellingReservationInformation>();
                expectedReservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule.Add(new reservationContracts.ExternalSellingReservationInformation()
                {
                    KeeperType = Reservation.BL.Constants.PO_KeeperType.Channel,
                    RoomsPriceSum = 2,
                    RoomsTotalAmount = 4,
                    TotalAmount = 6,
                    TotalTax = 8
                });

                inputReservation.ReservationAdditionalData.BigPullAuthRequestor_UID = 10;
            }

            if (withReservationRoomList)
            {
                inputReservation.ReservationAdditionalData.ReservationRoomList = new List<reservationContracts.ReservationRoomAdditionalData>()
                {
                    new reservationContracts.ReservationRoomAdditionalData()
                    {
                        ExternalSellingInformationByRule = new List<reservationContracts.ExternalSellingRoomInformation>()
                        {
                            new reservationContracts.ExternalSellingRoomInformation {
                                KeeperType = Reservation.BL.Constants.PO_KeeperType.Channel,
                                ReservationRoomsExtrasSum = 5,
                                ReservationRoomsPriceSum = 6,
                                ReservationRoomsTotalAmount = 7,
                                TotalTax = 8,
                                PricesPerDay = new List<reservationContracts.PriceDay>()
                                {
                                    new reservationContracts.PriceDay() { Price = 9 }
                                },
                                TaxPolicies = new List<reservationContracts.ReservationRoomTaxPolicy>()
                                {
                                    new reservationContracts.ReservationRoomTaxPolicy() { TaxCalculatedValue = 10 }
                                }
                            }
                        }
                    }
                };
                inputReservation.ReservationAdditionalData.BigPullAuthRequestor_UID = 10;

                expectedReservation.ReservationAdditionalData.ReservationRoomList = new List<reservationContracts.ReservationRoomAdditionalData>()
                {
                    new reservationContracts.ReservationRoomAdditionalData()
                    {
                        ExternalSellingInformationByRule = new List<reservationContracts.ExternalSellingRoomInformation>()
                        {
                            new reservationContracts.ExternalSellingRoomInformation {
                                KeeperType = Reservation.BL.Constants.PO_KeeperType.Channel,
                                ReservationRoomsExtrasSum = 10,
                                ReservationRoomsPriceSum = 12,
                                ReservationRoomsTotalAmount = 14,
                                TotalTax = 16,
                                PricesPerDay = new List<reservationContracts.PriceDay>()
                                {
                                    new reservationContracts.PriceDay() { Price = 18 }
                                },
                                TaxPolicies = new List<reservationContracts.ReservationRoomTaxPolicy>()
                                {
                                    new reservationContracts.ReservationRoomTaxPolicy() { TaxCalculatedValue = 20 }
                                }
                            }
                        }
                    }
                };
                expectedReservation.ReservationAdditionalData.BigPullAuthRequestor_UID = 10;
            }
        }

        #region PULL

        private void CreateReservation_TreatPull(decimal inputRoomsExtras, decimal inputRoomsPriceSum, decimal inputRoomsTax, decimal inputRoomsTotalAmount, decimal inputTotalAmount, decimal inputTotalTax,
            decimal expectRoomsExtras, decimal expectRoomsPriceSum, decimal expectRoomsTax, decimal expectRoomsTotalAmount, decimal expectTotalAmount, decimal expectTotalTax)
        {
            inputReservation = new reservationContracts.Reservation()
            {
                PropertyBaseCurrencyExchangeRate = 0.5M,
                Channel_UID = 247,
                ReservationBaseCurrency_UID = 34,
                Property_UID = 1635,
                RoomsExtras = inputRoomsExtras,
                RoomsPriceSum = inputRoomsPriceSum,
                RoomsTax = inputRoomsTax,
                RoomsTotalAmount = inputRoomsTotalAmount,
                Tax = 6,
                TotalAmount = inputTotalAmount,
                TotalTax = inputTotalTax,
                PaymentAmountCaptured = 8,
                ChannelProperties_Value = 9,
                InstallmentAmount = 10,
                SalesmanCommission = 11
            };

            expectedReservation = new reservationContracts.Reservation()
            {
                PropertyBaseCurrencyExchangeRate = 0.5M,
                Channel_UID = 247,
                Property_UID = 1635,
                RoomsExtras = expectRoomsExtras,
                RoomsPriceSum = expectRoomsPriceSum,
                RoomsTax = expectRoomsTax,
                RoomsTotalAmount = expectRoomsTotalAmount,
                Tax = 6,
                TotalAmount = expectTotalAmount,
                TotalTax = expectTotalTax,
                PaymentAmountCaptured = 16,
                ChannelProperties_Value = 18,
                InstallmentAmount = 20,
                SalesmanCommission = 22
            };
        }

        private void CreateReservationDataContext_TreatPull()
        {
            inputReservationDataContext = new ReservationDataContext()
            {
                Channel_UID = 247,
                Client_UID = 1,
                PropertyBaseCurrency_UID = 34
            };
        }

        private void WithRoom_TreatPull(bool withReservationRoomDetails, bool withReservationRoomExtras, bool withReservationRoomTaxPolicies,
            decimal inputReservationRoomsExtrasSum, decimal inputReservationRoomsPriceSum, decimal inputReservationRoomsTotalAmount, decimal inputTotalTax,
            decimal expectReservationRoomsExtrasSum, decimal expectReservationRoomsPriceSum, decimal expectReservationRoomsTotalAmount, decimal expectTotalTax, decimal pricePerDay)
        {
            var newRoom = new reservationContracts.ReservationRoom()
            {
                UID = 1,
                CancellationValue = 1,
                ReservationRoomsExtrasSum = inputReservationRoomsExtrasSum,
                ReservationRoomsPriceSum = inputReservationRoomsPriceSum,
                ReservationRoomsTotalAmount = inputReservationRoomsTotalAmount,
                TPIDiscountValue = 5,
                TotalTax = inputTotalTax,
                Rate_UID = 1,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(1),
                AdultCount = 2,
                RoomType_UID = 1,
                CommissionType = 1
            };

            var expectedRoom = new reservationContracts.ReservationRoom()
            {
                UID = 1,
                CancellationValue = 2,
                ReservationRoomsExtrasSum = expectReservationRoomsExtrasSum,
                ReservationRoomsPriceSum = expectReservationRoomsPriceSum,
                ReservationRoomsTotalAmount = expectReservationRoomsTotalAmount,
                TPIDiscountValue = 10,
                TotalTax = expectTotalTax,
                Rate_UID = 1,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(1),
                AdultCount = 2,
                RoomType_UID = 1,
                CommissionType = 1
            };

            if (withReservationRoomDetails)
            {
                newRoom.ReservationRoomDetails = new List<reservationContracts.ReservationRoomDetail>()
                {
                    new reservationContracts.ReservationRoomDetail()
                    {
                        ReservationRoom_UID = 1,
                        AdultPrice = 100,
                        ChildPrice = 50,
                        Price = pricePerDay,
                        Date = DateTime.UtcNow

                    }
                };

                expectedRoom.ReservationRoomDetails = new List<reservationContracts.ReservationRoomDetail>()
                {
                    new reservationContracts.ReservationRoomDetail()
                    {
                        ReservationRoom_UID = 1,
                        AdultPrice = 100,
                        ChildPrice = 50,
                        Price = pricePerDay,
                        Date = DateTime.UtcNow
                    }
                };
            }

            if (withReservationRoomExtras)
            {
                newRoom.ReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
                {
                    new reservationContracts.ReservationRoomExtra()
                    {
                        ReservationRoom_UID = 1,
                        Total_Price = 10,
                        Total_VAT = 11
                    }
                };

                expectedRoom.ReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
                {
                    new reservationContracts.ReservationRoomExtra()
                    {
                        ReservationRoom_UID = 1,
                        Total_Price = 20,
                        Total_VAT = 22
                    }
                };
            }

            if (withReservationRoomTaxPolicies)
            {
                newRoom.ReservationRoomTaxPolicies = new List<reservationContracts.ReservationRoomTaxPolicy>()
                {
                    new reservationContracts.ReservationRoomTaxPolicy()
                    {
                        TaxDefaultValue = 6,
                        TaxCalculatedValue = 7
                    }
                };

                expectedRoom.ReservationRoomTaxPolicies = new List<reservationContracts.ReservationRoomTaxPolicy>()
                {
                    new reservationContracts.ReservationRoomTaxPolicy()
                    {
                        TaxDefaultValue = 6,
                        TaxCalculatedValue = 7
                    }
                };
            }

            inputReservation.ReservationRooms = new List<reservationContracts.ReservationRoom>() { newRoom };
            expectedReservation.ReservationRooms = new List<reservationContracts.ReservationRoom>() { expectedRoom };
        }

        private void WithPaymentDetail_TreatPull()
        {
            inputReservation.ReservationPaymentDetail = new reservationContracts.ReservationPaymentDetail()
            {
                Amount = 1
            };

            expectedReservation.ReservationPaymentDetail = new reservationContracts.ReservationPaymentDetail()
            {
                Amount = 2
            };
        }

        private void WithPartialPaymentDetails_TreatPull()
        {
            inputReservation.ReservationPartialPaymentDetails = new List<reservationContracts.ReservationPartialPaymentDetail>()
            {
                new reservationContracts.ReservationPartialPaymentDetail()
                {
                    Amount = 1
                }
            };

            expectedReservation.ReservationPartialPaymentDetails = new List<reservationContracts.ReservationPartialPaymentDetail>()
            {
                new reservationContracts.ReservationPartialPaymentDetail()
                {
                    Amount = 2
                }
            };
        }

        private void WithAdditionalData_TreatPull()
        {
            inputReservation.ReservationAdditionalData = new reservationContracts.ReservationsAdditionalData();
        }

        #endregion PULL

        #region BE

        private void CreateReservation_TreatBe(decimal roomsExtras, decimal roomsPriceSum, decimal roomsTax, decimal roomsTotalAmount, decimal totalAmount, decimal totalTax)
        {
            inputReservation = new reservationContracts.Reservation()
            {
                PropertyBaseCurrencyExchangeRate = 1M,
                Channel_UID = 1,
                ReservationBaseCurrency_UID = 34,
                Property_UID = 1635,
                RoomsExtras = roomsExtras,
                RoomsPriceSum = roomsPriceSum,
                RoomsTax = roomsTax,
                RoomsTotalAmount = roomsTotalAmount,
                Tax = 6,
                TotalAmount = totalAmount,
                TotalTax = totalTax,
                PaymentAmountCaptured = 8,
                ChannelProperties_Value = 9,
                InstallmentAmount = 10,
                SalesmanCommission = 11
            };
        }

        private void CreateReservationDataContext_TreatBe()
        {
            inputReservationDataContext = new ReservationDataContext()
            {
                Channel_UID = 1,
                Client_UID = 1,
                PropertyBaseCurrency_UID = 34
            };
        }

        private void WithRoom_TreatBe(bool withReservationRoomDetails, bool withReservationRoomExtras, bool withReservationRoomTaxPolicies,
            decimal reservationRoomsExtrasSum, decimal reservationRoomsPriceSum, decimal reservationRoomsTotalAmount, decimal totalTax, decimal pricePerDay)
        {
            var newRoom = new reservationContracts.ReservationRoom()
            {
                UID = 1,
                CancellationValue = 1,
                ReservationRoomsExtrasSum = reservationRoomsExtrasSum,
                ReservationRoomsPriceSum = reservationRoomsPriceSum,
                ReservationRoomsTotalAmount = reservationRoomsTotalAmount,
                TPIDiscountValue = 5,
                TotalTax = totalTax,
                Rate_UID = 1,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(1),
                AdultCount = 2,
                RoomType_UID = 1,
                CommissionType = 1
            };

            if (withReservationRoomDetails)
            {
                newRoom.ReservationRoomDetails = new List<reservationContracts.ReservationRoomDetail>()
                {
                    new reservationContracts.ReservationRoomDetail()
                    {
                        ReservationRoom_UID = 1,
                        AdultPrice = 100,
                        ChildPrice = 50,
                        Price = pricePerDay,
                        Date = DateTime.UtcNow

                    }
                };
            }

            if (withReservationRoomExtras)
            {
                newRoom.ReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
                {
                    new reservationContracts.ReservationRoomExtra()
                    {
                        UID = 1,
                        ReservationRoom_UID = 1,
                        Extra_UID = 1,
                        Total_Price = 10,
                        Total_VAT = 11,
                        Qty = 1
                    }
                };
            }

            if (withReservationRoomTaxPolicies)
            {
                newRoom.ReservationRoomTaxPolicies = new List<reservationContracts.ReservationRoomTaxPolicy>()
                {
                    new reservationContracts.ReservationRoomTaxPolicy()
                    {
                        TaxDefaultValue = 6,
                        TaxCalculatedValue = 6
                    }
                };
            }

            inputReservation.ReservationRooms = new List<reservationContracts.ReservationRoom>() { newRoom };
        }

        private void WithPaymentDetail_TreatBe()
        {
            inputReservation.ReservationPaymentDetail = new reservationContracts.ReservationPaymentDetail()
            {
                Amount = 1
            };
        }

        private void WithPartialPaymentDetails_TreatBe()
        {
            inputReservation.ReservationPartialPaymentDetails = new List<reservationContracts.ReservationPartialPaymentDetail>()
            {
                new reservationContracts.ReservationPartialPaymentDetail()
                {
                    Amount = 1
                }
            };
        }

        #endregion BE

        private enum PORule
        {
            None = 0,
            IsOBTPIRule = 1,
            IsPOTPIRule = 2,
            IsRepresentativeRule = 3,
            IsOperatorRule = 4,
            IsRepresentativeAndOperatorRule = 5,
        };
        private void CreatePortalRules(PORule rule)
        {
            switch (rule)
            {
                case PORule.IsOBTPIRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 3,
                            ExternalName = "A TO Z TRAVEL",
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 10,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 16,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        },
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 3,
                            ExternalName = "A TO Z TRAVEL",
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 15,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 5,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        }
                    };
                    break;
                case PORule.IsPOTPIRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 1,
                            ExternalName = "Empresa A TO Z TRAVEL",
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 15,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 5,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        },
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 1,
                            ExternalName = "Empresa A TO Z TRAVEL",
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 18,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        }
                    };
                    break;
                case PORule.IsOperatorRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 247,
                            KeeperType = 2,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 10,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        },
                        new SellRule()
                        {
                            KeeperUid = 247,
                            KeeperType = 2,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 5,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 5,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        }
                    };
                    break;
                case PORule.IsRepresentativeRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 7,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 0,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 16,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = false,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 10,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 34
                        },
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 7,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 5,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = false,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 15,
                            CurrencyValueUID = 34
                        }
                    };
                    break;
                case PORule.IsRepresentativeAndOperatorRule:
                    sellRulesList = new List<SellRule>()
                    {
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 7,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 0,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 16,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = false,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 10,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 34
                        },
                        new SellRule()
                        {
                            KeeperUid = 126,
                            KeeperType = 7,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 5,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = false,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 15,
                            CurrencyValueUID = 34
                        },
                        new SellRule()
                        {
                            KeeperUid = 247,
                            KeeperType = 2,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 3,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 10,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 10,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = true,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        },
                        new SellRule()
                        {
                            KeeperUid = 247,
                            KeeperType = 2,
                            ExternalName = null,
                            PosCode = 0,
                            RuleType = 0,
                            RatesTypeTarget = ExternalRatesTypeTarget.Both,
                            MarkupType = ExternalApplianceType.Define,
                            Markup = 5,
                            CommissionType = ExternalApplianceType.Define,
                            Commission = 5,
                            Tax = 0,
                            CurrencyBaseUID = 19,
                            CommissionIsPercentage = false,
                            MarkupIsPercentage = true,
                            TaxIsPercentage = false,
                            MarkupCurrencyValue = 0,
                            TaxCurrencyValue = 0,
                            CurrencyValueUID = 0
                        }
                    };
                    break;
                default:
                    sellRulesList = new List<SellRule>();
                    break;
            }
        }

        private void CreateRateRoomDetails(int days, decimal rateModelValue, decimal priceAddOnValue, decimal adultPrice, decimal childPrice, bool isMarkup = true, bool isPackage = false, bool isCommission = false)
        {
            rrdReservations = new List<OBcontractsRates.RateRoomDetailReservation>();

            for (var i = 0; i < days; i++)
            {
                rrdReservations.Add(new OBcontractsRates.RateRoomDetailReservation()
                {
                    UID = 1,
                    AdultPrice = adultPrice, //100,
                    Adults = 2,
                    Allotment = 3,
                    AllotmentUsed = 3,
                    AppliedIncentives = null,
                    Channel_UID = 247,
                    ChildPrice = childPrice, //50,
                    Childs = 0,
                    ClosedOnArrival = false,
                    ClosedOnDeparture = false,
                    CurrencyId = 34,
                    Date = DateTime.UtcNow.AddDays(i),
                    DateRangeCount = 2,
                    FinalPrice = 0,
                    IsAvailableToTPI = false,
                    MaxFreeChilds = 2,
                    RateModelIsPercentage = false,
                    PriceAddOnIsValueDecrease = false,
                    PriceAddOnValue = priceAddOnValue, //15,
                    PriceAddOnIsPercentage = true,
                    PriceModel = true,
                    IsMarkup = isMarkup,
                    IsPackage = isPackage,
                    IsCommission = isCommission,
                    RateModelValue = rateModelValue, //30,
                    Rate_UID = 1
                });
            }
        }

        private void CreateIncentives()
        {
            incentivesList = new List<Incentive>
            {
                new Incentive
                {
                    UID = 1,
                    Property_UID = 1635,
                    Rate_UID = 1,
                    Days = 2,
                    DiscountPercentage = 30,
                    FreeDays = 2,
                    IncentiveType_UID = 1,
                    IsFreeDaysAtBegin = true,
                    Name = string.Empty,
                    IsDeleted = false,
                    IsCumulative = false,
                    DayDiscount = new List<decimal>() {}
                }
            };
        }

        private readonly OB.Domain.Reservations.GroupRule groupRule = new OB.Domain.Reservations.GroupRule
        {
            RuleType = OB.Domain.Reservations.RuleType.Pull,
            BusinessRules = OB.Domain.Reservations.BusinessRules.ValidateAllotment | OB.Domain.Reservations.BusinessRules.ValidateCancelationCosts | OB.Domain.Reservations.BusinessRules.ValidateGuarantee | OB.Domain.Reservations.BusinessRules.ValidateRestrictions
                | OB.Domain.Reservations.BusinessRules.HandleDepositPolicy | OB.Domain.Reservations.BusinessRules.ForceDefaultCancellationPolicy | OB.Domain.Reservations.BusinessRules.UseReservationTransactions | OB.Domain.Reservations.BusinessRules.CalculatePriceModel
                | OB.Domain.Reservations.BusinessRules.GenerateReservationNumber | OB.Domain.Reservations.BusinessRules.EncryptCreditCard | OB.Domain.Reservations.BusinessRules.ConvertValuesToPropertyCurrency
                | OB.Domain.Reservations.BusinessRules.PullTpiReservationCalculation | OB.Domain.Reservations.BusinessRules.ReturnSellingPrices
        };

        #endregion

        #region Mock Repository Calls

        private void MockPortalRulesRepository(PORule rule)
        {
            CreatePortalRules(rule);
            _portalMock.Setup(x => x.ListMarkupCommissionRules(It.IsAny<ListMarkupCommissionRulesRequest>()))
                .Returns((ListMarkupCommissionRulesRequest request) => {
                    return sellRulesList;
                });
        }

        private void MockSqlManagerRepository()
        {
            _sqlManagerRepoMock.Setup(x => x.ExecuteScalar<decimal>(It.IsAny<string>(), It.IsAny<DynamicParameters>(), CommandType.StoredProcedure)).Returns(1);
        }

        private void MockRateRoomDetailRepository()
        {
            _rrdRepoMock.Setup(x => x.ListRateRoomDetailForReservationRoom(It.IsAny<OB.BL.Contracts.Requests.ListRateRoomDetailForReservationRoomRequest>()))
            .Returns(rrdReservations);
        }

        private void MockChildTermsRepo()
        {
            _obChildTermsRepoMock.Setup(x => x.ListChildTerms(It.IsAny<ListChildTermsRequest>()))
                .Returns(new ListChildTermsResponse
                {
                    Result = new List<ChildTerm>(),
                    Status = Status.Success
                });
        }

        private void MockIncentives(bool? freeNights = null)
        {
            CreateIncentives();
            _incentivesRepoMock.Setup(x => x.ListIncentivesForReservationRoom(It.IsAny<OB.BL.Contracts.Requests.ListIncentivesForReservationRoomRequest>()))
                .Returns((OB.BL.Contracts.Requests.ListIncentivesForReservationRoomRequest req) =>
                {
                    if (freeNights.HasValue)
                    {
                        var fNs = freeNights.GetValueOrDefault();
                        return incentivesList.Where(y => (fNs ? (y.FreeDays > 0 && req.FreeNights) : (y.FreeDays == 0 && !req.FreeNights))).ToList();
                    }

                    return incentivesList;
                });
        }

        private void MockAppSettings()
        {
            _obAppSettingsRepoMock.Setup(x => x.ListSettings(It.IsAny<Contracts.Requests.ListSettingRequest>()))
                .Returns(new List<Contracts.Data.General.Setting>()
                {
                    new Contracts.Data.General.Setting() { Value = "All" }
                });
        }

        private void MockCalculateReservationRoomPrices(decimal pricePerDay)
        {
            _reservationPricesPOCOMock = new Mock<IReservationPricesCalculationPOCO>(MockBehavior.Default);
            this.Container = this.Container.RegisterInstance(_reservationPricesPOCOMock.Object);

            _reservationPricesPOCOMock.Setup(x => x.CalculateReservationRoomPrices(It.IsAny<Internal.BusinessObjects.ModifyClasses.CalculateFinalPriceParameters>()))
                .Returns(new List<OBcontractsRates.RateRoomDetailReservation>()
                {
                    new OBcontractsRates.RateRoomDetailReservation() { FinalPrice = pricePerDay }
                });
        }

        private void MockListTaxPoliciesByRateIds(decimal taxValue)
        {
            _obOtherPolicyRepoMock.Setup(x => x.ListTaxPoliciesByRateIds(It.IsAny<Contracts.Requests.ListTaxPoliciesByRateIdsRequest>()))
                .Returns(new List<OBcontractsRates.TaxPolicy>()
                {
                    new Contracts.Data.Rates.TaxPolicy { Rate_UID = 1, Value = taxValue, IsPerNight = true }
                });
        }

        private void MockListRatesExtras(decimal extraValue)
        {
            _obExtraRepoMock.Setup(x => x.ListRatesExtras(It.IsAny<Contracts.Requests.ListRatesExtrasRequest>()))
                .Returns(new Dictionary<long, List<OB.BL.Contracts.Data.Rates.Extra>>()
                {
                    { 1, new List<OBcontractsRates.Extra>() { new OBcontractsRates.Extra { UID = 1, Value = extraValue } } }
                });
        }

        private void MockGroupRulesRepo()
        {
            _groupRulesRepoMock.Setup(x => x.GetGroupRule(It.IsAny<DL.Common.Criteria.GetGroupRuleCriteria>()))
                .Returns((DL.Common.Criteria.GetGroupRuleCriteria criteria) =>
                {
                    return groupRuleList.AsQueryable().FirstOrDefault(x => x.RuleType == criteria.RuleType);
                });
        }

        #endregion


        [TestMethod]
        [TestCategory("ReservationConverter")]
        public void Test_ConvertAllValuesToRateCurrency()
        {
            // Build
            CreateReservation();
            WithRoom(true, true, true);
            WithPaymentDetail();
            WithPartialPaymentDetails();
            WithAdditionalData(true, true);

            //Act
            var convertedReservation = inputReservation;
            var result = reservationHelperPOCO.ConvertAllValuesToRateCurrency(convertedReservation);

            // Asserts
            string notNullMsg = "Converted '{0}' must be not null.";
            string areEqMsg = "'{0}' value must be converted to rate currency.";

            // Assert Base reservation values
            Assert.IsNotNull(convertedReservation, string.Format(notNullMsg, "Reservation"));
            Assert.IsTrue(result, "Result must be true.");
            Assert.AreEqual(expectedReservation.RoomsExtras, convertedReservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(expectedReservation.RoomsPriceSum, convertedReservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, convertedReservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, convertedReservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.Tax, convertedReservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(expectedReservation.TotalAmount, convertedReservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, convertedReservation.TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedReservation.PaymentAmountCaptured, convertedReservation.PaymentAmountCaptured, string.Format(areEqMsg, "PaymentAmountCaptured"));
            Assert.AreEqual(expectedReservation.ChannelProperties_Value, convertedReservation.ChannelProperties_Value, string.Format(areEqMsg, "ChannelProperties_Value"));
            Assert.AreEqual(expectedReservation.InstallmentAmount, convertedReservation.InstallmentAmount, string.Format(areEqMsg, "InstallmentAmount"));
            Assert.AreEqual(expectedReservation.SalesmanCommission, convertedReservation.SalesmanCommission, string.Format(areEqMsg, "SalesmanCommission"));

            // Assert Reservation Rooms values
            Assert.IsNotNull(convertedReservation.ReservationRooms, string.Format(notNullMsg, "ReservationRooms"));
            Assert.AreEqual(convertedReservation.ReservationRooms.Count, 1);
            var convertedRoom = convertedReservation.ReservationRooms[0];
            var expectedRoom = expectedReservation.ReservationRooms[0];
            Assert.AreEqual(expectedRoom.CancellationValue, convertedRoom.CancellationValue, string.Format(areEqMsg, "CancellationValue"));
            Assert.AreEqual(expectedRoom.ReservationRoomsExtrasSum, convertedRoom.ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedRoom.ReservationRoomsPriceSum, convertedRoom.ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedRoom.ReservationRoomsTotalAmount, convertedRoom.ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedRoom.TPIDiscountValue, convertedRoom.TPIDiscountValue, string.Format(areEqMsg, "TPIDiscountValue"));
            Assert.AreEqual(expectedRoom.TotalTax, convertedRoom.TotalTax, string.Format(areEqMsg, "TotalTax"));

            // Assert Reservation Room Details
            Assert.IsNotNull(convertedRoom.ReservationRoomDetails, string.Format(notNullMsg, "ReservationRoomDetails"));
            Assert.AreEqual(1, convertedRoom.ReservationRoomDetails.Count);
            Assert.AreEqual(expectedRoom.ReservationRoomDetails[0].AdultPrice, convertedRoom.ReservationRoomDetails[0].AdultPrice, string.Format(areEqMsg, "AdultPrice"));
            Assert.AreEqual(expectedRoom.ReservationRoomDetails[0].ChildPrice, convertedRoom.ReservationRoomDetails[0].ChildPrice, string.Format(areEqMsg, "ChildPrice"));
            Assert.AreEqual(expectedRoom.ReservationRoomDetails[0].Price, convertedRoom.ReservationRoomDetails[0].Price, string.Format(areEqMsg, "Price"));

            // Assert Reservation Room Details Applied Incentives 
            Assert.IsNotNull(convertedRoom.ReservationRoomDetails[0].ReservationRoomDetailsAppliedIncentives, string.Format(notNullMsg, "ReservationRoomDetailsAppliedIncentives"));
            Assert.AreEqual(1, convertedRoom.ReservationRoomDetails[0].ReservationRoomDetailsAppliedIncentives.Count);
            Assert.AreEqual(expectedRoom.ReservationRoomDetails[0].ReservationRoomDetailsAppliedIncentives[0].DiscountValue, convertedRoom.ReservationRoomDetails[0].ReservationRoomDetailsAppliedIncentives[0].DiscountValue, string.Format(areEqMsg, "DiscountValue"));
            
            // Assert Reservation Room Extras
            Assert.IsNotNull(convertedRoom.ReservationRoomExtras, string.Format(notNullMsg, "ReservationRoomExtras"));
            Assert.AreEqual(1, convertedRoom.ReservationRoomExtras.Count);
            Assert.AreEqual(expectedRoom.ReservationRoomExtras[0].Total_Price, convertedRoom.ReservationRoomExtras[0].Total_Price, string.Format(areEqMsg, "Total_Price"));
            Assert.AreEqual(expectedRoom.ReservationRoomExtras[0].Total_VAT, convertedRoom.ReservationRoomExtras[0].Total_VAT, string.Format(areEqMsg, "Total_VAT"));

            // Assert Reservation Room TaxPolicies
            Assert.IsNotNull(convertedRoom.ReservationRoomTaxPolicies, string.Format(notNullMsg, "ReservationRoomTaxPolicies"));
            Assert.AreEqual(1, convertedRoom.ReservationRoomTaxPolicies.Count);
            Assert.AreEqual(expectedRoom.ReservationRoomTaxPolicies[0].TaxDefaultValue, convertedRoom.ReservationRoomTaxPolicies[0].TaxDefaultValue, string.Format(areEqMsg, "TaxDefaultValue"));
            Assert.AreEqual(expectedRoom.ReservationRoomTaxPolicies[0].TaxCalculatedValue, convertedRoom.ReservationRoomTaxPolicies[0].TaxCalculatedValue, string.Format(areEqMsg, "TaxCalculatedValue"));

            // Assert Reservation PaymentDetail
            Assert.IsNotNull(convertedReservation.ReservationPaymentDetail, string.Format(notNullMsg, "ReservationPaymentDetail"));
            Assert.AreEqual(expectedReservation.ReservationPaymentDetail.Amount, convertedReservation.ReservationPaymentDetail.Amount, string.Format(areEqMsg, "PaymentDetail Amount"));

            // Assert Reservation Partial Payment Details
            Assert.IsNotNull(convertedReservation.ReservationPartialPaymentDetails, string.Format(notNullMsg, "ReservationPartialPaymentDetails"));
            Assert.AreEqual(1, convertedReservation.ReservationPartialPaymentDetails.Count);
            Assert.AreEqual(expectedReservation.ReservationPartialPaymentDetails[0].Amount, convertedReservation.ReservationPartialPaymentDetails[0].Amount, string.Format(areEqMsg, "Partial PaymentDetail Amount"));

            // Assert Additional Data ExternalSellingReservationInformation
            Assert.IsNotNull(convertedReservation.ReservationAdditionalData, string.Format(notNullMsg, "ReservationAdditionalData"));
            Assert.IsNotNull(convertedReservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule, string.Format(notNullMsg, "ExternalSellingReservationInformation"));
            var convertedExtSellingInfo = convertedReservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule[0];
            var expectedExtSellingInfo = expectedReservation.ReservationAdditionalData.ExternalSellingReservationInformationByRule[0];
            Assert.AreEqual(expectedExtSellingInfo.RoomsPriceSum, convertedExtSellingInfo.RoomsPriceSum, string.Format(areEqMsg, "ExternalSellingReservationInformation RoomsPriceSum"));
            Assert.AreEqual(expectedExtSellingInfo.RoomsTotalAmount, convertedExtSellingInfo.RoomsTotalAmount, string.Format(areEqMsg, "ExternalSellingReservationInformation RoomsTotalAmount"));
            Assert.AreEqual(expectedExtSellingInfo.TotalAmount, convertedExtSellingInfo.TotalAmount, string.Format(areEqMsg, "ExternalSellingReservationInformation TotalAmount"));
            Assert.AreEqual(expectedExtSellingInfo.TotalTax, convertedExtSellingInfo.TotalTax, string.Format(areEqMsg, "ExternalSellingReservationInformation TotalTax"));

            // Assert Additional Data 
            Assert.IsNotNull(convertedReservation.ReservationAdditionalData.ReservationRoomList, string.Format(notNullMsg, "ReservationAdditionalData ReservationRoomList"));
            Assert.AreEqual(1, convertedReservation.ReservationAdditionalData.ReservationRoomList.Count);
            var convertedRoomExtSellInfo = convertedReservation.ReservationAdditionalData.ReservationRoomList[0].ExternalSellingInformationByRule[0];
            var expectedRoomExtSellInfo = expectedReservation.ReservationAdditionalData.ReservationRoomList[0].ExternalSellingInformationByRule[0];
            Assert.IsNotNull(convertedRoomExtSellInfo, string.Format(notNullMsg, "ReservationAdditionalData ExternalSellingInformation"));
            Assert.AreEqual(expectedRoomExtSellInfo.ReservationRoomsExtrasSum, convertedRoomExtSellInfo.ReservationRoomsExtrasSum, string.Format(areEqMsg, "AdditionalData ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedRoomExtSellInfo.ReservationRoomsPriceSum, convertedRoomExtSellInfo.ReservationRoomsPriceSum, string.Format(areEqMsg, "AdditionalData ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedRoomExtSellInfo.ReservationRoomsTotalAmount, convertedRoomExtSellInfo.ReservationRoomsTotalAmount, string.Format(areEqMsg, "AdditionalData ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedRoomExtSellInfo.TotalTax, convertedRoomExtSellInfo.TotalTax, string.Format(areEqMsg, "AdditionalData TotalTax"));

            // Assert Additional Data BigPullUID
            Assert.IsNotNull(convertedReservation.ReservationAdditionalData.BigPullAuthRequestor_UID, string.Format(notNullMsg, "ReservationAdditionalData BigPull_UID"));
            Assert.AreEqual(10, convertedReservation.ReservationAdditionalData.BigPullAuthRequestor_UID);
            var convertedBigPullUid = convertedReservation.ReservationAdditionalData.BigPullAuthRequestor_UID;
            var expectedBigPullUid = expectedReservation.ReservationAdditionalData.BigPullAuthRequestor_UID;
            Assert.IsNotNull(convertedBigPullUid, string.Format(notNullMsg, "ReservationAdditionalData BigPull_UID"));
            Assert.AreEqual(convertedBigPullUid, expectedBigPullUid, string.Format(areEqMsg, "AdditionalData BigPull_UID"));

            // Assert ExternalSellingInformation PricesPerDay
            Assert.IsNotNull(convertedRoomExtSellInfo.PricesPerDay, string.Format(notNullMsg, "ReservationAdditionalData PricesPerDay"));
            Assert.AreEqual(1, convertedRoomExtSellInfo.PricesPerDay.Count);
            Assert.AreEqual(expectedRoomExtSellInfo.PricesPerDay[0].Price, convertedRoomExtSellInfo.PricesPerDay[0].Price, string.Format(areEqMsg, "AdditionalData PricePerDay"));

            // Assert ExternalSellingInformation TaxPolicies
            Assert.IsNotNull(convertedRoomExtSellInfo.TaxPolicies, string.Format(notNullMsg, "ReservationAdditionalData TaxPolicies"));
            Assert.AreEqual(1, convertedRoomExtSellInfo.TaxPolicies.Count);
            Assert.AreEqual(expectedRoomExtSellInfo.TaxPolicies[0].TaxCalculatedValue, convertedRoomExtSellInfo.TaxPolicies[0].TaxCalculatedValue, string.Format(areEqMsg, "AdditionalData TaxCalculatedValue"));
        }

        #region Validate Prices Tests

        #region PULL

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_All_Validations_Price_Without_Version_Pull()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 100 } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 100 } };
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = null
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail();
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_All_Validations_Price_With_Version_But_NotValid_Pull()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 100 } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 100 } };
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail();
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_PerDay_Invalid_Pull()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 100 } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 89 } };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
                Assert.Fail();
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_PerDay_Valid_Pull()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 100M } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 99.99M } };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail();
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_PerDay_Valid_RequestValueIsHigher_Pull()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 100M } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 105M } };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail();
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoom_NotValid_Pull()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 115,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 105,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);
                Assert.Fail();
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoom_Valid_Pull()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 300,
                RoomsTotalAmount = 300,
                TotalAmount = 310,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 300,
                        ReservationRoomsTotalAmount = 300,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 300,
                RoomsTotalAmount = 300,
                TotalAmount = 310,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 299.99M,
                        ReservationRoomsTotalAmount = 299.99M,
                        ReservationRoomsExtrasSum = 9.99M,
                        TotalTax = 9.99M
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail();
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoom_Valid_RequestValueIsHigher_Pull()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 300,
                RoomsTotalAmount = 300,
                TotalAmount = 310,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 300,
                        ReservationRoomsTotalAmount = 300,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 300,
                RoomsTotalAmount = 300,
                TotalAmount = 310,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 350M,
                        ReservationRoomsTotalAmount = 350M,
                        ReservationRoomsExtrasSum = 15M,
                        TotalTax = 15M
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail();
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_Reservation_NotValid_Pull()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 15,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
                Assert.Fail();
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_Reservation_Valid_Pull()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 15,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 14.99M,
                RoomsTax = 14.99M,
                RoomsPriceSum = 99.99M,
                RoomsTotalAmount = 109.99M,
                TotalAmount = 149.99M
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail();
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_Reservation_Valid_RequestValueIsHigher_Pull()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 10,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 120,
                TotalAmount = 120
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 20,
                RoomsTax = 25,
                RoomsPriceSum = 200,
                RoomsTotalAmount = 245,
                TotalAmount = 245
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.Pull,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail();
            }
        }

        #endregion PULL

        #region BE

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_All_Validations_Price_Without_Version_BE()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 100 } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 100 } };
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = null
            };
            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_All_Validations_Price_With_Version_But_NotValid_BE()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 100 } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 100 } };
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_PerDay_Valid_EqualLowerMargin_BE()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 99.99M } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 100 } };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_PerDay_Valid_EqualHigherMargin_BE()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 100.01M } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 100 } };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_PerDay_Valid_BetweenMargins_BE()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 100.001M } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 100 } };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_PerDay_NotValid_GreaterThanHigherMargin_BE()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 101M } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 100 } };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
                Assert.Fail("Prices must be invalid");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_PerDay_NotValid_LowerThanLowerMargin_BE()
        {
            // Build
            var rateRoomDetailCalculated = new List<OBcontractsRates.RateRoomDetailReservation> { new OBcontractsRates.RateRoomDetailReservation { FinalPrice = 99M } };
            var reservationRoomDetails = new List<reservationContracts.ReservationRoomDetail> { new reservationContracts.ReservationRoomDetail { Price = 100 } };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidatePricePerDay(rateRoomDetailCalculated, reservationRoomDetails, failMargin, request);
                Assert.Fail("Prices must be invalid");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoomExtra_Valid_EqualLowerMargin_BE()
        {
            // Build
            decimal propertyBaseCurrencyExchangeRate = 1;
            decimal failMargin = 0.01M;
            var calculatedExtras = new List<OBcontractsRates.Extra>()
            {
                new OBcontractsRates.Extra()
                {
                    UID = 1,
                    Value = 9.99M
                }
            };
            var originalReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
            {
                new reservationContracts.ReservationRoomExtra()
                {
                    Extra_UID = 1,
                    Qty = 1,
                    Total_Price = 10
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomExtras(originalReservationRoomExtras, calculatedExtras, propertyBaseCurrencyExchangeRate, 1, 1, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoomExtra_Valid_EqualHigherMargin_BE()
        {
            // Build
            decimal propertyBaseCurrencyExchangeRate = 1;
            decimal failMargin = 0.01M;
            var calculatedExtras = new List<OBcontractsRates.Extra>()
            {
                new OBcontractsRates.Extra()
                {
                    UID = 1,
                    Value = 10.01M
                }
            };
            var originalReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
            {
                new reservationContracts.ReservationRoomExtra()
                {
                    Extra_UID = 1,
                    Qty = 1,
                    Total_Price = 10
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomExtras(originalReservationRoomExtras, calculatedExtras, propertyBaseCurrencyExchangeRate, 1, 1, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoomExtra_Valid_BetweenMargins_BE()
        {
            // Build
            decimal propertyBaseCurrencyExchangeRate = 1;
            decimal failMargin = 0.01M;
            var calculatedExtras = new List<OBcontractsRates.Extra>()
            {
                new OBcontractsRates.Extra()
                {
                    UID = 1,
                    Value = 10.001M
                }
            };
            var originalReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
            {
                new reservationContracts.ReservationRoomExtra()
                {
                    Extra_UID = 1,
                    Qty = 1,
                    Total_Price = 10
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomExtras(originalReservationRoomExtras, calculatedExtras, propertyBaseCurrencyExchangeRate, 1, 1, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoomExtra_NotValid_GreaterThanHigherMargin_BE()
        {
            // Build
            decimal propertyBaseCurrencyExchangeRate = 1;
            decimal failMargin = 0.01M;
            var calculatedExtras = new List<OBcontractsRates.Extra>()
            {
                new OBcontractsRates.Extra()
                {
                    UID = 1,
                    Value = 11
                }
            };
            var originalReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
            {
                new reservationContracts.ReservationRoomExtra()
                {
                    Extra_UID = 1,
                    Qty = 1,
                    Total_Price = 10
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomExtras(originalReservationRoomExtras, calculatedExtras, propertyBaseCurrencyExchangeRate, 1, 1, failMargin, request);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoomExtra_NotValid_LowerThanLowerMargin_BE()
        {
            // Build
            decimal propertyBaseCurrencyExchangeRate = 1;
            decimal failMargin = 0.01M;
            var calculatedExtras = new List<OBcontractsRates.Extra>()
            {
                new OBcontractsRates.Extra()
                {
                    UID = 1,
                    Value = 9
                }
            };
            var originalReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
            {
                new reservationContracts.ReservationRoomExtra()
                {
                    Extra_UID = 1,
                    Qty = 1,
                    Total_Price = 10
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomExtras(originalReservationRoomExtras, calculatedExtras, propertyBaseCurrencyExchangeRate, 1, 1, failMargin, request);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoomExtra_TwoExtras_OneExtraNotValid_LowerThanLowerMargin_BE()
        {
            // Build
            decimal propertyBaseCurrencyExchangeRate = 1;
            decimal failMargin = 0.01M;
            var calculatedExtras = new List<OBcontractsRates.Extra>()
            {
                new OBcontractsRates.Extra()
                {
                    UID = 1,
                    Value = 10
                },
                new OBcontractsRates.Extra()
                {
                    UID = 2,
                    Value = 15
                }
            };
            var originalReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
            {
                new reservationContracts.ReservationRoomExtra()
                {
                    Extra_UID = 1,
                    Qty = 1,
                    Total_Price = 10
                },
                new reservationContracts.ReservationRoomExtra()
                {
                    Extra_UID = 2,
                    Qty = 1,
                    Total_Price = 14
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomExtras(originalReservationRoomExtras, calculatedExtras, propertyBaseCurrencyExchangeRate, 1, 1, failMargin, request);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoomExtra_TwoExtras_OneReservationRoomExtra_ExtraQty2_Invalid_LowerThanLowerMargin_BE()
        {
            // Build
            decimal propertyBaseCurrencyExchangeRate = 1;
            decimal failMargin = 0.01M;
            var calculatedExtras = new List<OBcontractsRates.Extra>()
            {
                new OBcontractsRates.Extra()
                {
                    UID = 1,
                    Value = 10
                }
            };
            var originalReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
            {
                new reservationContracts.ReservationRoomExtra()
                {
                    UID = 1,
                    Extra_UID = 1,
                    Qty = 2,
                    Total_Price = 10
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomExtras(originalReservationRoomExtras, calculatedExtras, propertyBaseCurrencyExchangeRate, 1, 1, failMargin, request);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoomExtra_TwoExtras_OneReservationRoomExtra_ExtraQty2_Valid_LowerThanLowerMargin_BE()
        {
            // Build
            decimal propertyBaseCurrencyExchangeRate = 1;
            decimal failMargin = 0.01M;
            var calculatedExtras = new List<OBcontractsRates.Extra>()
            {
                new OBcontractsRates.Extra()
                {
                    UID = 1,
                    Value = 10
                }
            };
            var originalReservationRoomExtras = new List<reservationContracts.ReservationRoomExtra>()
            {
                new reservationContracts.ReservationRoomExtra()
                {
                    UID = 1,
                    Extra_UID = 1,
                    Qty = 2,
                    Total_Price = 20
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomExtras(originalReservationRoomExtras, calculatedExtras, propertyBaseCurrencyExchangeRate, 1, 1, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoom_Valid_EqualLowerMargin_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 99.99M,
                TotalAmount = 109.99M,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 99.99M,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 100,
                TotalAmount = 110,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);

            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoom_Valid_EqualHigherMargin_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 100.01M,
                TotalAmount = 110.01M,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100.01M,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 100,
                TotalAmount = 110,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);

            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoom_Valid_BetweenMargins_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 100.001M,
                TotalAmount = 110.001M,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100.001M,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 100,
                TotalAmount = 110,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);

            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoom_NotValid_GreaterThanHigherMargin_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 101,
                TotalAmount = 111,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 101,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 100,
                TotalAmount = 110,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_ReservationRoom_NotValid_LowerThanLowerMargin_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 99,
                TotalAmount = 109,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 99,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 10,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 100,
                TotalAmount = 110,
                ReservationRooms = new List<reservationContracts.ReservationRoom>
                {
                    new reservationContracts.ReservationRoom
                    {
                        ReservationRoomsPriceSum = 100,
                        ReservationRoomsTotalAmount = 100,
                        ReservationRoomsExtrasSum = 10,
                        TotalTax = 10
                    }
                }
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationRoomsPrices(reservationCalculated.ReservationRooms[0], reservation.ReservationRooms[0], failMargin, request);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_Reservation_Valid_EqualLowerMargin_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 14.99M,
                RoomsTax = 14.99M,
                RoomsPriceSum = 99.99M,
                RoomsTotalAmount = 109.99M,
                TotalAmount = 149.99M
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 15,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_Reservation_Valid_EqualHigherMargin_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 15.01M,
                RoomsTax = 15.01M,
                RoomsPriceSum = 100.01M,
                RoomsTotalAmount = 110.01M,
                TotalAmount = 150.01M
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 15,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_Reservation_Valid_BetweenMargins_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 15.001M,
                RoomsTax = 15.001M,
                RoomsPriceSum = 100.001M,
                RoomsTotalAmount = 110.001M,
                TotalAmount = 150.001M
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 15,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
            }
            catch (BusinessLayerException)
            {
                // Asserts
                Assert.Fail("Prices must be valid.");
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_Reservation_NotValid_GreaterThanHigherMargin_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 16,
                RoomsTax = 16,
                RoomsPriceSum = 101,
                RoomsTotalAmount = 111,
                TotalAmount = 151
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 15,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        [TestMethod]
        [TestCategory("ReservationValidatePrices")]
        public void Test_Validation_Price_Reservation_NotValid_LowerThanLowerMargin_BE()
        {
            // Build
            var reservationCalculated = new reservationContracts.Reservation
            {
                TotalTax = 14,
                RoomsTax = 14,
                RoomsPriceSum = 99,
                RoomsTotalAmount = 109,
                TotalAmount = 149
            };
            var reservation = new reservationContracts.Reservation
            {
                TotalTax = 15,
                RoomsTax = 15,
                RoomsPriceSum = 100,
                RoomsTotalAmount = 110,
                TotalAmount = 150
            };
            var request = new OB.Reservation.BL.Contracts.Requests.InsertReservationRequest()
            {
                RuleType = OB.Reservation.BL.Constants.RuleType.BE,
                Version = new Version { Major = 0, Minor = 9, Patch = 45 }
            };

            try
            {
                //Act
                reservationHelperPOCO.ValidateReservationPrices(reservationCalculated, reservation, failMargin, request);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException be)
            {
                // Asserts
                Assert.AreEqual("Reservation Prices Are Invalid", be.Message);
                Assert.AreEqual(-579, be.ErrorCode);
            }
        }

        #endregion BE

        #endregion Validate Prices Tests

        #region TreatPullReservation
        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservationWithRepresentativeAndOperatorRulesPortal()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 114.4480M, 6, 122.4480M, 122.4480M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 114.4480M, 122.4480M, 6, 2, 84.52M, 92.52M, 6, 114.4480M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsRepresentativeAndOperatorRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();
            MockListTaxPoliciesByRateIds(6);

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };

            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            reservationHelperPOCO.TreatPullTpiReservation(parameters);

            string areEqMsg = "'{0}' value is different.";
            //Assert Reservation Values
            Assert.AreEqual(expectedReservation.RoomsExtras, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(expectedReservation.RoomsPriceSum, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.Tax, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(expectedReservation.TotalAmount, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsExtrasSum, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsPriceSum, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsTotalAmount, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().TotalTax, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = parameters.AddicionalData.ExternalSellingReservationInformationByRule;

            decimal expectedREP = 104.4960M;
            decimal expectedChannel = 114.4480M;

            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedChannel + 8, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedChannel + 8, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = parameters.AddicionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedChannel + 8, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedChannel + 8, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservationWithTwoRulesPortalAndOmnibeesRateTypePVP()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 120.75M, 6, 128.75M, 128.75M, 6, 2, 120.75M, 6, 128.75M, 128.75M, 6);
            WithRoom_TreatPull(true, true, true, 2, 120.75M, 128.75M, 6, 2, 120.75M, 128.75M, 6, 120.75M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsRepresentativeAndOperatorRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50, false, false, true);
            MockRateRoomDetailRepository();
            MockListTaxPoliciesByRateIds(6);

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };

            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            reservationHelperPOCO.TreatPullTpiReservation(parameters);

            string areEqMsg = "'{0}' value is different.";
            //Assert Reservation Values
            Assert.AreEqual(expectedReservation.RoomsExtras, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(expectedReservation.RoomsPriceSum, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.Tax, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(expectedReservation.TotalAmount, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsExtrasSum, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsPriceSum, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsTotalAmount, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().TotalTax, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = parameters.AddicionalData.ExternalSellingReservationInformationByRule;
            //REP
            Assert.AreEqual(120.75M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(128.75M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(128.75M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(120.75M, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(128.75M, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(128.75M, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = parameters.AddicionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //REP
            Assert.AreEqual(120.75M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(128.75M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(128.75M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(120.75M, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(128.75M, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(128.75M, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservationWithRepresentativeRulePortal()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 112.1980M, 6, 120.1980M, 120.1980M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 112.1980M, 120.1980M, 6, 2, 84.52M, 92.52M, 6, 112.1980M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsRepresentativeRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();
            MockListTaxPoliciesByRateIds(6);

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            reservationHelperPOCO.TreatPullTpiReservation(parameters);

            string areEqMsg = "'{0}' value is different.";
            //Assert Reservation Values
            Assert.AreEqual(expectedReservation.RoomsExtras, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(expectedReservation.RoomsPriceSum, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.Tax, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(expectedReservation.TotalAmount, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsExtrasSum, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsPriceSum, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsTotalAmount, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().TotalTax, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = parameters.AddicionalData.ExternalSellingReservationInformationByRule;

            decimal expectedREP = 104.4960M;
            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = parameters.AddicionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservationWithOBTpiRulePortal()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 112.1980M, 6, 120.1980M, 120.1980M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 112.1980M, 120.1980M, 6, 2, 84.52M, 92.52M, 6, 112.1980M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsOBTPIRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();
            MockListTaxPoliciesByRateIds(6);

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };

            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            reservationHelperPOCO.TreatPullTpiReservation(parameters);

            string areEqMsg = "'{0}' value is different.";
            //Assert Reservation Values
            Assert.AreEqual(expectedReservation.RoomsExtras, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(expectedReservation.RoomsPriceSum, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.Tax, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(expectedReservation.TotalAmount, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsExtrasSum, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsPriceSum, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsTotalAmount, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().TotalTax, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = parameters.AddicionalData.ExternalSellingReservationInformationByRule;
            //TPI
            Assert.AreEqual(97.1980M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(105.1980M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(105.1980M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = parameters.AddicionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //TPI
            Assert.AreEqual(97.1980M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(105.1980M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(105.1980M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservationWithPOTpiRulePortal()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 112.1980M, 6, 120.1980M, 120.1980M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 112.1980M, 120.1980M, 6, 2, 84.52M, 92.52M, 6, 112.1980M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            inputReservation.ReservationAdditionalData = new reservationContracts.ReservationsAdditionalData() { ExternalTpiId = 126 };

            MockPortalRulesRepository(PORule.IsPOTPIRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();
            MockListTaxPoliciesByRateIds(6);

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            reservationHelperPOCO.TreatPullTpiReservation(parameters);

            string areEqMsg = "'{0}' value is different.";
            //Assert Reservation Values
            Assert.AreEqual(expectedReservation.RoomsExtras, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(expectedReservation.RoomsPriceSum, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.Tax, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(expectedReservation.TotalAmount, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsExtrasSum, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsPriceSum, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsTotalAmount, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().TotalTax, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = parameters.AddicionalData.ExternalSellingReservationInformationByRule;
            //TPI
            Assert.AreEqual(99.7336M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(107.7336M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(107.7336M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = parameters.AddicionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //TPI
            Assert.AreEqual(99.7336M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(107.7336M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(107.7336M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            Assert.AreEqual(126, parameters.AddicionalData.ExternalTpiId);
            Assert.AreEqual(null, parameters.AddicionalData.ExternalChannelId);
            Assert.AreEqual("Empresa A TO Z TRAVEL", parameters.AddicionalData.ExternalName);
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservationWithoutRulePortal()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 84.52M, 6, 92.52M, 92.52M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 84.52M, 92.52M, 6, 2, 84.52M, 92.52M, 6, 84.52M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.None);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            reservationHelperPOCO.TreatPullTpiReservation(parameters);

            string areEqMsg = "'{0}' value is different.";
            //Assert Reservation Values
            Assert.AreEqual(expectedReservation.RoomsExtras, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(expectedReservation.RoomsPriceSum, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.Tax, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(expectedReservation.TotalAmount, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsExtrasSum, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsPriceSum, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsTotalAmount, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().TotalTax, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert External Selling Reservation Information Rules 
            var inputExternalSellingResInfoRule = parameters.AddicionalData.ExternalSellingReservationInformationByRule;
            Assert.AreEqual(null, inputExternalSellingResInfoRule);

            //Assert External Selling Room Information Rules 
            var inputExternalSellingRoomInfoRule = parameters.AddicionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            Assert.AreEqual(null, inputExternalSellingRoomInfoRule);

            Assert.AreEqual(null, parameters.AddicionalData.ExternalTpiId);
            Assert.AreEqual(null, parameters.AddicionalData.ExternalChannelId);
            Assert.AreEqual(string.Empty, parameters.AddicionalData.ExternalName);
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservation_PricePerDay_PullPricesIsBiggerThanCalculatedPrices()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 112.1980M, 6, 120.1980M, 120.1980M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 112.1980M, 120.1980M, 6, 2, 84.52M, 92.52M, 6, 150M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsRepresentativeRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();
            MockListTaxPoliciesByRateIds(6);

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            reservationHelperPOCO.TreatPullTpiReservation(parameters);

            string areEqMsg = "'{0}' value is different.";
            //Assert Reservation Values
            Assert.AreEqual(expectedReservation.RoomsExtras, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(expectedReservation.RoomsPriceSum, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.Tax, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(expectedReservation.TotalAmount, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsExtrasSum, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsPriceSum, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsTotalAmount, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().TotalTax, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = parameters.AddicionalData.ExternalSellingReservationInformationByRule;

            decimal expectedREP = 104.4960M;

            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = parameters.AddicionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservation_Invalid_PricePerDay_PullPricesIsSmallerThanCalculatedPrices()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 112.1980M, 6, 120.1980M, 120.1980M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 112.1980M, 120.1980M, 6, 2, 84.52M, 92.52M, 6, 100M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsRepresentativeRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };

            try
            {
                var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
                reservationHelperPOCO.TreatPullTpiReservation(parameters);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -579);
                Assert.AreEqual(ex.ErrorType, "ReservationPricesAreInvalid");
                Assert.AreEqual(ex.Message, "Reservation Prices Are Invalid");
            }
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservation_Invalid_ReservationRoomsPrices_PullPricesIsSmallerThanCalculatedPrices()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 112.1980M, 6, 120.1980M, 120.1980M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 100.1980M, 120.1980M, 6, 2, 84.52M, 92.52M, 6, 112.1980M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsRepresentativeRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };

            try
            {
                var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
                reservationHelperPOCO.TreatPullTpiReservation(parameters);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -579);
                Assert.AreEqual(ex.ErrorType, "ReservationPricesAreInvalid");
                Assert.AreEqual(ex.Message, "Reservation Prices Are Invalid");
            }
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservation_Invalid_ReservationPrices_PullPricesIsSmallerThanCalculatedPrices()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 90M, 6, 120.1980M, 120.1980M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 112.1980M, 120.1980M, 6, 2, 84.52M, 92.52M, 6, 112.1980M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsRepresentativeRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };

            try
            {
                var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
                reservationHelperPOCO.TreatPullTpiReservation(parameters);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -579);
                Assert.AreEqual(ex.ErrorType, "ReservationPricesAreInvalid");
                Assert.AreEqual(ex.Message, "Reservation Prices Are Invalid");
            }
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservationWithTwoRules_TheOperatorRuleKeepWithPullPrices()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 120M, 6, 128M, 128M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 120M, 128M, 6, 2, 84.52M, 92.52M, 6, 120M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsRepresentativeAndOperatorRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();
            MockListTaxPoliciesByRateIds(6);

            //Call TreatPullTpiReservation
            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            reservationHelperPOCO.TreatPullTpiReservation(parameters);

            string areEqMsg = "'{0}' value is different.";
            //Assert Reservation Values
            Assert.AreEqual(expectedReservation.RoomsExtras, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(expectedReservation.RoomsPriceSum, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.Tax, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(expectedReservation.TotalAmount, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsExtrasSum, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsPriceSum, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsTotalAmount, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().TotalTax, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = parameters.AddicionalData.ExternalSellingReservationInformationByRule;

            decimal expectedREP = 104.4960M;
            decimal expectedChannel = 120M;

            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedChannel + 8, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedChannel + 8, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = parameters.AddicionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //REP
            Assert.AreEqual(expectedREP, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedREP + 8, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            //CHANNEL
            Assert.AreEqual(expectedChannel, inputExternalSellingResInfoRule.Last().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.Last().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(expectedChannel + 8, inputExternalSellingResInfoRule.Last().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedChannel + 8, inputExternalSellingResInfoRule.Last().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
        }

        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservation_ReservationRoomDetails_Required()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(2, 120M, 6, 128M, 128M, 6, 2, 84.52M, 6, 92.52M, 92.52M, 6);
            WithRoom_TreatPull(true, true, true, 2, 120M, 128M, 6, 2, 84.52M, 92.52M, 6, 120M);
            WithPaymentDetail_TreatPull();
            WithPartialPaymentDetails_TreatPull();
            WithAdditionalData_TreatPull();

            MockPortalRulesRepository(PORule.IsRepresentativeAndOperatorRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(true);

            CreateRateRoomDetails(1, 30, 15, 100, 50);
            MockRateRoomDetailRepository();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomExtras = inputReservation.ReservationRooms.Select(x => x.ReservationRoomExtras.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = null,
                ReservationRoomExtras = reservationRoomExtras,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };

            try
            {
                //Act
                // ATENTION: when chanset 81425 were passed to stable, we should call: reservationHelperPOCO.TreatBeReservation(parameters);
                IDbTransaction transaction_Omnibees = null;
                reservationHelperPOCO.TreatPullTpiReservation(parameters);
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -2);
                Assert.AreEqual(ex.ErrorType, nameof(Errors.RequiredParameter));
                Assert.AreEqual(ex.Message, "The parameter 'ReservationRoomDetails' is required");
            }
        }


        [TestMethod]
        [TestCategory("TreatPullReservation")]
        public void Test_TreatPullReservationWithPOTpiRulePortal_OverridePullTaxes()
        {
            // Build
            CreateReservationDataContext_TreatPull();
            CreateReservation_TreatPull(
                inputRoomsExtras: 0,
                inputRoomsPriceSum: 118M,
                inputRoomsTax: 5,
                inputRoomsTotalAmount: 123M,
                inputTotalAmount: 123M,
                inputTotalTax: 5,
                expectRoomsExtras: 0,
                expectRoomsPriceSum: 100M,
                expectRoomsTax: 6,
                expectRoomsTotalAmount: 106M,
                expectTotalAmount: 106M,
                expectTotalTax: 6
                );

            WithRoom_TreatPull(
                withReservationRoomDetails: true,
                withReservationRoomExtras: false,
                withReservationRoomTaxPolicies: true,
                inputReservationRoomsExtrasSum: 0,
                inputReservationRoomsPriceSum: 118M,
                inputReservationRoomsTotalAmount: 123M,
                inputTotalTax: 5,
                expectReservationRoomsExtrasSum: 0,
                expectReservationRoomsPriceSum: 100M,
                expectReservationRoomsTotalAmount: 106M,
                expectTotalTax: 6,
                pricePerDay: 100M
            );

            WithAdditionalData_TreatPull();

            inputReservation.ReservationAdditionalData = new reservationContracts.ReservationsAdditionalData() { ExternalTpiId = 126 };

            MockPortalRulesRepository(PORule.IsPOTPIRule);
            MockSqlManagerRepository();
            MockChildTermsRepo();
            MockIncentives(false);

            CreateRateRoomDetails(1, 0, 0, 100, 0);
            MockRateRoomDetailRepository();
            MockListTaxPoliciesByRateIds(6);

            //Call TreatPullTpiReservation
            var version = new Version { Major = 4, Minor = 0, Patch = 0 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatPullTpiReservationParameters
            {
                AddicionalData = inputReservation.ReservationAdditionalData,
                GroupRule = groupRule,
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = null,
                Rooms = inputReservation.ReservationRooms.ToList(),
                IsInsert = true,
                Version = version,
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.Pull
                }
            };
            var sqlManager = RepositoryFactory.GetSqlManager(Reservation.BL.Constants.OmnibeesConnectionString);
            reservationHelperPOCO.TreatPullTpiReservation(parameters);

            string areEqMsg = "'{0}' value is different.";
            //Assert Reservation Values
            Assert.AreEqual(expectedReservation.RoomsPriceSum, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(expectedReservation.RoomsTax, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(expectedReservation.RoomsTotalAmount, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.TotalAmount, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(expectedReservation.TotalTax, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsExtrasSum, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsPriceSum, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().ReservationRoomsTotalAmount, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(expectedReservation.ReservationRooms.First().TotalTax, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert External Selling Reservation Information Rules Values
            var inputExternalSellingResInfoRule = parameters.AddicionalData.ExternalSellingReservationInformationByRule;
            //TPI
            Assert.AreEqual(118.0M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(124.0M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(124.0M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            //Assert External Selling Room Information Rules Values
            var inputExternalSellingRoomInfoRule = parameters.AddicionalData.ReservationRoomList.First().ExternalSellingInformationByRule;
            //TPI
            Assert.AreEqual(118.0M, inputExternalSellingResInfoRule.First().RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(6, inputExternalSellingResInfoRule.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
            Assert.AreEqual(124.0M, inputExternalSellingResInfoRule.First().TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(124.0M, inputExternalSellingResInfoRule.First().RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));

            Assert.AreEqual(126, parameters.AddicionalData.ExternalTpiId);
            Assert.AreEqual(null, parameters.AddicionalData.ExternalChannelId);
            Assert.AreEqual("Empresa A TO Z TRAVEL", parameters.AddicionalData.ExternalName);
        }
        #endregion

        #region TreatBeReservation

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesPerDay_Valid_EqualLowerMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 100);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(99.99M);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            // Act
            reservationHelperPOCO.TreatBeReservation(parameters);

            string areEqMsg = "'{0}' value is different.";

            //Assert Reservation Values
            Assert.AreEqual(10, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(99.99M, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(115.99M, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(115.99M, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(10, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(99.99M, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(115.99M, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesPerDay_Valid_EqualHigherMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 100);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(100.01M);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            // Act
            reservationHelperPOCO.TreatBeReservation(parameters);

            string areEqMsg = "'{0}' value is different.";

            //Assert Reservation Values
            Assert.AreEqual(10, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(100.01M, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(116.01M, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(116.01M, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(10, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(100.01M, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(116.01M, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesPerDay_Valid_BetweenMargins()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 100);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(100.001M);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            // Act
            reservationHelperPOCO.TreatBeReservation(parameters);

            string areEqMsg = "'{0}' value is different.";

            //Assert Reservation Values
            Assert.AreEqual(10, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(100.001M, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(116.001M, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(116.001M, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(10, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(100.001M, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(116.001M, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesPerDay_NotValid_GreaterThanHigherMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 100);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(101);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            try
            {
                // Act
                reservationHelperPOCO.TreatBeReservation(parameters);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -579);
                Assert.AreEqual(ex.ErrorType, "ReservationPricesAreInvalid");
                Assert.AreEqual(ex.Message, "Reservation Prices Are Invalid");
            }
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesPerDay_NotValid_LowerThanLowerMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 100);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(99);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            try
            {
                // Act
                reservationHelperPOCO.TreatBeReservation(parameters);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -579);
                Assert.AreEqual(ex.ErrorType, "ReservationPricesAreInvalid");
                Assert.AreEqual(ex.Message, "Reservation Prices Are Invalid");
            }
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservationRoom_Valid_EqualLowerMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 99.99M);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(99.99M);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            // Act
            reservationHelperPOCO.TreatBeReservation(parameters);

            string areEqMsg = "'{0}' value is different.";

            //Assert Reservation Values
            Assert.AreEqual(10, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(99.99M, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(115.99M, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(115.99M, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(10, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(99.99M, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(115.99M, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservationRoom_Valid_EqualHigherMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 100.01M);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(100.01M);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            // Act            
            reservationHelperPOCO.TreatBeReservation(parameters);

            string areEqMsg = "'{0}' value is different.";

            //Assert Reservation Values
            Assert.AreEqual(10, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(100.01M, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(116.01M, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(116.01M, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(10, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(100.01M, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(116.01M, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservationRoom_Valid_BetweenMargins()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 100.001M);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(100.001M);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            // Act
            reservationHelperPOCO.TreatBeReservation(parameters);

            string areEqMsg = "'{0}' value is different.";

            //Assert Reservation Values
            Assert.AreEqual(10, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(100.001M, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(116.001M, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(116.001M, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(10, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(100.001M, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(116.001M, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservationRoom_NotValid_GreaterThanHigherMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 101);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(101);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            try
            {
                // Act
                reservationHelperPOCO.TreatBeReservation(parameters);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -579);
                Assert.AreEqual(ex.ErrorType, "ReservationPricesAreInvalid");
                Assert.AreEqual(ex.Message, "Reservation Prices Are Invalid");
            }
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservationRoom_NotValid_LowerThanLowerMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100,
                reservationRoomsTotalAmount: 116,
                totalTax: 6,
                pricePerDay: 99);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(99);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            try
            {
                // Act
                reservationHelperPOCO.TreatBeReservation(parameters);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -579);
                Assert.AreEqual(ex.ErrorType, "ReservationPricesAreInvalid");
                Assert.AreEqual(ex.Message, "Reservation Prices Are Invalid");
            }
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservation_Valid_EqualLowerMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 99.99M,
                reservationRoomsTotalAmount: 115.99M,
                totalTax: 6,
                pricePerDay: 99.99M);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(99.99M);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            // Act
            reservationHelperPOCO.TreatBeReservation(parameters);

            string areEqMsg = "'{0}' value is different.";

            //Assert Reservation Values
            Assert.AreEqual(10, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(99.99M, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(115.99M, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(115.99M, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(10, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(99.99M, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(115.99M, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservation_Valid_EqualHigherMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100.01M,
                reservationRoomsTotalAmount: 116.01M,
                totalTax: 6,
                pricePerDay: 100.01M);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(100.01M);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            // Act
            reservationHelperPOCO.TreatBeReservation(parameters);

            string areEqMsg = "'{0}' value is different.";

            //Assert Reservation Values
            Assert.AreEqual(10, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(100.01M, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(116.01M, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(116.01M, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(10, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(100.01M, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(116.01M, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservation_Valid_BetweenMargins()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 100.001M,
                reservationRoomsTotalAmount: 116.001M,
                totalTax: 6,
                pricePerDay: 100.001M);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(100.001M);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            // Act
            reservationHelperPOCO.TreatBeReservation(parameters);

            string areEqMsg = "'{0}' value is different.";

            //Assert Reservation Values
            Assert.AreEqual(10, parameters.Reservation.RoomsExtras, string.Format(areEqMsg, "RoomsExtras"));
            Assert.AreEqual(100.001M, parameters.Reservation.RoomsPriceSum, string.Format(areEqMsg, "RoomsPriceSum"));
            Assert.AreEqual(6, parameters.Reservation.RoomsTax, string.Format(areEqMsg, "RoomsTax"));
            Assert.AreEqual(116.001M, parameters.Reservation.RoomsTotalAmount, string.Format(areEqMsg, "RoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.Tax, string.Format(areEqMsg, "Tax"));
            Assert.AreEqual(116.001M, parameters.Reservation.TotalAmount, string.Format(areEqMsg, "TotalAmount"));
            Assert.AreEqual(6, parameters.Reservation.TotalTax, string.Format(areEqMsg, "TotalTax"));

            //Assert Reservation Rooms Values
            Assert.AreEqual(10, parameters.Rooms.First().ReservationRoomsExtrasSum, string.Format(areEqMsg, "ReservationRoomsExtrasSum"));
            Assert.AreEqual(100.001M, parameters.Rooms.First().ReservationRoomsPriceSum, string.Format(areEqMsg, "ReservationRoomsPriceSum"));
            Assert.AreEqual(116.001M, parameters.Rooms.First().ReservationRoomsTotalAmount, string.Format(areEqMsg, "ReservationRoomsTotalAmount"));
            Assert.AreEqual(6, parameters.Rooms.First().TotalTax, string.Format(areEqMsg, "TotalTax"));
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservation_NotValid_GreaterThanHigherMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 101,
                reservationRoomsTotalAmount: 117,
                totalTax: 6,
                pricePerDay: 101);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(101);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            try
            {
                // Act
                reservationHelperPOCO.TreatBeReservation(parameters);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -579);
                Assert.AreEqual(ex.ErrorType, "ReservationPricesAreInvalid");
                Assert.AreEqual(ex.Message, "Reservation Prices Are Invalid");
            }
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_PricesReservation_NotValid_LowerThanLowerMargin()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 99,
                reservationRoomsTotalAmount: 115,
                totalTax: 6,
                pricePerDay: 99);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(99);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var reservationRoomDetails = inputReservation.ReservationRooms.Select(x => x.ReservationRoomDetails.First()).ToList();
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = reservationRoomDetails,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            try
            {
                // Act
                reservationHelperPOCO.TreatBeReservation(parameters);
                Assert.Fail("Prices must be invalid.");
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -579);
                Assert.AreEqual(ex.ErrorType, "ReservationPricesAreInvalid");
                Assert.AreEqual(ex.Message, "Reservation Prices Are Invalid");
            }
        }

        [TestMethod]
        [TestCategory("TreatBeReservation")]
        public void Test_TreatBeReservation_ReservationRoomDetails_Required()
        {
            // Build
            CreateReservationDataContext_TreatBe();
            CreateReservation_TreatBe(
                roomsExtras: 10,
                roomsPriceSum: 100,
                roomsTax: 6,
                roomsTotalAmount: 116,
                totalAmount: 116,
                totalTax: 6);
            WithRoom_TreatBe(
                withReservationRoomDetails: true,
                withReservationRoomExtras: true,
                withReservationRoomTaxPolicies: true,
                reservationRoomsExtrasSum: 10,
                reservationRoomsPriceSum: 99,
                reservationRoomsTotalAmount: 115,
                totalTax: 6,
                pricePerDay: 99);
            WithPaymentDetail_TreatBe();
            WithPartialPaymentDetails_TreatBe();

            MockListTaxPoliciesByRateIds(6);
            MockListRatesExtras(10);
            MockCalculateReservationRoomPrices(99);
            MockAppSettings();

            var version = new Version { Major = 0, Minor = 9, Patch = 45 };
            var parameters = new TreatBEReservationParameters
            {
                Guest = OtherConverter.Convert(inputReservation.Guest),
                Reservation = inputReservation,
                ReservationContext = inputReservationDataContext,
                ReservationRoomChilds = null,
                ReservationRoomDetails = null,
                ReservationRoomExtras = inputReservation.ReservationRooms.SelectMany(x => x.ReservationRoomExtras).ToList(),
                Rooms = inputReservation.ReservationRooms.ToList(),
                GroupRule = new GroupRule() { RuleType = RuleType.BE, BusinessRules = BusinessRules.BEReservationCalculation },
                ReservationRequest = new Reservation.BL.Contracts.Requests.InsertReservationRequest()
                {
                    Version = version,
                    RuleType = Reservation.BL.Constants.RuleType.BE
                }
            };

            try
            {
                // Act
                // ATENTION: when chanset 81425 were passed to stable, we should call: reservationHelperPOCO.TreatBeReservation(parameters);
                IDbTransaction transaction_Omnibees = null;
                reservationHelperPOCO.TreatBeReservation(parameters);
            }
            catch (BusinessLayerException ex)
            {
                Assert.AreEqual(ex.ErrorCode, -2);
                Assert.AreEqual(ex.ErrorType, nameof(Errors.RequiredParameter));
                Assert.AreEqual(ex.Message, "The parameter 'ReservationRoomDetails' is required");
            }
        }

        #endregion TreatBeReservation
    }
}