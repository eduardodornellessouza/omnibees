using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.Api.Core;
using OB.BL.Contracts.Data.PMS;
using OB.BL.Contracts.Data.Properties;
using OB.BL.Contracts.Data.Reservations;
using OB.BL.Contracts.Requests;
using OB.BL.Contracts.Responses;
using OB.BL.Operations.Helper;
using OB.BL.Operations.Impl;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Entity;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OB.Domain.Reservations;
using PaymentGatewaysLibrary.AdyenService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using domainReservation = OB.Domain.Reservations;
using Status = OB.Reservation.BL.Contracts.Responses.Status;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class ReservationManagerPOCOModifyTest : UnitBaseTest
    {
        IReservationManagerPOCO ReservationPOCO
        {
            get;
            set;
        }

        Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
        Mock<IDbTransaction> dbTransactionMock = new Mock<IDbTransaction>();
        Mock<IOBPMSRepository> obpmsRepo = new Mock<IOBPMSRepository>();
        Mock<IOBPropertyRepository> propertyRepo = new Mock<IOBPropertyRepository>();
        Mock<IReservationRoomRepository> reservationRoomRepo = new Mock<IReservationRoomRepository>();
        Mock<IReservationsRepository> reservationsRepo = new Mock<IReservationsRepository>();
        Mock<IEventSystemManagerPOCO> eventSystemManager = new Mock<IEventSystemManagerPOCO>();
        Mock<IOBRateRepository> obRateRepo = new Mock<IOBRateRepository>();
        Mock<IOBPropertyEventsRepository> obPropertyEventRepo = new Mock<IOBPropertyEventsRepository>();
        Mock<IVisualStateRepository> visualStateRepoMock = new Mock<IVisualStateRepository>();
        Mock<IReservationsFilterRepository> reservationsFilterRepoMock = new Mock<IReservationsFilterRepository>();
        Mock<IOBCRMRepository> obCrmRepoMock = new Mock<IOBCRMRepository>();
        Mock<IReservationHistoryRepository> reservationHistoryRepo = new Mock<IReservationHistoryRepository>();
        Mock<IOBChannelRepository> obChannelRepoMock = new Mock<IOBChannelRepository>();
        Mock<IOBCurrencyRepository> obCurrencyRepoMock = new Mock<IOBCurrencyRepository>();

        IList<PropertyLight> propertyLightDatabase = new List<PropertyLight>
        {
            new PropertyLight
            {
                UID = 1,
                Client_UID = 1
            },
            new PropertyLight
            {
                UID = 2,
                Client_UID = 1
            },
            new PropertyLight
            {
                UID = 3,
                Client_UID = 2
            }
        };

        IList<domainReservation.ReservationRoom> reservationRoomsInDatabase = new List<domainReservation.ReservationRoom>
        {
            new domainReservation.ReservationRoom
            {
                UID = 1,
                Reservation_UID = 1,
                DateFrom = DateTime.Now.AddDays(-1),
                Status = 0,
            },
            new domainReservation.ReservationRoom
            {
                UID = 2,
                Reservation_UID = 1,
                DateFrom = DateTime.Now.AddDays(-2),
                Status = 1,
            },
            new domainReservation.ReservationRoom
            {
                UID = 3,
                Reservation_UID = 2,
                DateFrom = DateTime.Now.AddDays(-1),
                Status = 2,
            },
        };

        IList<domainReservation.Reservation> reservationsInDatabase = new List<domainReservation.Reservation>
        {
            new domainReservation.Reservation
            {
                UID = 1,
                Property_UID = 1,
                Status = 0,
            },
            new domainReservation.Reservation
            {
                UID = 2,
                Property_UID = 1,
                Status = 1,
            },
            new domainReservation.Reservation
            {
                UID = 3,
                Property_UID = 2,
                Status = 3,
            }
        };

        IList<PMSServicesPropertyMapping> pmsServicePropertyMappingInDatabase = new List<PMSServicesPropertyMapping>
        {
            new PMSServicesPropertyMapping
            {
                Property_UID = 1,
                PMSService_UID = 1,
            },
            new PMSServicesPropertyMapping
            {
                Property_UID = 2,
                PMSService_UID = 1,
            },
            new PMSServicesPropertyMapping
            {
                Property_UID = 3,
                PMSService_UID = 2,
            }
        };

        IList<PMSService> pmsServiceInDatabase = new List<PMSService>
        {
            new PMSService
            {
                UID = 1,
                PMS_UID = 1
            },
        };

        IList<Contracts.Data.Channels.ChannelLight> channelsLightInDatabase = new List<Contracts.Data.Channels.ChannelLight>
        {
            new Contracts.Data.Channels.ChannelLight
            {
                UID = 1,
                Name = "Booking Engine"
            }
        };

        IList<Contracts.Data.Channels.Channel> channelsInDatabase = new List<Contracts.Data.Channels.Channel>
        {
            new Contracts.Data.Channels.Channel
            {
                Id = 1,
                Name = "Booking Engine"
            }
        };

        IList<Contracts.Data.General.Currency> currenciesInDataBase = new List<Contracts.Data.General.Currency>
        {
            new Contracts.Data.General.Currency
            {
                UID = 16,
                Name = "Brasil Real"
            }
        };

        IList<OB.BL.Contracts.Data.PMS.PMSReservationsHistory> pmsReservationsHistoryInDatabase = new List<OB.BL.Contracts.Data.PMS.PMSReservationsHistory>();

        // Base Reservation to use on ModifyReservationBackgroundOperations Tests
        private static Reservation.BL.Contracts.Data.Reservations.Reservation BaseReservation = new Reservation.BL.Contracts.Data.Reservations.Reservation
        {
            ReservationRooms = new List<OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoom>
                {
                    new OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoom
                    {
                        RoomType_UID = 1,
                        ReservationRoomDetails = new List<OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail>
                        {
                            new OB.Reservation.BL.Contracts.Data.Reservations.ReservationRoomDetail
                            {

                            }
                        },
                        Status = (int?)Constants.ReservationStatus.Pending,
                        ReservationRoomNo = "RES0000001-1/1"
                    }
                },
            Status = (long)Constants.ReservationStatus.Pending,
            Channel_UID = 1,
            Property_UID = 1,
            Number = "RES0000001-1",
            UID = 1
        };

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();


            #region Mocks
            var sessionFactoryMock = new Mock<ISessionFactory>();
            this.Container = this.Container.RegisterInstance<ISessionFactory>(sessionFactoryMock.Object);

            sessionFactoryMock.Setup(x => x.GetUnitOfWork()).Returns(unitOfWorkMock.Object);
            sessionFactoryMock.Setup(x => x.GetUnitOfWork(It.IsAny<DomainScope>())).Returns(unitOfWorkMock.Object);
            sessionFactoryMock.Setup(x => x.GetUnitOfWork(It.IsAny<DomainScope>())).Returns(unitOfWorkMock.Object);

            //Mock DbTranbcaction
            unitOfWorkMock.Setup(x => x.BeginTransaction(It.IsAny<DomainScope>(), It.IsAny<IsolationLevel>()))
                .Returns(dbTransactionMock.Object);

            this.Container = this.Container.RegisterInstance<IRepositoryFactory>(RepositoryFactoryMock.Object);
            this.Container.RegisterInstance<IEventSystemManagerPOCO>(eventSystemManager.Object);

            RepositoryFactoryMock.Setup(x => x.GetOBPMSRepository()).Returns(obpmsRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBPropertyRepository()).Returns(propertyRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetReservationRoomRepository(It.IsAny<IUnitOfWork>())).Returns(reservationRoomRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetReservationsRepository(It.IsAny<IUnitOfWork>())).Returns(reservationsRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBRateRepository()).Returns(obRateRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBPropertyEventsRepository()).Returns(obPropertyEventRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetVisualStateRepository(It.IsAny<IUnitOfWork>())).Returns(visualStateRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetReservationsFilterRepository(It.IsAny<IUnitOfWork>())).Returns(reservationsFilterRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBCRMRepository()).Returns(obCrmRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetReservationHistoryRepository(It.IsAny<IUnitOfWork>())).Returns(reservationHistoryRepo.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBChannelRepository()).Returns(obChannelRepoMock.Object);
            RepositoryFactoryMock.Setup(x => x.GetOBCurrencyRepository()).Returns(obCurrencyRepoMock.Object);
                
            #endregion

            #region Fill Mocks
            PopulateAndMockOBPMS();
            PopulateAndMockProperty();
            PopulateAndMockReservationRoom();
            PopulateAndMockReservations();
            #endregion

            #region Mock Repo Methods
            MockObRateRepo();
            MockObPropertyEventRepo();
            MockVisualStateRepo();
            MockReservationsFilterRepo();
            MockObCrmRepo();
            MockReservationHistoryRepo();
            MockObChannelRepo();
            MockObCurrencyRepo();
            #endregion

            ReservationPOCO = this.Container.Resolve<IReservationManagerPOCO>();
        }

        #region Fill Mocks
        private void PopulateAndMockOBPMS()
        {
            obpmsRepo.Setup(x => x.ListPMSServicesPropertyMappings(It.IsAny<ListPMSServicesPropertyMappingRequest>())).Returns(
                (ListPMSServicesPropertyMappingRequest request) =>
                {
                    return new ListPMSServicesPropertyMappingResponse
                    {
                        Result = pmsServicePropertyMappingInDatabase.Where(p => request.PropertyUIDs.Contains(p.Property_UID)).ToList(),
                    };
                });

            obpmsRepo.Setup(x => x.ListPMSServices(It.IsAny<ListPMSServiceRequest>())).Returns((ListPMSServiceRequest request) =>
            {
                return new ListPMSServiceResponse
                {
                    Result = pmsServiceInDatabase.Where(p => request.PMSServiceUIDs.Contains(p.UID)).ToList(),
                };
            });
            obpmsRepo.Setup(x => x.InsertPMSReservationsHistory(It.IsAny<InsertPMSReservationsHistoryRequest>())).Returns(
                (InsertPMSReservationsHistoryRequest request) =>
                {
                    foreach (var pmsH in request.PMSReservationsHistories)
                        pmsReservationsHistoryInDatabase.Add(pmsH);

                    return new InsertPMSReservationsHistoryResponse
                    {
                        Status = OB.BL.Contracts.Responses.Status.Success,
                    };
                });
        }

        private void PopulateAndMockProperty()
        {
            propertyRepo.Setup(x => x.ListPropertiesLight(It.IsAny<ListPropertyRequest>())).Returns(
                (ListPropertyRequest request) =>
                {
                    return propertyLightDatabase.Where(p => request.UIDs.Contains(p.UID)).ToList();
                });
        }

        private void PopulateAndMockReservationRoom()
        {
            reservationRoomRepo.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservation.ReservationRoom, bool>>>())).Returns(
                (Expression<Func<domainReservation.ReservationRoom, bool>> expression) =>
                {
                    foreach (var rr in reservationRoomsInDatabase)
                        rr.Reservation = reservationsInDatabase.Where(r => r.UID == rr.Reservation_UID).Single();

                    return reservationRoomsInDatabase.AsQueryable().Where(expression);
                });
        }

        private void PopulateAndMockReservations()
        {
            reservationsRepo.Setup(x => x.GetQuery(It.IsAny<Expression<Func<domainReservation.Reservation, bool>>>())).Returns(
                (Expression<Func<domainReservation.Reservation, bool>> expression) =>
                {
                    return reservationsInDatabase.AsQueryable().Where(expression);
                });
        }
        #endregion


        #region Mock Repo Methods
        private void MockObRateRepo()
        {
            obRateRepo.Setup(x => x.ConnectorEventQueueInsert(It.IsAny<ConnectorEventQueueInsertRequest>()))
                .Returns(0);
        }

        private void MockObPropertyEventRepo()
        {
            obPropertyEventRepo.Setup(x => x.InsertPropertyQueue(It.IsAny<InsertPropertyQueueRequest>()))
                .Returns(0);
        }

        private void MockReservationsFilterRepo()
        {
            visualStateRepoMock.Setup(x => x.GetQuery())
                .Returns(new List<VisualState>().AsQueryable());
        }

        private void MockVisualStateRepo()
        {
            reservationsFilterRepoMock.Setup(x => x.FindByReservationUIDs(It.IsAny<List<long>>()))
                .Returns(new List<ReservationFilter>().AsEnumerable());
        }

        private void MockObCrmRepo()
        {
            obCrmRepoMock.Setup(x => x.ListGuestsByLightCriteria(It.IsAny<ListGuestLightRequest>()))
                .Returns(new List<Contracts.Data.CRM.Guest>());
        }
        private void MockReservationHistoryRepo()
        {
            reservationHistoryRepo.Setup(x => x.Add(It.IsAny<domainReservation.ReservationsHistory>()))
                .Returns((domainReservation.ReservationsHistory entry) =>
                    {
                        return entry;
                    });
        }

        private void MockObChannelRepo()
        {
            obChannelRepoMock.Setup(x => x.ListChannelLight(It.IsAny<ListChannelLightRequest>()))
                .Returns((ListChannelLightRequest request) =>
                {
                    return channelsLightInDatabase.Where(x => request.ChannelUIDs.Contains(x.UID)).ToList();
                });

            obChannelRepoMock.Setup(x => x.ListChannel(It.IsAny<ListChannelRequest>()))
                .Returns((ListChannelRequest request) =>
                {
                    return channelsInDatabase.Where(x => request.ChannelUIDs.Contains(x.Id)).ToList();
                });
        }

        private void MockObCurrencyRepo()
        {
            obCurrencyRepoMock.Setup(x => x.ListCurrencies(It.IsAny<ListCurrencyRequest>()))
                .Returns((ListCurrencyRequest request) =>
                {
                    return currenciesInDataBase.Where(x => request.UIDs.Contains(x.UID)).ToList();
                });
        }
        #endregion


        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber_Event")]
        public void TestUpdatePMSReservationNumber_SendEvent_RRYes_RREqual_RNo()
        {
            bool rrDirty = true;
            bool rDirty = false;
            long pmsId = 5;

            List<OB.Domain.Reservations.Reservation> reservations = new List<domainReservation.Reservation>()
            {
                new domainReservation.Reservation()
                {
                    UID = 5555,
                    Property_UID = 1111,
                    Channel_UID = 10,
                    Number = "RES01",
                    PmsRservationNumber = "RRR",
                    Status = 1,
                    ReservationRooms = new List<OB.Domain.Reservations.ReservationRoom>()
                    {
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/1",
                            PmsRservationNumber = "AAA",
                            Status = 1,
                        },
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/2",
                            PmsRservationNumber = "AAA",
                            Status = 1,
                        }
                    }
                }

            };

            ReservationManagerPOCO reservationManagerPOCO = Container.Resolve<ReservationManagerPOCO>();

            eventSystemManager.Setup(x => x.SendMessage(It.IsAny<Events.Contracts.ICustomNotification>()))
                .Callback((Events.Contracts.ICustomNotification notification) =>
                {

                    Assert.IsNotNull(notification);
                    var pmsNumberSaveNotification = (Events.Contracts.Messages.PmsNumberSaveNotification)notification;

                    Assert.IsNotNull(pmsNumberSaveNotification.Deltas);
                    Assert.IsTrue(pmsNumberSaveNotification.Deltas.Any());

                    var pmsNumberSave = (Events.Contracts.EntityDeltaBase<Events.Contracts.Entities.Pms.PmsNumberSave>)pmsNumberSaveNotification.Deltas.FirstOrDefault();

                    Assert.IsNotNull(pmsNumberSave);
                    Assert.IsNotNull(pmsNumberSave.Entity);
                    Assert.AreEqual(10, pmsNumberSave.Entity.ChannelId);
                    Assert.AreEqual(false, pmsNumberSave.Entity.IsCancellation);
                    Assert.AreEqual(5, pmsNumberSave.Entity.PmsId);
                    Assert.AreEqual("AAA", pmsNumberSave.Entity.PmsReservationNumber);
                    Assert.AreEqual(1111, pmsNumberSave.Entity.PropertyId);
                    Assert.AreEqual(5555, pmsNumberSave.Entity.ReservationId);
                    Assert.AreEqual("RES01", pmsNumberSave.Entity.ReservationNumber);

                    Assert.IsNotNull(pmsNumberSave.Entity.ReservationRooms);
                    Assert.IsTrue(pmsNumberSave.Entity.ReservationRooms.Any());
                    Assert.AreEqual(2, pmsNumberSave.Entity.ReservationRooms.Count);

                    var room1 = pmsNumberSave.Entity.ReservationRooms[0];

                    Assert.AreEqual(false, room1.IsCancellation);
                    Assert.AreEqual("AAA", room1.PmsReservationNumber);
                    Assert.AreEqual(1111, room1.PropertyId);
                    Assert.AreEqual("RES01", room1.ReservationNumber);
                    Assert.AreEqual("RES01/1", room1.ReservationRoomNo);

                    var room2 = pmsNumberSave.Entity.ReservationRooms[1];

                    Assert.AreEqual(false, room2.IsCancellation);
                    Assert.AreEqual("AAA", room2.PmsReservationNumber);
                    Assert.AreEqual(1111, room2.PropertyId);
                    Assert.AreEqual("RES01", room2.ReservationNumber);
                    Assert.AreEqual("RES01/2", room2.ReservationRoomNo);

                });

            reservationManagerPOCO.UpdateExternalConfirmationNumber(reservations, reservations[0].ReservationRooms.ToList(), rDirty, rrDirty, pmsId);

        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber_Event")]
        public void TestUpdatePMSReservationNumber_SendEvent_RRYes_RRDiff_RNo()
        {
            bool rrDirty = true;
            bool rDirty = false;
            long pmsId = 5;

            List<OB.Domain.Reservations.Reservation> reservations = new List<domainReservation.Reservation>()
            {
                new domainReservation.Reservation()
                {
                    UID = 5555,
                    Property_UID = 1111,
                    Channel_UID = 10,
                    Number = "RES01",
                    PmsRservationNumber = "RRR",
                    Status = 1,
                    ReservationRooms = new List<OB.Domain.Reservations.ReservationRoom>()
                    {
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/1",
                            PmsRservationNumber = "AAA",
                            Status = 1,
                        },
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/2",
                            PmsRservationNumber = "BBB",
                            Status = 1,
                        }
                    }
                }

            };

            ReservationManagerPOCO reservationManagerPOCO = Container.Resolve<ReservationManagerPOCO>();

            eventSystemManager.Setup(x => x.SendMessage(It.IsAny<Events.Contracts.ICustomNotification>()))
                .Callback((Events.Contracts.ICustomNotification notification) =>
                {

                    Assert.IsNotNull(notification);
                    var pmsNumberSaveNotification = (Events.Contracts.Messages.PmsNumberSaveNotification)notification;

                    Assert.IsNotNull(pmsNumberSaveNotification.Deltas);
                    Assert.IsTrue(pmsNumberSaveNotification.Deltas.Any());

                    var pmsNumberSave = (Events.Contracts.EntityDeltaBase<Events.Contracts.Entities.Pms.PmsNumberSave>)pmsNumberSaveNotification.Deltas.FirstOrDefault();

                    Assert.IsNotNull(pmsNumberSave);
                    Assert.IsNotNull(pmsNumberSave.Entity);
                    Assert.AreEqual(10, pmsNumberSave.Entity.ChannelId);
                    Assert.AreEqual(false, pmsNumberSave.Entity.IsCancellation);
                    Assert.AreEqual(5, pmsNumberSave.Entity.PmsId);
                    Assert.AreEqual(null, pmsNumberSave.Entity.PmsReservationNumber);
                    Assert.AreEqual(1111, pmsNumberSave.Entity.PropertyId);
                    Assert.AreEqual(5555, pmsNumberSave.Entity.ReservationId);
                    Assert.AreEqual("RES01", pmsNumberSave.Entity.ReservationNumber);

                    Assert.IsNotNull(pmsNumberSave.Entity.ReservationRooms);
                    Assert.IsTrue(pmsNumberSave.Entity.ReservationRooms.Any());
                    Assert.AreEqual(2, pmsNumberSave.Entity.ReservationRooms.Count);

                    var room1 = pmsNumberSave.Entity.ReservationRooms[0];

                    Assert.AreEqual(false, room1.IsCancellation);
                    Assert.AreEqual("AAA", room1.PmsReservationNumber);
                    Assert.AreEqual(1111, room1.PropertyId);
                    Assert.AreEqual("RES01", room1.ReservationNumber);
                    Assert.AreEqual("RES01/1", room1.ReservationRoomNo);

                    var room2 = pmsNumberSave.Entity.ReservationRooms[1];

                    Assert.AreEqual(false, room2.IsCancellation);
                    Assert.AreEqual("BBB", room2.PmsReservationNumber);
                    Assert.AreEqual(1111, room2.PropertyId);
                    Assert.AreEqual("RES01", room2.ReservationNumber);
                    Assert.AreEqual("RES01/2", room2.ReservationRoomNo);

                });

            reservationManagerPOCO.UpdateExternalConfirmationNumber(reservations, reservations[0].ReservationRooms.ToList(), rDirty, rrDirty, pmsId);

        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber_Event")]
        public void TestUpdatePMSReservationNumber_SendEvent_RRYes_RREqual_RYes()
        {
            bool rrDirty = true;
            bool rDirty = true;
            long pmsId = 5;

            List<OB.Domain.Reservations.Reservation> reservations = new List<domainReservation.Reservation>()
            {
                new domainReservation.Reservation()
                {
                    UID = 5555,
                    Property_UID = 1111,
                    Channel_UID = 10,
                    Number = "RES01",
                    PmsRservationNumber = "RRR",
                    Status = 1,
                    ReservationRooms = new List<OB.Domain.Reservations.ReservationRoom>()
                    {
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/1",
                            PmsRservationNumber = "AAA",
                            Status = 1,
                        },
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/2",
                            PmsRservationNumber = "AAA",
                            Status = 1,
                        }
                    }
                }

            };

            ReservationManagerPOCO reservationManagerPOCO = Container.Resolve<ReservationManagerPOCO>();

            eventSystemManager.Setup(x => x.SendMessage(It.IsAny<Events.Contracts.ICustomNotification>()))
                .Callback((Events.Contracts.ICustomNotification notification) =>
                {

                    Assert.IsNotNull(notification);
                    var pmsNumberSaveNotification = (Events.Contracts.Messages.PmsNumberSaveNotification)notification;

                    Assert.IsNotNull(pmsNumberSaveNotification.Deltas);
                    Assert.IsTrue(pmsNumberSaveNotification.Deltas.Any());

                    var pmsNumberSave = (Events.Contracts.EntityDeltaBase<Events.Contracts.Entities.Pms.PmsNumberSave>)pmsNumberSaveNotification.Deltas.FirstOrDefault();

                    Assert.IsNotNull(pmsNumberSave);
                    Assert.IsNotNull(pmsNumberSave.Entity);
                    Assert.AreEqual(10, pmsNumberSave.Entity.ChannelId);
                    Assert.AreEqual(false, pmsNumberSave.Entity.IsCancellation);
                    Assert.AreEqual(5, pmsNumberSave.Entity.PmsId);
                    Assert.AreEqual("RRR", pmsNumberSave.Entity.PmsReservationNumber);
                    Assert.AreEqual(1111, pmsNumberSave.Entity.PropertyId);
                    Assert.AreEqual(5555, pmsNumberSave.Entity.ReservationId);
                    Assert.AreEqual("RES01", pmsNumberSave.Entity.ReservationNumber);

                    Assert.IsNotNull(pmsNumberSave.Entity.ReservationRooms);
                    Assert.IsTrue(pmsNumberSave.Entity.ReservationRooms.Any());
                    Assert.AreEqual(2, pmsNumberSave.Entity.ReservationRooms.Count);

                    var room1 = pmsNumberSave.Entity.ReservationRooms[0];

                    Assert.AreEqual(false, room1.IsCancellation);
                    Assert.AreEqual("AAA", room1.PmsReservationNumber);
                    Assert.AreEqual(1111, room1.PropertyId);
                    Assert.AreEqual("RES01", room1.ReservationNumber);
                    Assert.AreEqual("RES01/1", room1.ReservationRoomNo);

                    var room2 = pmsNumberSave.Entity.ReservationRooms[1];

                    Assert.AreEqual(false, room2.IsCancellation);
                    Assert.AreEqual("AAA", room2.PmsReservationNumber);
                    Assert.AreEqual(1111, room2.PropertyId);
                    Assert.AreEqual("RES01", room2.ReservationNumber);
                    Assert.AreEqual("RES01/2", room2.ReservationRoomNo);

                });

            reservationManagerPOCO.UpdateExternalConfirmationNumber(reservations, reservations[0].ReservationRooms.ToList(), rDirty, rrDirty, pmsId);

        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber_Event")]
        public void TestUpdatePMSReservationNumber_SendEvent_RRYes_RRDiff_RYes()
        {
            bool rrDirty = true;
            bool rDirty = true;
            long pmsId = 5;

            List<OB.Domain.Reservations.Reservation> reservations = new List<domainReservation.Reservation>()
            {
                new domainReservation.Reservation()
                {
                    UID = 5555,
                    Property_UID = 1111,
                    Channel_UID = 10,
                    Number = "RES01",
                    PmsRservationNumber = "RRR",
                    Status = 1,
                    ReservationRooms = new List<OB.Domain.Reservations.ReservationRoom>()
                    {
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/1",
                            PmsRservationNumber = "AAA",
                            Status = 1,
                        },
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/2",
                            PmsRservationNumber = "BBB",
                            Status = 1,
                        }
                    }
                }

            };

            ReservationManagerPOCO reservationManagerPOCO = Container.Resolve<ReservationManagerPOCO>();

            eventSystemManager.Setup(x => x.SendMessage(It.IsAny<Events.Contracts.ICustomNotification>()))
                .Callback((Events.Contracts.ICustomNotification notification) =>
                {

                    Assert.IsNotNull(notification);
                    var pmsNumberSaveNotification = (Events.Contracts.Messages.PmsNumberSaveNotification)notification;

                    Assert.IsNotNull(pmsNumberSaveNotification.Deltas);
                    Assert.IsTrue(pmsNumberSaveNotification.Deltas.Any());

                    var pmsNumberSave = (Events.Contracts.EntityDeltaBase<Events.Contracts.Entities.Pms.PmsNumberSave>)pmsNumberSaveNotification.Deltas.FirstOrDefault();

                    Assert.IsNotNull(pmsNumberSave);
                    Assert.IsNotNull(pmsNumberSave.Entity);
                    Assert.AreEqual(10, pmsNumberSave.Entity.ChannelId);
                    Assert.AreEqual(false, pmsNumberSave.Entity.IsCancellation);
                    Assert.AreEqual(5, pmsNumberSave.Entity.PmsId);
                    Assert.AreEqual("RRR", pmsNumberSave.Entity.PmsReservationNumber);
                    Assert.AreEqual(1111, pmsNumberSave.Entity.PropertyId);
                    Assert.AreEqual(5555, pmsNumberSave.Entity.ReservationId);
                    Assert.AreEqual("RES01", pmsNumberSave.Entity.ReservationNumber);

                    Assert.IsNotNull(pmsNumberSave.Entity.ReservationRooms);
                    Assert.IsTrue(pmsNumberSave.Entity.ReservationRooms.Any());
                    Assert.AreEqual(2, pmsNumberSave.Entity.ReservationRooms.Count);

                    var room1 = pmsNumberSave.Entity.ReservationRooms[0];

                    Assert.AreEqual(false, room1.IsCancellation);
                    Assert.AreEqual("AAA", room1.PmsReservationNumber);
                    Assert.AreEqual(1111, room1.PropertyId);
                    Assert.AreEqual("RES01", room1.ReservationNumber);
                    Assert.AreEqual("RES01/1", room1.ReservationRoomNo);

                    var room2 = pmsNumberSave.Entity.ReservationRooms[1];

                    Assert.AreEqual(false, room2.IsCancellation);
                    Assert.AreEqual("BBB", room2.PmsReservationNumber);
                    Assert.AreEqual(1111, room2.PropertyId);
                    Assert.AreEqual("RES01", room2.ReservationNumber);
                    Assert.AreEqual("RES01/2", room2.ReservationRoomNo);

                });

            reservationManagerPOCO.UpdateExternalConfirmationNumber(reservations, reservations[0].ReservationRooms.ToList(), rDirty, rrDirty, pmsId);

        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber_Event")]
        public void TestUpdatePMSReservationNumber_SendEvent_RRNo_RRNA_RYes()
        {
            bool rrDirty = false;
            bool rDirty = true;
            long pmsId = 5;

            List<OB.Domain.Reservations.Reservation> reservations = new List<domainReservation.Reservation>()
            {
                new domainReservation.Reservation()
                {
                    UID = 5555,
                    Property_UID = 1111,
                    Channel_UID = 10,
                    Number = "RES01",
                    PmsRservationNumber = "RRR",
                    Status = 1,
                    ReservationRooms = new List<OB.Domain.Reservations.ReservationRoom>()
                    {
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/1",
                            PmsRservationNumber = "AAA",
                            Status = 1,
                        },
                        new domainReservation.ReservationRoom()
                        {
                            UID = 1,
                            Reservation_UID = 5555,
                            ReservationRoomNo = "RES01/2",
                            PmsRservationNumber = "BBB",
                            Status = 1,
                        }
                    }
                }

            };

            ReservationManagerPOCO reservationManagerPOCO = Container.Resolve<ReservationManagerPOCO>();

            eventSystemManager.Setup(x => x.SendMessage(It.IsAny<Events.Contracts.ICustomNotification>()))
                .Callback((Events.Contracts.ICustomNotification notification) =>
                {

                    Assert.IsNotNull(notification);
                    var pmsNumberSaveNotification = (Events.Contracts.Messages.PmsNumberSaveNotification)notification;

                    Assert.IsNotNull(pmsNumberSaveNotification.Deltas);
                    Assert.IsTrue(pmsNumberSaveNotification.Deltas.Any());

                    var pmsNumberSave = (Events.Contracts.EntityDeltaBase<Events.Contracts.Entities.Pms.PmsNumberSave>)pmsNumberSaveNotification.Deltas.FirstOrDefault();

                    Assert.IsNotNull(pmsNumberSave);
                    Assert.IsNotNull(pmsNumberSave.Entity);
                    Assert.AreEqual(10, pmsNumberSave.Entity.ChannelId);
                    Assert.AreEqual(false, pmsNumberSave.Entity.IsCancellation);
                    Assert.AreEqual(5, pmsNumberSave.Entity.PmsId);
                    Assert.AreEqual("RRR", pmsNumberSave.Entity.PmsReservationNumber);
                    Assert.AreEqual(1111, pmsNumberSave.Entity.PropertyId);
                    Assert.AreEqual(5555, pmsNumberSave.Entity.ReservationId);
                    Assert.AreEqual("RES01", pmsNumberSave.Entity.ReservationNumber);

                    Assert.IsNotNull(pmsNumberSave.Entity.ReservationRooms);
                    Assert.IsTrue(pmsNumberSave.Entity.ReservationRooms.Any());
                    Assert.AreEqual(2, pmsNumberSave.Entity.ReservationRooms.Count);

                    var room1 = pmsNumberSave.Entity.ReservationRooms[0];

                    Assert.AreEqual(false, room1.IsCancellation);
                    Assert.AreEqual("RRR", room1.PmsReservationNumber);
                    Assert.AreEqual(1111, room1.PropertyId);
                    Assert.AreEqual("RES01", room1.ReservationNumber);
                    Assert.AreEqual("RES01/1", room1.ReservationRoomNo);

                    var room2 = pmsNumberSave.Entity.ReservationRooms[1];

                    Assert.AreEqual(false, room2.IsCancellation);
                    Assert.AreEqual("RRR", room2.PmsReservationNumber);
                    Assert.AreEqual(1111, room2.PropertyId);
                    Assert.AreEqual("RES01", room2.ReservationNumber);
                    Assert.AreEqual("RES01/2", room2.ReservationRoomNo);

                });

            reservationManagerPOCO.UpdateExternalConfirmationNumber(reservations, reservations[0].ReservationRooms.ToList(), rDirty, rrDirty, pmsId);

        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber")]
        [Ignore]
        public void TestUpdatePMSReservationNumber_Success_ByReservation()
        {
            var request = new OB.Reservation.BL.Contracts.Requests.UpdatePMSReservationNumberRequest
            {
                PMSReservationNumberByReservationUID = new Dictionary<long, string>
                {
                    {1,  "PMS-NUMBER-1"},
                    {2,  "PMS-NUMBER-2"}
                },
                PMSReservationNumberByReservationRoomUID = new Dictionary<long, Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData>(),
            };
            var result = ReservationPOCO.UpdatePMSReservationNumber(request);

            Assert.AreEqual(Status.Success, result.Status);
            reservationsRepo.Verify(mock => mock.GetQuery(It.IsAny<Expression<Func<domainReservation.Reservation, bool>>>()), Times.Once());
            propertyRepo.Verify(mock => mock.ListPropertiesLight(It.IsAny<ListPropertyRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServicesPropertyMappings(It.IsAny<ListPMSServicesPropertyMappingRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServices(It.IsAny<ListPMSServiceRequest>()), Times.Once());

            unitOfWorkMock.Verify(mock => mock.Save(It.IsAny<int?>()), Times.Once());
            obpmsRepo.Verify(mock => mock.InsertPMSReservationsHistory(It.IsAny<InsertPMSReservationsHistoryRequest>()), Times.Once());


            reservationsInDatabase.Where(r => request.PMSReservationNumberByReservationUID.Select(i => i.Key).Contains(r.UID)).ForEach(r =>
            {
                Assert.AreEqual(request.PMSReservationNumberByReservationUID[r.UID], r.PmsRservationNumber);
            });
            Assert.AreEqual(2, pmsReservationsHistoryInDatabase.Count);
            request.PMSReservationNumberByReservationUID.ForEach(e =>
            {
                PMSReservationsHistory history = pmsReservationsHistoryInDatabase.Where(h => h.Reservation_UID == e.Key).Single();
                var reservation = reservationsInDatabase.Where(r => r.UID == e.Key).Single();
                var pmsServiceMapping = pmsServicePropertyMappingInDatabase.Where(s => s.Property_UID == reservation.Property_UID).First();
                var pmsService = pmsServiceInDatabase.Where(S => S.UID == pmsServiceMapping.PMSService_UID).FirstOrDefault();
                var property = propertyLightDatabase.Where(p => p.UID == reservation.Property_UID).First();

                Assert.AreEqual(e.Key, history.Reservation_UID);
                Assert.AreEqual(e.Value, history.PMSReservationNumber);
                Assert.AreEqual(reservation.Status, history.Status);
                Assert.AreEqual(pmsService.PMS_UID, history.PMS_UID);
                Assert.AreEqual(reservation.Property_UID, history.Property_UID);
                Assert.AreEqual(property.Client_UID, history.Client_UID);
            });
        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber")]
        [Ignore]
        public void TestUpdatePMSReservationNumber_Success_ByReservation_WithReservationRoom_SameGUID()
        {
            Guid guid = Guid.NewGuid();
            var request = new Reservation.BL.Contracts.Requests.UpdatePMSReservationNumberRequest
            {
                PMSReservationNumberByReservationUID = new Dictionary<long, string>
                {
                    {1, "PMS-Number-Reservation-1" }
                },
                PMSReservationNumberByReservationRoomUID = new Dictionary<long, Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData>
                {
                    {1, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = guid,
                            PMSNumber = "PMS-Number-1",
                        }
                    },
                    {2, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = guid,
                            PMSNumber = "PMS-Number-2",
                        }
                    },
                },
            };
            var result = ReservationPOCO.UpdatePMSReservationNumber(request);

            Assert.AreEqual(Status.Success, result.Status);
            reservationsRepo.Verify(mock => mock.GetQuery(It.IsAny<Expression<Func<domainReservation.Reservation, bool>>>()), Times.Once());
            reservationRoomRepo.Verify(mock => mock.GetQuery(It.IsAny<Expression<Func<domainReservation.ReservationRoom, bool>>>()), Times.Once());
            propertyRepo.Verify(mock => mock.ListPropertiesLight(It.IsAny<ListPropertyRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServicesPropertyMappings(It.IsAny<ListPMSServicesPropertyMappingRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServices(It.IsAny<ListPMSServiceRequest>()), Times.Once());

            unitOfWorkMock.Verify(mock => mock.Save(It.IsAny<int?>()), Times.Once());
            obpmsRepo.Verify(mock => mock.InsertPMSReservationsHistory(It.IsAny<InsertPMSReservationsHistoryRequest>()), Times.Once());

            reservationRoomsInDatabase.Where(rr => request.PMSReservationNumberByReservationRoomUID.Select(i => i.Key).Contains(rr.UID)).ForEach(rr =>
            {
                Assert.AreEqual(request.PMSReservationNumberByReservationRoomUID[rr.UID].PMSNumber, rr.PmsRservationNumber);
            });

            Assert.AreEqual(1, pmsReservationsHistoryInDatabase.Count);
            request.PMSReservationNumberByReservationRoomUID.ForEach(e =>
            {
                var reservationRoom = reservationRoomsInDatabase.Where(r => r.UID == e.Key).Single();

                PMSReservationsHistory history = pmsReservationsHistoryInDatabase.Where(h => h.Reservation_UID == reservationRoom.Reservation.UID).Single();
                var pmsServiceMapping = pmsServicePropertyMappingInDatabase.Where(s => s.Property_UID == reservationRoom.Reservation.Property_UID).First();
                var pmsService = pmsServiceInDatabase.Where(S => S.UID == pmsServiceMapping.PMSService_UID).FirstOrDefault();
                var property = propertyLightDatabase.Where(p => p.UID == reservationRoom.Reservation.Property_UID).First();

                Assert.AreEqual(reservationRoom.Reservation.UID, history.Reservation_UID);
                Assert.AreEqual(request.PMSReservationNumberByReservationUID[reservationRoom.Reservation_UID], history.PMSReservationNumber);
                // NOTE: Uncomment if fixing the Status Bug from the service
                //Assert.AreEqual((int)reservationRoom.Reservation.Status, history.Status);
                // NOTE: Remove the following line if fizing the Status Bug from the service
                Assert.AreEqual(reservationRoomsInDatabase.Where(r => request.PMSReservationNumberByReservationRoomUID.Select(E => E.Key).Contains(r.UID)).FirstOrDefault().Status, history.Status);
                Assert.AreEqual(pmsService.PMS_UID, history.PMS_UID);
                Assert.AreEqual(reservationRoom.Reservation.Property_UID, history.Property_UID);
                Assert.AreEqual(property.Client_UID, history.Client_UID);
            });
        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber")]
        [Ignore]
        public void TestUpdatePMSReservationNumber_Success_ByReservation_WithReservationRooms_DifferentGUID()
        {
            var request = new OB.Reservation.BL.Contracts.Requests.UpdatePMSReservationNumberRequest
            {
                PMSReservationNumberByReservationUID = new Dictionary<long, string>
                {
                    {1, "PMS-Number-Reservation-1" }
                },
                PMSReservationNumberByReservationRoomUID = new Dictionary<long, Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData>
                {
                    {1, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = Guid.NewGuid(),
                            PMSNumber = "PMS-Number-1",
                        }
                    },
                    {2, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = Guid.NewGuid(),
                            PMSNumber = "PMS-Number-2",
                        }
                    },
                },
            };
            var result = ReservationPOCO.UpdatePMSReservationNumber(request);

            Assert.AreEqual(Status.Success, result.Status);
            reservationRoomRepo.Verify(mock => mock.GetQuery(It.IsAny<Expression<Func<domainReservation.ReservationRoom, bool>>>()), Times.Once());
            propertyRepo.Verify(mock => mock.ListPropertiesLight(It.IsAny<ListPropertyRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServicesPropertyMappings(It.IsAny<ListPMSServicesPropertyMappingRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServices(It.IsAny<ListPMSServiceRequest>()), Times.Once());

            unitOfWorkMock.Verify(mock => mock.Save(It.IsAny<int?>()), Times.Once());
            obpmsRepo.Verify(mock => mock.InsertPMSReservationsHistory(It.IsAny<InsertPMSReservationsHistoryRequest>()), Times.Once());

            reservationRoomsInDatabase.Where(rr => request.PMSReservationNumberByReservationRoomUID.Select(i => i.Key).Contains(rr.UID)).ForEach(rr =>
            {
                Assert.AreEqual(request.PMSReservationNumberByReservationRoomUID[rr.UID].PMSNumber, rr.PmsRservationNumber);
            });

            Assert.AreEqual(2, pmsReservationsHistoryInDatabase.Count);
            request.PMSReservationNumberByReservationRoomUID.ForEach(e =>
            {
                var reservationRoom = reservationRoomsInDatabase.Where(r => r.UID == e.Key).Single();

                PMSReservationsHistory history = pmsReservationsHistoryInDatabase.Where(h => h.Reservation_UID == reservationRoom.Reservation.UID && h.PMSReservationNumber == e.Value.PMSNumber).Single();
                var pmsServiceMapping = pmsServicePropertyMappingInDatabase.Where(s => s.Property_UID == reservationRoom.Reservation.Property_UID).First();
                var pmsService = pmsServiceInDatabase.Where(S => S.UID == pmsServiceMapping.PMSService_UID).FirstOrDefault();
                var property = propertyLightDatabase.Where(p => p.UID == reservationRoom.Reservation.Property_UID).First();

                Assert.AreEqual(reservationRoom.Reservation.UID, history.Reservation_UID);
                Assert.AreEqual(e.Value.PMSNumber, history.PMSReservationNumber);
                Assert.AreEqual((int)reservationRoom.Status, history.Status);
                Assert.AreEqual(pmsService.PMS_UID, history.PMS_UID);
                Assert.AreEqual(reservationRoom.Reservation.Property_UID, history.Property_UID);
                Assert.AreEqual(property.Client_UID, history.Client_UID);
            });
        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber")]
        [Ignore]
        public void TestUpdatePMSReservationNumber_Success_ByReservationRoom_WithReservation_DifferentGUID()
        {
            var request = new OB.Reservation.BL.Contracts.Requests.UpdatePMSReservationNumberRequest
            {
                PMSReservationNumberByReservationRoomUID = new Dictionary<long, Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData>
                {
                    {1, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = Guid.NewGuid(),
                            PMSNumber = "PMS-Number-1",
                        }
                    },
                    {2, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = Guid.NewGuid(),
                            PMSNumber = "PMS-Number-2",
                        }
                    },
                },
            };
            var result = ReservationPOCO.UpdatePMSReservationNumber(request);

            Assert.AreEqual(Status.Success, result.Status);
            reservationRoomRepo.Verify(mock => mock.GetQuery(It.IsAny<Expression<Func<domainReservation.ReservationRoom, bool>>>()), Times.Once());
            propertyRepo.Verify(mock => mock.ListPropertiesLight(It.IsAny<ListPropertyRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServicesPropertyMappings(It.IsAny<ListPMSServicesPropertyMappingRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServices(It.IsAny<ListPMSServiceRequest>()), Times.Once());

            unitOfWorkMock.Verify(mock => mock.Save(It.IsAny<int?>()), Times.Once());
            obpmsRepo.Verify(mock => mock.InsertPMSReservationsHistory(It.IsAny<InsertPMSReservationsHistoryRequest>()), Times.Once());

            reservationRoomsInDatabase.Where(rr => request.PMSReservationNumberByReservationRoomUID.Select(i => i.Key).Contains(rr.UID)).ForEach(rr =>
            {
                Assert.AreEqual(request.PMSReservationNumberByReservationRoomUID[rr.UID].PMSNumber, rr.PmsRservationNumber);
            });

            Assert.AreEqual(2, pmsReservationsHistoryInDatabase.Count);
            request.PMSReservationNumberByReservationRoomUID.ForEach(e =>
            {
                var reservationRoom = reservationRoomsInDatabase.Where(r => r.UID == e.Key).Single();

                PMSReservationsHistory history = pmsReservationsHistoryInDatabase.Where(h => h.Reservation_UID == reservationRoom.Reservation.UID && h.PMSReservationNumber == e.Value.PMSNumber).Single();
                var pmsServiceMapping = pmsServicePropertyMappingInDatabase.Where(s => s.Property_UID == reservationRoom.Reservation.Property_UID).First();
                var pmsService = pmsServiceInDatabase.Where(S => S.UID == pmsServiceMapping.PMSService_UID).FirstOrDefault();
                var property = propertyLightDatabase.Where(p => p.UID == reservationRoom.Reservation.Property_UID).First();

                Assert.AreEqual(reservationRoom.Reservation.UID, history.Reservation_UID);
                Assert.AreEqual(e.Value.PMSNumber, history.PMSReservationNumber);
                Assert.AreEqual((int)reservationRoom.Status, history.Status);
                Assert.AreEqual(pmsService.PMS_UID, history.PMS_UID);
                Assert.AreEqual(reservationRoom.Reservation.Property_UID, history.Property_UID);
                Assert.AreEqual(property.Client_UID, history.Client_UID);
            });
        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber")]
        [Ignore]
        public void TestUpdatePMSReservationNumber_Success_ByReservationRoom_WithReservation_SameGUID()
        {
            Guid guid = new Guid();
            var request = new OB.Reservation.BL.Contracts.Requests.UpdatePMSReservationNumberRequest
            {
                PMSReservationNumberByReservationRoomUID = new Dictionary<long, Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData>
                {
                    {1, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = guid,
                            PMSNumber = "PMS-Number-1",
                        }
                    },
                    {2, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = guid,
                            PMSNumber = "PMS-Number-2",
                        }
                    },
                },
            };
            var result = ReservationPOCO.UpdatePMSReservationNumber(request);

            Assert.AreEqual(Status.Success, result.Status);
            reservationRoomRepo.Verify(mock => mock.GetQuery(It.IsAny<Expression<Func<domainReservation.ReservationRoom, bool>>>()), Times.Once());
            propertyRepo.Verify(mock => mock.ListPropertiesLight(It.IsAny<ListPropertyRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServicesPropertyMappings(It.IsAny<ListPMSServicesPropertyMappingRequest>()), Times.Once());
            obpmsRepo.Verify(mock => mock.ListPMSServices(It.IsAny<ListPMSServiceRequest>()), Times.Once());

            unitOfWorkMock.Verify(mock => mock.Save(It.IsAny<int?>()), Times.Once());
            obpmsRepo.Verify(mock => mock.InsertPMSReservationsHistory(It.IsAny<InsertPMSReservationsHistoryRequest>()), Times.Once());

            reservationRoomsInDatabase.Where(rr => request.PMSReservationNumberByReservationRoomUID.Select(i => i.Key).Contains(rr.UID)).ForEach(rr =>
            {
                Assert.AreEqual(request.PMSReservationNumberByReservationRoomUID[rr.UID].PMSNumber, rr.PmsRservationNumber);
            });

            Assert.AreEqual(1, pmsReservationsHistoryInDatabase.Count);
            request.PMSReservationNumberByReservationRoomUID.ForEach(e =>
            {
                var reservationRoom = reservationRoomsInDatabase.Where(r => r.UID == e.Key).Single();

                PMSReservationsHistory history = pmsReservationsHistoryInDatabase.Where(h => h.Reservation_UID == reservationRoom.Reservation.UID).Single();
                var pmsServiceMapping = pmsServicePropertyMappingInDatabase.Where(s => s.Property_UID == reservationRoom.Reservation.Property_UID).First();
                var pmsService = pmsServiceInDatabase.Where(S => S.UID == pmsServiceMapping.PMSService_UID).FirstOrDefault();
                var property = propertyLightDatabase.Where(p => p.UID == reservationRoom.Reservation.Property_UID).First();

                Assert.AreEqual(reservationRoom.Reservation.UID, history.Reservation_UID);
                Assert.AreNotEqual(e.Value.PMSNumber, history.PMSReservationNumber);
                // NOTE: Uncomment if fixing the Status Bug from the service
                //Assert.AreEqual((int)reservationRoom.Reservation.Status, history.Status);
                // NOTE: Remove the following line if fizing the Status Bug from the service
                Assert.AreEqual(reservationRoomsInDatabase.Where(r => request.PMSReservationNumberByReservationRoomUID.Select(E => E.Key).Contains(r.UID)).FirstOrDefault().Status, history.Status);
                Assert.AreEqual(pmsService.PMS_UID, history.PMS_UID);
                Assert.AreEqual(reservationRoom.Reservation.Property_UID, history.Property_UID);
                Assert.AreEqual(property.Client_UID, history.Client_UID);
            });
        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber")]
        [Ignore]
        public void TestUpdatePMSReservationNumber_Success_ByReservation_GivenParameters()
        {
            var request = new OB.Reservation.BL.Contracts.Requests.UpdatePMSReservationNumberRequest
            {
                PMSReservationNumberByReservationUID = new Dictionary<long, string>
                {
                    {1,  "PMS-NUMBER-1"},
                    {2,  "PMS-NUMBER-2"}
                },
                PMSReservationNumberByReservationRoomUID = new Dictionary<long, Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData>(),
                ClientId = 1,
                PmsId = 1
            };
            var result = ReservationPOCO.UpdatePMSReservationNumber(request);

            Assert.AreEqual(Status.Success, result.Status);
            reservationsRepo.Verify(mock => mock.GetQuery(It.IsAny<Expression<Func<domainReservation.Reservation, bool>>>()), Times.Once());
            propertyRepo.Verify(mock => mock.ListPropertiesLight(It.IsAny<ListPropertyRequest>()), Times.Never());
            obpmsRepo.Verify(mock => mock.ListPMSServicesPropertyMappings(It.IsAny<ListPMSServicesPropertyMappingRequest>()), Times.Never());
            obpmsRepo.Verify(mock => mock.ListPMSServices(It.IsAny<ListPMSServiceRequest>()), Times.Never());

            unitOfWorkMock.Verify(mock => mock.Save(It.IsAny<int?>()), Times.Once());
            obpmsRepo.Verify(mock => mock.InsertPMSReservationsHistory(It.IsAny<InsertPMSReservationsHistoryRequest>()), Times.Once());

            reservationsInDatabase.Where(r => request.PMSReservationNumberByReservationUID.Select(i => i.Key).Contains(r.UID)).ForEach(r =>
            {
                Assert.AreEqual(request.PMSReservationNumberByReservationUID[r.UID], r.PmsRservationNumber);
            });
            Assert.AreEqual(2, pmsReservationsHistoryInDatabase.Count);
            request.PMSReservationNumberByReservationUID.ForEach(e =>
            {
                PMSReservationsHistory history = pmsReservationsHistoryInDatabase.Where(h => h.Reservation_UID == e.Key).Single();
                var reservation = reservationsInDatabase.Where(r => r.UID == e.Key).Single();
                var pmsServiceMapping = pmsServicePropertyMappingInDatabase.Where(s => s.Property_UID == reservation.Property_UID).First();
                var pmsService = pmsServiceInDatabase.Where(S => S.UID == pmsServiceMapping.PMSService_UID).FirstOrDefault();
                var property = propertyLightDatabase.Where(p => p.UID == reservation.Property_UID).First();

                Assert.AreEqual(e.Key, history.Reservation_UID);
                Assert.AreEqual(e.Value, history.PMSReservationNumber);
                Assert.AreEqual(reservation.Status, history.Status);
                Assert.AreEqual(pmsService.PMS_UID, history.PMS_UID);
                Assert.AreEqual(reservation.Property_UID, history.Property_UID);
                Assert.AreEqual(property.Client_UID, history.Client_UID);
            });
        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber")]
        [Ignore]
        public void TestUpdatePMSReservationNumber_Success_ByReservationRoom_WithReservation_DifferentGUID_GivenParameters()
        {
            var request = new OB.Reservation.BL.Contracts.Requests.UpdatePMSReservationNumberRequest
            {
                PMSReservationNumberByReservationRoomUID = new Dictionary<long, Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData>
                {
                    {1, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = Guid.NewGuid(),
                            PMSNumber = "PMS-Number-1",
                        }
                    },
                    {2, new  Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData
                        {
                            Guid = Guid.NewGuid(),
                            PMSNumber = "PMS-Number-2",
                        }
                    },
                },
                ClientId = 1,
                PmsId = 1
            };
            var result = ReservationPOCO.UpdatePMSReservationNumber(request);

            Assert.AreEqual(Status.Success, result.Status);
            reservationRoomRepo.Verify(mock => mock.GetQuery(It.IsAny<Expression<Func<domainReservation.ReservationRoom, bool>>>()), Times.Once());
            propertyRepo.Verify(mock => mock.ListPropertiesLight(It.IsAny<ListPropertyRequest>()), Times.Never());
            obpmsRepo.Verify(mock => mock.ListPMSServicesPropertyMappings(It.IsAny<ListPMSServicesPropertyMappingRequest>()), Times.Never());
            obpmsRepo.Verify(mock => mock.ListPMSServices(It.IsAny<ListPMSServiceRequest>()), Times.Never());

            unitOfWorkMock.Verify(mock => mock.Save(It.IsAny<int?>()), Times.Once());
            obpmsRepo.Verify(mock => mock.InsertPMSReservationsHistory(It.IsAny<InsertPMSReservationsHistoryRequest>()), Times.Once());

            reservationRoomsInDatabase.Where(rr => request.PMSReservationNumberByReservationRoomUID.Select(i => i.Key).Contains(rr.UID)).ForEach(rr =>
            {
                Assert.AreEqual(request.PMSReservationNumberByReservationRoomUID[rr.UID].PMSNumber, rr.PmsRservationNumber);
            });

            Assert.AreEqual(2, pmsReservationsHistoryInDatabase.Count);
            request.PMSReservationNumberByReservationRoomUID.ForEach(e =>
            {
                var reservationRoom = reservationRoomsInDatabase.Where(r => r.UID == e.Key).Single();

                PMSReservationsHistory history = pmsReservationsHistoryInDatabase.Where(h => h.Reservation_UID == reservationRoom.Reservation.UID && h.PMSReservationNumber == e.Value.PMSNumber).Single();
                var pmsServiceMapping = pmsServicePropertyMappingInDatabase.Where(s => s.Property_UID == reservationRoom.Reservation.Property_UID).First();
                var pmsService = pmsServiceInDatabase.Where(S => S.UID == pmsServiceMapping.PMSService_UID).FirstOrDefault();
                var property = propertyLightDatabase.Where(p => p.UID == reservationRoom.Reservation.Property_UID).First();

                Assert.AreEqual(reservationRoom.Reservation.UID, history.Reservation_UID);
                Assert.AreEqual(e.Value.PMSNumber, history.PMSReservationNumber);
                Assert.AreEqual((int)reservationRoom.Status, history.Status);
                Assert.AreEqual(pmsService.PMS_UID, history.PMS_UID);
                Assert.AreEqual(reservationRoom.Reservation.Property_UID, history.Property_UID);
                Assert.AreEqual(property.Client_UID, history.Client_UID);
            });
        }

        [TestMethod]
        [TestCategory("UpdatePMSReservationNumber")]
        [Ignore]
        public void TestUpdatePMSReservationNumber_Success_ByReservation_ReservationRoomGetQueryOnlyOnce()
        {
            var request = new OB.Reservation.BL.Contracts.Requests.UpdatePMSReservationNumberRequest
            {
                PMSReservationNumberByReservationUID = new Dictionary<long, string>
                {
                    {1,  "PMS-NUMBER-1"},
                    {2,  "PMS-NUMBER-2"},
                    {3,  "PMS-NUMBER-3"},
                },
                PMSReservationNumberByReservationRoomUID = new Dictionary<long, Reservation.BL.Contracts.Data.PMS.PMSHistoryRoomData>(),
            };
            var result = ReservationPOCO.UpdatePMSReservationNumber(request);

            Assert.AreEqual(Status.Success, result.Status);
            reservationRoomRepo.Verify(mock => mock.GetQuery(It.IsAny<Expression<Func<domainReservation.ReservationRoom, bool>>>()), Times.Once());
        }


        #region Test ModifyReservationBackgroundOperations

        [TestMethod]
        [TestCategory("ModifyReservationBackgroundOperations")]
        public void TestModifyReservationBackgroundOperations_Ignore_PendingToBooked_CheckNotSendToPms()
        {
            var newReservation = BaseReservation.Clone();
            newReservation.Status = (long)Constants.ReservationStatus.Booked;
            newReservation.ReservationRooms[0].Status = (int?)Constants.ReservationStatus.Booked;

            var request = new Reservation.BL.Contracts.Logs.ReservationBackgroundOperationsRequest 
            {
                NewReservation = newReservation,
                OldReservation = BaseReservation,
                SendOperatorCreditLimitExcededEmail = false,
                SendTPICreditLimitExcededEmail = false,
                ReservationTransactionId = "teste",
                ReservationTransactionState = Reservation.BL.Constants.ReservationTransactionStatus.Pending,
                ReservationAdditionalDataUID = 0,
                ReservationAdditionalData = new domainReservation.ReservationsAdditionalData { },
                HangfireId = 1000,
                Inventories = null,
                BigPullAuthRequestor_UID = 1,
                ReservationRequest = new Reservation.BL.Contracts.Requests.ReservationBaseRequest
                {
                    TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Ignore
                }
            };

            MethodInfo methodInfo = ReservationPOCO.GetType().GetMethod("ModifyReservationBackgroundOperations", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(this.ReservationPOCO, new object[] { request, false });

            obRateRepo.Verify(x => x.ConnectorEventQueueInsert(It.IsAny<ConnectorEventQueueInsertRequest>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("ModifyReservationBackgroundOperations")]
        public void TestModifyReservationBackgroundOperations_Ignore_PendingToModified_CheckNotSendToPms()
        {
            var newReservation = BaseReservation.Clone();
            newReservation.Status = (long)Constants.ReservationStatus.Modified;
            newReservation.ReservationRooms[0].Status = (int?)Constants.ReservationStatus.Modified;

            var request = new Reservation.BL.Contracts.Logs.ReservationBackgroundOperationsRequest
            {
                NewReservation = newReservation,
                OldReservation = BaseReservation,
                SendOperatorCreditLimitExcededEmail = false,
                SendTPICreditLimitExcededEmail = false,
                ReservationTransactionId = "teste",
                ReservationTransactionState = Reservation.BL.Constants.ReservationTransactionStatus.Pending,
                ReservationAdditionalDataUID = 0,
                ReservationAdditionalData = new domainReservation.ReservationsAdditionalData { },
                HangfireId = 1000,
                Inventories = null,
                BigPullAuthRequestor_UID = 1,
                ReservationRequest = new Reservation.BL.Contracts.Requests.ReservationBaseRequest
                {
                    TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Ignore
                }
            };

            MethodInfo methodInfo = ReservationPOCO.GetType().GetMethod("ModifyReservationBackgroundOperations", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(this.ReservationPOCO, new object[] { request, false });

            obRateRepo.Verify(x => x.ConnectorEventQueueInsert(It.IsAny<ConnectorEventQueueInsertRequest>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("ModifyReservationBackgroundOperations")]
        public void TestModifyReservationBackgroundOperations_Commit_PendingToBooked_CheckSendToPms()
        {
            var newReservation = BaseReservation.Clone();
            newReservation.Status = (long)Constants.ReservationStatus.Booked;
            newReservation.ReservationRooms[0].Status = (int?)Constants.ReservationStatus.Booked;

            var request = new Reservation.BL.Contracts.Logs.ReservationBackgroundOperationsRequest
            {
                DecrementInventory = false,
                NewReservation = newReservation,
                OldReservation = BaseReservation,
                propertyID = newReservation.Property_UID,
                ReservationTransactionId = "teste",
                ReservationTransactionState = Reservation.BL.Constants.ReservationTransactionStatus.Commited,
                ServiceName = Reservation.BL.Contracts.Data.General.ServiceName.ModifyReservation,
                UserID = 0,
                UserIsGuest = false,
                HangfireId = 1000,
                Inventories = null,
                BigPullAuthRequestor_UID = 1
            };

            MethodInfo methodInfo = ReservationPOCO.GetType().GetMethod("ModifyReservationBackgroundOperations", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(this.ReservationPOCO, new object[] { request, false });

            obRateRepo.Verify(x => x.ConnectorEventQueueInsert(It.IsAny<ConnectorEventQueueInsertRequest>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("ModifyReservationBackgroundOperations")]
        public void TestModifyReservationBackgroundOperations_Commit_PendingToModified_CheckSendToPms()
        {
            var newReservation = BaseReservation.Clone();
            newReservation.Status = (long)Constants.ReservationStatus.Modified;
            newReservation.ReservationRooms[0].Status = (int?)Constants.ReservationStatus.Modified;

            var request = new Reservation.BL.Contracts.Logs.ReservationBackgroundOperationsRequest
            {
                DecrementInventory = false,
                NewReservation = newReservation,
                OldReservation = BaseReservation,
                propertyID = newReservation.Property_UID,
                ReservationTransactionId = "teste",
                ReservationTransactionState = Reservation.BL.Constants.ReservationTransactionStatus.Commited,
                ServiceName = Reservation.BL.Contracts.Data.General.ServiceName.ModifyReservation,
                UserID = 0,
                UserIsGuest = false,
                HangfireId = 1000,
                Inventories = null,
                BigPullAuthRequestor_UID = 1
            };

            MethodInfo methodInfo = ReservationPOCO.GetType().GetMethod("ModifyReservationBackgroundOperations", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(this.ReservationPOCO, new object[] { request, false });

            obRateRepo.Verify(x => x.ConnectorEventQueueInsert(It.IsAny<ConnectorEventQueueInsertRequest>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("ModifyReservationBackgroundOperations")]
        public void TestModifyReservationBackgroundOperations_Commit_BookedToPending_CheckNotSendToPms()
        {
            var newReservation = BaseReservation.Clone();
            newReservation.Status = (long)Constants.ReservationStatus.Pending;
            newReservation.ReservationRooms[0].Status = (int?)Constants.ReservationStatus.Pending;

            var oldReservation = BaseReservation.Clone();
            oldReservation.Status = (long)Constants.ReservationStatus.Booked;
            oldReservation.ReservationRooms[0].Status = (int?)Constants.ReservationStatus.Booked;

            var request = new Reservation.BL.Contracts.Logs.ReservationBackgroundOperationsRequest
            {
                NewReservation = newReservation,
                OldReservation = BaseReservation,
                SendOperatorCreditLimitExcededEmail = false,
                SendTPICreditLimitExcededEmail = false,
                ReservationTransactionId = "teste",
                ReservationTransactionState = Reservation.BL.Constants.ReservationTransactionStatus.Pending,
                ReservationAdditionalDataUID = (long?)null,
                ReservationAdditionalData = null,
                HangfireId = 1000,
                ReservationRequest = new Reservation.BL.Contracts.Requests.ReservationBaseRequest
                {
                    TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Initiate
                },
                TransactionRetries = 0,
                Inventories = null,
                BigPullAuthRequestor_UID = 1
            };

            MethodInfo methodInfo = ReservationPOCO.GetType().GetMethod("ModifyReservationBackgroundOperations", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(this.ReservationPOCO, new object[] { request, false });

            obRateRepo.Verify(x => x.ConnectorEventQueueInsert(It.IsAny<ConnectorEventQueueInsertRequest>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("ModifyReservationBackgroundOperations")]
        public void TestModifyReservationBackgroundOperations_Commit_ModifiedToPending_CheckNotSendToPms()
        {
            var newReservation = BaseReservation.Clone();
            newReservation.Status = (long)Constants.ReservationStatus.Pending;
            newReservation.ReservationRooms[0].Status = (int?)Constants.ReservationStatus.Pending;

            var oldReservation = BaseReservation.Clone();
            oldReservation.Status = (long)Constants.ReservationStatus.Modified;
            oldReservation.ReservationRooms[0].Status = (int?)Constants.ReservationStatus.Modified;

            var request = new Reservation.BL.Contracts.Logs.ReservationBackgroundOperationsRequest
            {
                NewReservation = newReservation,
                OldReservation = BaseReservation,
                SendOperatorCreditLimitExcededEmail = false,
                SendTPICreditLimitExcededEmail = false,
                ReservationTransactionId = "teste",
                ReservationTransactionState = Reservation.BL.Constants.ReservationTransactionStatus.Pending,
                ReservationAdditionalDataUID = (long?)null,
                ReservationAdditionalData = null,
                HangfireId = 1000,
                ReservationRequest = new Reservation.BL.Contracts.Requests.ReservationBaseRequest
                {
                    TransactionAction = Reservation.BL.Constants.ReservationTransactionAction.Initiate
                },
                TransactionRetries = 0,
                Inventories = null,
                BigPullAuthRequestor_UID = 1
            };

            MethodInfo methodInfo = ReservationPOCO.GetType().GetMethod("ModifyReservationBackgroundOperations", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(this.ReservationPOCO, new object[] { request, false });

            obRateRepo.Verify(x => x.ConnectorEventQueueInsert(It.IsAny<ConnectorEventQueueInsertRequest>()), Times.Never);
        }

        #endregion Test ModifyReservationBackgroundOperation

    }
}
