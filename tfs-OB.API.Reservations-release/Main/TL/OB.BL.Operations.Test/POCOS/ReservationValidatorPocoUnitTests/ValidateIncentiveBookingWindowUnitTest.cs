using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Internal.BusinessObjects;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Repositories.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Cached;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using contractsReservation = OB.Reservation.BL.Contracts.Data.Reservations;
using domainReservations = OB.Domain.Reservations;
using OB.BL.Operations.Extensions;
using OB.BL.Contracts.Data.Properties;
using OB.Reservation.BL.Contracts.Responses;
using OB.BL.Operations.Exceptions;
using OB.BL.Contracts.Requests;

namespace OB.BL.Operations.Test.ReservationValidator
{
    [TestClass]
    public partial class ValidateIncentiveBookingWindowUnitTest : UnitBaseTest
    {
        public IReservationValidatorPOCO ReservationValidatorPOCO { get; set; }
        public IReservationManagerPOCO ReservationManagerPOCO { get; set; }

        #region Variables

        private Dictionary<long, IEnumerable<Incentive>> _incentivesInDataBase = null;

        #endregion

        private Mock<IOBIncentiveRepository> _incentivesRepoMock = null;
        private Mock<IGroupRulesRepository> _groupRulesRepoMock = null;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            #region initialize data bases mock

            _incentivesInDataBase = new Dictionary<long, IEnumerable<Incentive>>();

            FillIncentivesForTest();
            FillGroupRulesMock();
            #endregion

            //Mock Repository factory to return mocked RateRepository
            InitializeMocks(RepositoryFactoryMock);

            SessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(UnitOfWorkMock.Object);
            SessionFactoryMock.Setup(x => x.GetUnitOfWork(It.IsAny<DomainScope>(), It.IsAny<DomainScope>())).Returns(UnitOfWorkMock.Object);

            UnitOfWorkMock.Setup(x => x.BeginTransaction(It.IsAny<DomainScope>(), It.IsAny<IsolationLevel>()))
                .Returns(new Mock<IDbTransaction>(MockBehavior.Default).Object);

            #region Container
            this.Container = this.Container.RegisterInstance<IGroupRulesRepository>(_groupRulesRepoMock.Object, new TransientLifetimeManager());
            this.Container = this.Container.RegisterInstance<IOBIncentiveRepository>(_incentivesRepoMock.Object, new TransientLifetimeManager());
            #endregion Container

            this.ReservationValidatorPOCO = this.Container.Resolve<IReservationValidatorPOCO>();
            this.ReservationManagerPOCO = this.Container.Resolve<IReservationManagerPOCO>();


            MockListIncentivesWithBookingAndStayPeriodsForReservationRoom();
        }

        private void InitializeMocks(Mock<IRepositoryFactory> repoFactoryMock)
        {
            _groupRulesRepoMock = new Mock<IGroupRulesRepository>(MockBehavior.Default);
            _incentivesRepoMock = new Mock<IOBIncentiveRepository>(MockBehavior.Default);

            repoFactoryMock.Setup(x => x.GetGroupRulesRepository(It.IsAny<IUnitOfWork>()))
                .Returns(_groupRulesRepoMock.Object);

            repoFactoryMock.Setup(x => x.GetOBIncentiveRepository())
                .Returns(_incentivesRepoMock.Object);
        }

        private void MockListIncentivesWithBookingAndStayPeriodsForReservationRoom()
        {
            _incentivesRepoMock.Setup(x => x.ListIncentivesWithBookingAndStayPeriodsForReservationRoom(It.IsAny<ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest>()))
                .Returns(new Contracts.Responses.ListIncentivesWithBookingAndStayPeriodsForReservationRoomResponse { 
                    Status = Contracts.Responses.Status.Success,
                    Result =_incentivesInDataBase
                });
        }

