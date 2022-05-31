using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OB.BL.Contracts.Requests;
using OB.BL.Operations.Interfaces;
using OB.DL.Common.Repositories.Interfaces.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class ValidateReservationRestricionsUnitTest : UnitBaseTest
    {

        public IReservationManagerPOCO ReservationManagerPOCO { get; set; }

        //GetRepository<PropertyQueue>
        private Mock<IReservationManagerPOCO> _reservationManagerPOCOMock = null;
        private Mock<IOBRateRoomDetailsForReservationRoomRepository> _rrdRepoMock;


        List<Contracts.Data.Rates.RateRoomDetail> rateRoomDetailList = null;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _reservationManagerPOCOMock = new Mock<IReservationManagerPOCO>(MockBehavior.Default);
            _rrdRepoMock = new Mock<IOBRateRoomDetailsForReservationRoomRepository>(MockBehavior.Default);

            this.ReservationManagerPOCO = this.Container.Resolve<IReservationManagerPOCO>();

            RepositoryFactoryMock.Setup(x => x.GetOBRateRoomDetailsForReservationRoomRepository())
                .Returns(_rrdRepoMock.Object);


            this.Container = this.Container.RegisterInstance<IOBRateRoomDetailsForReservationRoomRepository>(_rrdRepoMock.Object);
            this.Container = this.Container.RegisterInstance<IReservationManagerPOCO>(_reservationManagerPOCOMock.Object);

            rateRoomDetailList = new List<Contracts.Data.Rates.RateRoomDetail>();

            _rrdRepoMock.Setup(x => x.ListRateRoomDetailsAsync(It.IsAny<ListRateRoomDetailsRequest>()))
              .ReturnsAsync((ListRateRoomDetailsRequest req) =>
              {
                  return rateRoomDetailList.Where(x => x.Date >= req.RrdFromDate && x.Date <= req.RrdToDate).ToList();
              });
        }

        private void FillListRateRoomDetail()
        {
            rateRoomDetailList = new List<Contracts.Data.Rates.RateRoomDetail>
            {
                 new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,01),
                      BlockedChannelsListIds = new List<long>()
                  },
                   new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = true,
                      Date =new DateTime(2018,01,02),
                      BlockedChannelsListIds = new List<long>()
                  },
                     new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = true,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,03),
                      BlockedChannelsListIds = new List<long>()
                  },
                     new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = 4,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,04),
                      BlockedChannelsListIds = new List<long>()
                  },
                     new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = 3,
                      StayThrough = null,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,05),
                      BlockedChannelsListIds = new List<long>()
                  },
                    new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,06),
                      BlockedChannelsListIds = new List<long>()
                  },
                    new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,07),
                      BlockedChannelsListIds = new List<long>()
                  },
                     new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,08),
                      BlockedChannelsListIds = new List<long>()
                  },
                     new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = 3,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,09),
                      BlockedChannelsListIds = new List<long>()
                  },
                     new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = true,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,10),
                      BlockedChannelsListIds = new List<long>()
                  },
                     new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = true,
                      Date =new DateTime(2018,01,11),
                      BlockedChannelsListIds = new List<long>()
                  },
                          new Contracts.Data.Rates.RateRoomDetail
                  {
                      MinimumLengthOfStay = null,
                      MaximumLengthOfStay = null,
                      StayThrough = null,
                      ClosedOnArrival = null,
                      ClosedOnDeparture = null,
                      Date =new DateTime(2018,01,12),
                      BlockedChannelsListIds = new List<long>{ 35 }
                  },

            };
        }

        //Business rules tests 

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_Simple_Success()
        {

            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                PropertyId = 1,
                ChannelId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(true, result.Result, "Expected True value");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_NotFound_TotalDays_False()
        {
            FillListRateRoomDetail();

            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                PropertyId = 1,
                ChannelId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,01),
                        CheckOut = new DateTime(2018,01,06)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(false, result.Result, "Exprected False value");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_MinimumLengthOfStay_Null_True()
        {
            FillListRateRoomDetail();

            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                PropertyId = 1,
                ChannelId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(true, result.Result, "Expected True value");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_MinimumLengthOfStay_True()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                PropertyId = 1,
                ChannelId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,05),
                        CheckOut = new DateTime(2018,01,07)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(true, result.Result, "Exprected True value");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_MinimumLengthOfStay_False()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,02),
                        CheckOut = new DateTime(2018,01,04)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(false, result.Result, "IsValid equal False expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_MaximumLengthOfStay_Null_True()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,01),
                        CheckOut = new DateTime(2018,01,03)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(true, result.Result, "IsValid equal True expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_MaximumLengthOfStay_True()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,05),
                        CheckOut = new DateTime(2018,01,06)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(true, result.Result, "IsValid equal True expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_MaximumLengthOfStay_False()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,05),
                        CheckOut = new DateTime(2018,01,10)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(false, result.Result, "IsValid equal False expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_StayThrough_Null_True()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,07),
                        CheckOut = new DateTime(2018,01,08)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(true, result.Result, "IsValid equal True expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_StayThrough_True()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,06),
                        CheckOut = new DateTime(2018,01,09)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(true, result.Result, "IsValid equal True expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_StayThrough_Bigger_False()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,06),
                        CheckOut = new DateTime(2018,01,10)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(false, result.Result, "IsValid equal False expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_StayThrough_Smaller_False()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,08),
                        CheckOut = new DateTime(2018,01,09)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(false, result.Result, "IsValid equal False expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ClosedOnArrival_True_Return_False()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,10),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(false, result.Result, "IsValid equal False expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ClosedOnArrival_True_Return_True()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,07),
                        CheckOut = new DateTime(2018,01,10)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(true, result.Result, "IsValid equal True expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ClosedOnDeparture_True_Return_False()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,09),
                        CheckOut = new DateTime(2018,01,11)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(false, result.Result, "IsValid equal False expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ClosedOnDeparture_True_Return_True()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(true, result.Result, "IsValid equal True expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_BlockedChannelsListIds_Return_True()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 38,
                PropertyId = 1,
                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            Assert.AreEqual(true, result.Result, "IsValid equal True expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_BlockedChannelsListIds_Return_False()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 35,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            Assert.AreEqual(false, result.Result, "IsValid equal False expected");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        ///request validation

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ValidateRequest_Property_Null()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, result.Status, "Expected fail status");
            Assert.AreEqual(1, result.Errors.Count, "Expected 1 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ValidateRequest_CheckIn_Null()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, result.Status, "Expected fail status");
            Assert.AreEqual(1, result.Errors.Count, "Expected 1 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ValidateRequest_CheckOut_Null()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, result.Status, "Expected fail status");
            Assert.AreEqual(1, result.Errors.Count, "Expected 1 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ValidateRequest_RateId_Null()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, result.Status, "Expected fail status");
            Assert.AreEqual(1, result.Errors.Count, "Expected 1 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ValidateRequest_RoomTypeId_Null()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, result.Status, "Expected fail status");
            Assert.AreEqual(1, result.Errors.Count, "Expected 1 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ValidateRequest_CheckIn_BiggerThan_CheckOut()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,12),
                        CheckOut = new DateTime(2018,01,11)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, result.Status, "Expected fail status");
            Assert.AreEqual(1, result.Errors.Count, "Expected 1 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_ValidateRequest_CheckIn_LessThan_CheckOut()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    }
                }
            });

            // assert
            
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        //multiple request

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_MultipleRequests_Success()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,
                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    },
                     new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,01),
                        CheckOut = new DateTime(2018,01,03)
                    }
                }
            });

            // assert
            Assert.AreEqual(true, result.Result, "Expected Result = true");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Success, result.Status, "Expected Success status");
            Assert.AreEqual(0, result.Errors.Count, "Expected 0 error");
        }

        [TestMethod]
        [TestCategory("ValidateReservationRestricions")]
        public async Task TestRateRoomAvaliebleByDate_MultipleRequests_Fail()
        {
            FillListRateRoomDetail();

            // act            
            var result = await this.ReservationManagerPOCO.ValidateReservationRestrictions(new OB.Reservation.BL.Contracts.Requests.ValidateReservationRestricionsRequest()
            {
                ChannelId = 1,
                PropertyId = 1,

                Items = new List<Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions>
                {
                    new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        //CheckIn = new DateTime(2018,01,11),
                        CheckOut = new DateTime(2018,01,12)
                    },
                     new Reservation.BL.Contracts.Data.Reservations.ValidateReservationRestricions
                    {
                        RateId = 1,
                        RoomTypeId = 1,
                        CheckIn = new DateTime(2018,01,01),
                        CheckOut = new DateTime(2018,01,03)
                    }
                }
            });

            // assert
            Assert.AreEqual(false, result.Result, "Expected Result = false");
            Assert.AreEqual(OB.Reservation.BL.Contracts.Responses.Status.Fail, result.Status, "Expected Fail status");
            Assert.AreEqual(1, result.Errors.Count, "Expected 0 error");
        }

    }
}
