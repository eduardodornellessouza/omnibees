using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Moq;
using OB.Reservation.BL.Contracts.Requests;
using OB.BL.Operations.Impl;
using OB.DL.Common.Repositories.Interfaces.Rest;
using OBContracts = OB.BL.Contracts.Requests;
using System.Linq;
using OB.DL.Common.Interfaces;
using Microsoft.Practices.Unity;
using OB.Api.Core;
using System.Transactions;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Exceptions;
using OB.BL.Contracts.Responses;

namespace OB.BL.Operations.Test.POCOS
{
    [TestClass]
    public class ApproveOrRefuseOnRequestReservationUnitTest : UnitBaseTest
    {
        private Mock<ReservationManagerPOCO> _reservationManagerMock = null;
        private Mock<IOBChannelRepository> _channelRepository = null;

        protected List<OB.BL.Contracts.Data.Channels.Channel> _channelsInDatabase = new List<OB.BL.Contracts.Data.Channels.Channel>();


        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            InitReservationManagerMock();
            InitChannelRepositoryMock();
        }

        private void InitReservationManagerMock()
        {
            _reservationManagerMock = new Mock<ReservationManagerPOCO>(MockBehavior.Default);
            Container.RegisterInstance(_reservationManagerMock.Object);
            _reservationManagerMock.Object.Container = this.Container;
            _reservationManagerMock.Object.RepositoryFactory = this.Container.Resolve<IRepositoryFactory>();
        }

        private void InitChannelRepositoryMock()
        {
            _channelRepository = new Mock<IOBChannelRepository>(MockBehavior.Default);
            RepositoryFactoryMock.Setup(x => x.GetOBChannelRepository()).Returns(_channelRepository.Object);
            _channelRepository.Setup(x => x.ListChannel(It.IsAny<OBContracts.ListChannelRequest>()))
                .Returns((OBContracts.ListChannelRequest request) =>
                {
                    return _channelsInDatabase.Where(c => request.ChannelIds.Contains(c.Id)).ToList();
                });

            _channelsInDatabase.Add(new Contracts.Data.Channels.Channel
            {
                Id = 1,
                Type = 0
            });

            _channelsInDatabase.Add(new Contracts.Data.Channels.Channel
            {
                Id = 2,
                Type = 2
            });
        }

        [TestMethod]
        [TestCategory("ApproveOrRefuseOnRequest")]
        [ExpectedException(typeof(NullReferenceException))]
        public void NoRequestException()
        {
            _reservationManagerMock.Object.ApproveOrRefuseOnRequestReservation(null);
        }

        [TestMethod]
        [TestCategory("ApproveOrRefuseOnRequest")]
        public void ChannelIDInexistent()
        {
            ApproveOrRefuseOnRequestReservationRequest request = new ApproveOrRefuseOnRequestReservationRequest
            {
                ReservationId = 1,
                IsApprove = true,
                UserId = 1,
                ChannelId = 999,
                TransactionId = ""
            };
            var result = _reservationManagerMock.Object.ApproveOrRefuseOnRequestReservation(request);

            Assert.AreEqual(result.Status, Reservation.BL.Contracts.Responses.Status.Fail);
            Assert.AreNotEqual(0, result.Errors.Count);
            Assert.IsTrue(result.Errors.Where(e => e.ErrorCode == (int)Errors.InvalidChannel).Any());
            Assert.IsTrue(result.Errors.Where(e => e.ErrorType == "InvalidChannel").Any());
        }


        [TestMethod]
        [TestCategory("ApproveOrRefuseOnRequest")]
        public void NotPullChannelApproveTest()
        {
            ReservationBaseRequest innerRequest = null;
            _reservationManagerMock.Setup(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()))
                .Callback((ReservationBaseRequest iRequest, Reservation.BL.Constants.ReservationAction action) =>
                    {
                        innerRequest = iRequest;
                    })
                .Returns((ReservationBaseRequest iRequest, Reservation.BL.Constants.ReservationAction action) =>
                    new OB.Reservation.BL.Contracts.Responses.ReservationBaseResponse()
                    {
                        Status = OB.Reservation.BL.Contracts.Responses.Status.Success
                    }
                );