        private void FillIncentivesForTest()
        {
            _incentivesInDataBase.Add(1, new List<Incentive> {
                    new Incentive
                    {
                        UID = 1,
                        DayDiscount = new List<decimal> { 12M, 34M },
                        Days = 2,
                        DiscountPercentage = 20,
                        Property_UID = 1,
                        Rate_UID = 1,
                        IsLastMinuteInHours = true,
                        IncentiveFrom = DateTime.UtcNow,
                        IncentiveTo = DateTime.UtcNow.AddDays(4),
                        IncentiveType = Constants.IncentiveType.LastMinuteBooking,
                        IncentiveType_UID = 2,
                        IsCumulative = true
                    }
                }
            );

            _incentivesInDataBase.Add(2, new List<Incentive> {
                    new Incentive
                    {
                        UID = 2,
                        DayDiscount = new List<decimal> { 12M, 34M },
                        Property_UID = 1,
                        Rate_UID = 1,
                        IsLastMinuteInHours = true,
                        IncentiveFrom = DateTime.UtcNow,
                        IncentiveTo = DateTime.UtcNow.AddDays(4),
                        IncentiveType = Constants.IncentiveType.LastMinuteBooking,
                        IncentiveType_UID = 2,
                        FreeDays = 2,
                        IsFreeDaysAtBegin = true,
                        IsCumulative = true
                    }
                }
);
        }


        [TestMethod]
        [TestCategory("ValidateIncentiveBookingWindow")]
        public void Test_ValidateIncentiveBookingWindow_Without_FlagValidateBookingWindow_Success()
        {
            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.Pull);
            groupRule.BusinessRules &= ~BusinessRules.ValidateBookingWindow;

            var request = new InitialValidationRequest
            {
                GroupRule = groupRule,
                RequestId = Guid.NewGuid().ToString()
            };

            //act
            var exception = ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request);

            //assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        [TestCategory("ValidateIncentiveBookingWindow")]
        public void Test_ValidateIncentiveBookingWindow_Without_ReservationRooms_Fail()
        {
            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.Pull);
            var exceptionExpected = Errors.ReservationError.ToBusinessLayerException();

            var request = new InitialValidationRequest
            {
                GroupRule = groupRule,
                RequestId = Guid.NewGuid().ToString()
            };

            //act
            var exception = ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request);

            //assert
            Assert.AreEqual(exceptionExpected.ErrorType, exception.ErrorType);
            Assert.AreEqual(exceptionExpected.ErrorCode, exception.ErrorCode);
            Assert.AreEqual(exceptionExpected.Message, exception.Message);
            Assert.ThrowsException<BusinessLayerException>(() => ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request).ValidateToThrowException());
        }

        [TestMethod]
        [TestCategory("ValidateIncentiveBookingWindow")]
        public void Test_ValidateIncentiveBookingWindow_Without_ReservationRoomDetails_Success()
        {
            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.Pull);

            var request = new InitialValidationRequest
            {
                ReservationRooms = new List<contractsReservation.ReservationRoom>
                {
                    new contractsReservation.ReservationRoom
                    {
                        DateFrom = DateTime.UtcNow,
                        DateTo = DateTime.UtcNow.AddDays(4),
                        Rate_UID = 1
                    }
                },
                GroupRule = groupRule,
                RequestId = Guid.NewGuid().ToString()
            };

            //act
            var exception = ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request);

            //assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        [TestCategory("ValidateIncentiveBookingWindow")]
        public void Test_ValidateIncentiveBookingWindow_Without_AppliedIncentives_Success()
        {
            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.Pull);

            var request = new InitialValidationRequest
            {
                ReservationRooms = new List<contractsReservation.ReservationRoom>
                {
                    new contractsReservation.ReservationRoom
                    {
                        DateFrom = DateTime.UtcNow,
                        DateTo = DateTime.UtcNow.AddDays(4),
                        Rate_UID = 1
                    }
                },
                ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail> {
                    new contractsReservation.ReservationRoomDetail {
                        ReservationRoomDetailsAppliedIncentives = null
                    }
                },
                GroupRule = groupRule,
                RequestId = Guid.NewGuid().ToString()
            };

            //act
            var exception = ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request);

            //assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        [TestCategory("ValidateIncentiveBookingWindow")]
        public void Test_ValidateIncentiveBookingWindow_WithAppliedIncentives_IncentiveBookingWindowPeriods_Fail()
        {
            var exceptionExpected = Errors.IncentivesAppliedError.ToBusinessLayerException();
            var reservationRoomRequest = new List<contractsReservation.ReservationRoom>{
                    new contractsReservation.ReservationRoom{
                        DateFrom = DateTime.UtcNow,
                        DateTo = DateTime.UtcNow.AddDays(4),
                        Rate_UID = 1,
                        ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail> {
                            new contractsReservation.ReservationRoomDetail {
                                ReservationRoomDetailsAppliedIncentives = new List<contractsReservation.ReservationRoomDetailsAppliedIncentive> {
                                    new contractsReservation.ReservationRoomDetailsAppliedIncentive {
                                        Incentive_UID = 1
                                    },
                                    new contractsReservation.ReservationRoomDetailsAppliedIncentive {
                                        Incentive_UID = 2
                                    }
                                }
                            }
                        }
                    }
                };

            //mock
            _incentivesRepoMock.Setup(x => x.ListIncentivesWithBookingAndStayPeriodsForReservationRoom(It.IsAny<ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest>()))
                .Returns(new Contracts.Responses.ListIncentivesWithBookingAndStayPeriodsForReservationRoomResponse { Status = Contracts.Responses.Status.Success });

            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.Pull);
            var request = new InitialValidationRequest
            {
                ReservationRooms = reservationRoomRequest,
                ReservationRoomDetails = reservationRoomRequest.SelectMany(x =>x.ReservationRoomDetails).ToList(),
                GroupRule = groupRule,
                RequestId = Guid.NewGuid().ToString()
            };

            //act
            var exception = ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request);

            //assert
            Assert.AreEqual(exceptionExpected.ErrorType, exception.ErrorType);
            Assert.AreEqual(exceptionExpected.ErrorCode, exception.ErrorCode);
            Assert.AreEqual(exceptionExpected.Message, exception.Message);
            Assert.ThrowsException<BusinessLayerException>(() => ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request).ValidateToThrowException());
        }

        [TestMethod]
        [TestCategory("ValidateIncentiveBookingWindow")]
        public void Test_ValidateIncentiveBookingWindow_ListIncentivesWithBookingAndStayPeriodsForReservationRoom_Fail()
        {
            var exceptionExpected = Errors.IncentivesAppliedError.ToBusinessLayerException();
            var reservationRoomRequest = new List<contractsReservation.ReservationRoom>{
                    new contractsReservation.ReservationRoom{
                        DateFrom = DateTime.UtcNow,
                        DateTo = DateTime.UtcNow.AddDays(4),
                        Rate_UID = 1,
                        ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail> {
                            new contractsReservation.ReservationRoomDetail {
                                ReservationRoomDetailsAppliedIncentives = new List<contractsReservation.ReservationRoomDetailsAppliedIncentive> {
                                    new contractsReservation.ReservationRoomDetailsAppliedIncentive {
                                        Incentive_UID = 1
                                    },
                                    new contractsReservation.ReservationRoomDetailsAppliedIncentive {
                                        Incentive_UID = 2
                                    }
                                }
                            }
                        }
                    }
                };

            //mock
            _incentivesRepoMock.Setup(x => x.ListIncentivesWithBookingAndStayPeriodsForReservationRoom(It.IsAny<ListIncentivesWithBookingAndStayPeriodsForReservationRoomRequest>()))
                .Returns(new Contracts.Responses.ListIncentivesWithBookingAndStayPeriodsForReservationRoomResponse { Status = Contracts.Responses.Status.Fail });

            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.Pull);
            var request = new InitialValidationRequest
            {
                ReservationRooms = reservationRoomRequest,
                ReservationRoomDetails = reservationRoomRequest.SelectMany(x => x.ReservationRoomDetails).ToList(),
                GroupRule = groupRule,
                RequestId = Guid.NewGuid().ToString()
            };

            //act
            var exception = ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request);

            //assert
            Assert.AreEqual(exceptionExpected.ErrorType, exception.ErrorType);
            Assert.AreEqual(exceptionExpected.ErrorCode, exception.ErrorCode);
            Assert.AreEqual(exceptionExpected.Message, exception.Message);
            Assert.ThrowsException<BusinessLayerException>(() => ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request).ValidateToThrowException());
        }

        [TestMethod]
        [TestCategory("ValidateIncentiveBookingWindow")]
        public void Test_ValidateIncentiveBookingWindow_AppliedIncentives_NoMatch_WithIncentiveBookingWindowPeriodsDefined_Fail()
        {
            var exceptionExpected = Errors.IncentivesAppliedError.ToBusinessLayerException();
            var reservationRoomRequest = new List<contractsReservation.ReservationRoom>{
                    new contractsReservation.ReservationRoom{
                        DateFrom = DateTime.UtcNow,
                        DateTo = DateTime.UtcNow.AddDays(4),
                        Rate_UID = 1,
                        ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail> {
                            new contractsReservation.ReservationRoomDetail {
                                ReservationRoomDetailsAppliedIncentives = new List<contractsReservation.ReservationRoomDetailsAppliedIncentive> {
                                    new contractsReservation.ReservationRoomDetailsAppliedIncentive {
                                        Incentive_UID = 5
                                    },
                                    new contractsReservation.ReservationRoomDetailsAppliedIncentive {
                                        Incentive_UID = 7
                                    }
                                }
                            }
                        }
                    }
                };

            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.Pull);
            var request = new InitialValidationRequest
            {
                ReservationRooms = reservationRoomRequest,
                ReservationRoomDetails = reservationRoomRequest.SelectMany(x => x.ReservationRoomDetails).ToList(),
                GroupRule = groupRule,
                RequestId = Guid.NewGuid().ToString()
            };

            //act
            var exception = ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request);

            //assert
            Assert.AreEqual(exceptionExpected.ErrorType, exception.ErrorType);
            Assert.AreEqual(exceptionExpected.ErrorCode, exception.ErrorCode);
            Assert.AreEqual(exceptionExpected.Message, exception.Message);
            Assert.ThrowsException<BusinessLayerException>(() => ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request).ValidateToThrowException());
        }

        [TestMethod]
        [TestCategory("ValidateIncentiveBookingWindow")]
        public void Test_ValidateIncentiveBookingWindow_AppliedIncentives_Match_WithIncentiveBookingWindowPeriodsDefined_Success()
        {
            var reservationRoomRequest = new List<contractsReservation.ReservationRoom>{
                    new contractsReservation.ReservationRoom{
                        DateFrom = DateTime.UtcNow,
                        DateTo = DateTime.UtcNow.AddDays(4),
                        Rate_UID = 1,
                        ReservationRoomDetails = new List<contractsReservation.ReservationRoomDetail> {
                            new contractsReservation.ReservationRoomDetail {
                                ReservationRoomDetailsAppliedIncentives = new List<contractsReservation.ReservationRoomDetailsAppliedIncentive> {
                                    new contractsReservation.ReservationRoomDetailsAppliedIncentive {
                                        Incentive_UID = 1
                                    },
                                    new contractsReservation.ReservationRoomDetailsAppliedIncentive {
                                        Incentive_UID = 2
                                    }
                                }
                            }
                        }
                    }
                };

            var groupRule = groupRuleList.FirstOrDefault(x => x.RuleType == domainReservations.RuleType.Pull);
            var request = new InitialValidationRequest
            {
                ReservationRooms = reservationRoomRequest,
                GroupRule = groupRule,
                RequestId = Guid.NewGuid().ToString()
            };

            //act
            var exception = ReservationValidatorPOCO.ValidateIncentiveBookingWindow(request);

            //assert
            Assert.IsNull(exception);
        }
    }
}