            ApproveOrRefuseOnRequestReservationRequest request = new ApproveOrRefuseOnRequestReservationRequest
            {
                ReservationId = 1,
                IsApprove = true,
                UserId = 1,
                ChannelId = 1,
                TransactionId = ""
            };
            var result = _reservationManagerMock.Object.ApproveOrRefuseOnRequestReservation(request);
            _reservationManagerMock.Verify(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()), Times.Once());

            Assert.AreEqual(result.Status, Reservation.BL.Contracts.Responses.Status.Success);
            Assert.AreEqual(innerRequest.ChannelId, request.ChannelId);
            Assert.AreEqual(innerRequest.UserId, request.UserId);
            Assert.AreEqual(innerRequest.TransactionAction, Reservation.BL.Constants.ReservationTransactionAction.Commit);
            Assert.AreEqual(innerRequest.RuleType, Reservation.BL.Constants.RuleType.Omnibees);
            Assert.AreEqual(innerRequest.TransactionId, request.TransactionId);
            Assert.AreEqual(innerRequest.TransactionType, Reservation.BL.Constants.ReservationTransactionType.A);
        }

        [TestMethod]
        [TestCategory("ApproveOrRefuseOnRequest")]
        public void NotPullChannelCancelTest()
        {
            CancelReservationRequest innerRequest = null;
            _reservationManagerMock.Setup(c => c.CancelReservation(It.IsAny<CancelReservationRequest>()))
                .Callback((CancelReservationRequest iRequest) =>
                {
                    innerRequest = iRequest;
                })
                .Returns((CancelReservationRequest iRequest) =>
                    new OB.Reservation.BL.Contracts.Responses.CancelReservationResponse()
                    {
                        Status = OB.Reservation.BL.Contracts.Responses.Status.Success
                    }
                );


            ApproveOrRefuseOnRequestReservationRequest request = new ApproveOrRefuseOnRequestReservationRequest
            {
                ReservationId = 1,
                IsApprove = false,
                UserId = 1,
                ChannelId = 1,
                TransactionId = ""
            };

            var result = _reservationManagerMock.Object.ApproveOrRefuseOnRequestReservation(request);
            _reservationManagerMock.Verify(c => c.CancelReservation(It.IsAny<CancelReservationRequest>()), Times.Once());

            Assert.AreEqual(result.Status, Reservation.BL.Contracts.Responses.Status.Success);
            Assert.AreEqual(innerRequest.ReservationUID, request.ReservationId);
            Assert.AreEqual(innerRequest.UserId, request.UserId);
            Assert.AreEqual(innerRequest.ChannelId, request.ChannelId);
            Assert.AreEqual(innerRequest.TransactionId, request.TransactionId);
        }

        [TestMethod]
        [TestCategory("ApproveOrRefuseOnRequest")]
        public void PullChannelApproveTest()
        {
            ReservationBaseRequest innerRequest = null;
            _reservationManagerMock.Setup(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()))
                .Callback((ReservationBaseRequest iRequest, Reservation.BL.Constants.ReservationAction action) =>
                {
                    innerRequest = iRequest;
                })
                .Returns((ReservationBaseRequest iRequest, Reservation.BL.Constants.ReservationAction action) =>
                    new OB.Reservation.BL.Contracts.Responses.ReservationBaseResponse()
                    {
                        Status = OB.Reservation.BL.Contracts.Responses.Status.Success
                    }
                );


            ApproveOrRefuseOnRequestReservationRequest request = new ApproveOrRefuseOnRequestReservationRequest
            {
                ReservationId = 1,
                IsApprove = true,
                UserId = 1,
                ChannelId = 2,
                TransactionId = ""
            };

            var result = _reservationManagerMock.Object.ApproveOrRefuseOnRequestReservation(request);
            _reservationManagerMock.Verify(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()), Times.Once());

            Assert.AreEqual(result.Status, Reservation.BL.Contracts.Responses.Status.Success);
            Assert.AreEqual(innerRequest.ChannelId, request.ChannelId);
            Assert.AreEqual(innerRequest.UserId, request.UserId);
            Assert.AreEqual(innerRequest.TransactionAction, Reservation.BL.Constants.ReservationTransactionAction.Commit);
            Assert.AreEqual(innerRequest.RuleType, Reservation.BL.Constants.RuleType.Omnibees);
            Assert.AreEqual(innerRequest.TransactionId, request.TransactionId);
            Assert.AreEqual(innerRequest.TransactionType, Reservation.BL.Constants.ReservationTransactionType.A);
        }

        [TestMethod]
        [TestCategory("ApproveOrRefuseOnRequest")]
        public void PullChannelCancelTest()
        {
            ReservationBaseRequest innerRequest = null;
            _reservationManagerMock.Setup(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()))
                .Callback((ReservationBaseRequest iRequest, Reservation.BL.Constants.ReservationAction action) =>
                {
                    innerRequest = iRequest;
                })
                .Returns((ReservationBaseRequest iRequest, Reservation.BL.Constants.ReservationAction action) =>
                    new OB.Reservation.BL.Contracts.Responses.ReservationBaseResponse()
                    {
                        Status = OB.Reservation.BL.Contracts.Responses.Status.Success
                    }
                );


            ApproveOrRefuseOnRequestReservationRequest request = new ApproveOrRefuseOnRequestReservationRequest
            {
                ReservationId = 1,
                IsApprove = false,
                UserId = 1,
                ChannelId = 2,
                TransactionId = ""
            };

            var result = _reservationManagerMock.Object.ApproveOrRefuseOnRequestReservation(request);
            _reservationManagerMock.Verify(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()), Times.Once());

            Assert.AreEqual(result.Status, Reservation.BL.Contracts.Responses.Status.Success);
            Assert.AreEqual(innerRequest.ChannelId, request.ChannelId);
            Assert.AreEqual(innerRequest.UserId, request.UserId);
            Assert.AreEqual(innerRequest.TransactionAction, Reservation.BL.Constants.ReservationTransactionAction.Ignore);
            Assert.AreEqual(innerRequest.RuleType, Reservation.BL.Constants.RuleType.Omnibees);
            Assert.AreEqual(innerRequest.TransactionId, request.TransactionId);
            Assert.AreEqual(innerRequest.TransactionType, Reservation.BL.Constants.ReservationTransactionType.A);
        }

        [TestMethod]
        [TestCategory("ApproveOrRefuseOnRequest")]
        public void NotPullChannelApproveErrorTest()
        {
            _reservationManagerMock.Setup(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()))
                .Returns((ReservationBaseRequest iRequest, Reservation.BL.Constants.ReservationAction action) =>
                    new OB.Reservation.BL.Contracts.Responses.ReservationBaseResponse()
                    {
                        Errors = new List<OB.Reservation.BL.Contracts.Responses.Error>
                        {
                            new OB.Reservation.BL.Contracts.Responses.Error(),
                        },
                        Warnings = new List<OB.Reservation.BL.Contracts.Responses.Warning>
                        {
                            new OB.Reservation.BL.Contracts.Responses.Warning(),
                            new OB.Reservation.BL.Contracts.Responses.Warning()
                        },
                        Status = OB.Reservation.BL.Contracts.Responses.Status.Fail
                    }
                );


            ApproveOrRefuseOnRequestReservationRequest request = new ApproveOrRefuseOnRequestReservationRequest
            {
                ReservationId = 1,
                IsApprove = true,
                UserId = 1,
                ChannelId = 1,
                TransactionId = ""
            };

            var result = _reservationManagerMock.Object.ApproveOrRefuseOnRequestReservation(request);
            _reservationManagerMock.Verify(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()), Times.Once());
            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Warnings.Count, 2);
            Assert.AreEqual(result.Status, OB.Reservation.BL.Contracts.Responses.Status.Fail);
        }

        [TestMethod]
        [TestCategory("ApproveOrRefuseOnRequest")]
        public void NotPullChannelCancelErrorTest()
        {
            _reservationManagerMock.Setup(c => c.CancelReservation(It.IsAny<CancelReservationRequest>()))
                .Returns((CancelReservationRequest iRequest) =>
                    new OB.Reservation.BL.Contracts.Responses.CancelReservationResponse()
                    {
                        Errors = new List<OB.Reservation.BL.Contracts.Responses.Error>
                        {
                            new OB.Reservation.BL.Contracts.Responses.Error(),
                        },
                        Warnings = new List<OB.Reservation.BL.Contracts.Responses.Warning>
                        {
                            new OB.Reservation.BL.Contracts.Responses.Warning(),
                            new OB.Reservation.BL.Contracts.Responses.Warning()
                        },
                        Status = OB.Reservation.BL.Contracts.Responses.Status.Fail
                    }
                );


            ApproveOrRefuseOnRequestReservationRequest request = new ApproveOrRefuseOnRequestReservationRequest
            {
                ReservationId = 1,
                IsApprove = false,
                UserId = 1,
                ChannelId = 1,
                TransactionId = ""
            };

            var result = _reservationManagerMock.Object.ApproveOrRefuseOnRequestReservation(request);
            _reservationManagerMock.Verify(c => c.CancelReservation(It.IsAny<CancelReservationRequest>()), Times.Once());
            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Warnings.Count, 2);
            Assert.AreEqual(result.Status, OB.Reservation.BL.Contracts.Responses.Status.Fail);
        }

        [TestMethod]
        [TestCategory("ApproveOrRefuseOnRequest")]
        public void PullChannelApproveErrorTest()
        {
            _reservationManagerMock.Setup(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()))
                .Returns((ReservationBaseRequest iRequest, Reservation.BL.Constants.ReservationAction action) =>
                    new OB.Reservation.BL.Contracts.Responses.ReservationBaseResponse()
                    {
                        Errors = new List<OB.Reservation.BL.Contracts.Responses.Error>
                        {
                            new OB.Reservation.BL.Contracts.Responses.Error(),
                        },
                        Warnings = new List<OB.Reservation.BL.Contracts.Responses.Warning>
                        {
                            new OB.Reservation.BL.Contracts.Responses.Warning(),
                            new OB.Reservation.BL.Contracts.Responses.Warning()
                        },
                        Status = OB.Reservation.BL.Contracts.Responses.Status.Fail
                    }
                );


            ApproveOrRefuseOnRequestReservationRequest request = new ApproveOrRefuseOnRequestReservationRequest
            {
                ReservationId = 1,
                IsApprove = true,
                UserId = 1,
                ChannelId = 2,
                TransactionId = ""
            };

            var result = _reservationManagerMock.Object.ApproveOrRefuseOnRequestReservation(request);
            _reservationManagerMock.Verify(c => c.ReservationCoordinator(It.IsAny<ReservationBaseRequest>(), It.IsAny<Reservation.BL.Constants.ReservationAction>()), Times.Once());
            Assert.AreEqual(result.Errors.Count, 1);
            Assert.AreEqual(result.Warnings.Count, 2);
            Assert.AreEqual(result.Status, OB.Reservation.BL.Contracts.Responses.Status.Fail);
        }
    }
}
