using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using OB.Api.Core;
using OB.BL.Operations.Interfaces;
using OB.BL.Operations.Test.Helper;
using OB.DL.Common;
using OB.DL.Common.Interfaces;
using OB.Reservation.BL.Contracts.Requests;
using OB.Reservation.BL.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OB.BL.Operations.Test.Reservations
{
    [TestClass]
    public class ReservationManagerPOCOLoadTest : BaseTest
    {

        private List<MetadataWorkspace> workspaces = new List<MetadataWorkspace>();


        [Ignore]
        [TestMethod, TestCategory("Load Tests")]
        [DeploymentItem("./Databases_WithData", "TestInsertReservation_30_Threads")]
        [DeploymentItem("./DL")]
        public void TestInsertReservation_30_Threads()
        {
            // NOTE: This test should be reviewed
            //       If it should test threads, it should use threads directly, not tasks

            int minWorkerThreads, minCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            int maxWorkerThreads, maxCompletionPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
            try
            {
                ThreadPool.SetMinThreads(1, 1);
                ThreadPool.SetMaxThreads(Int32.MaxValue, Int32.MaxValue);
                ThreadPool.SetMinThreads(10, 20);
                ThreadPool.SetMaxThreads(20, 50);



                IUnityContainer container = new UnityContainer();

                var dataAccessLayerModule = new DataAccessLayerModule();



                var businessLayerModule = new BusinessLayerModule();
                container = container.AddExtension(dataAccessLayerModule);
                container = container.AddExtension(businessLayerModule);

                dataAccessLayerModule.WarmUp();
                businessLayerModule.WarmUp();

                Thread.Sleep(3000); // WHY?

                var sessionFactory = container.Resolve<ISessionFactory>();



                // arrange
                var reservationBuilder = new ReservationBuilderSimple().WithNewGuest().WithOneRoom().WithOneDay().WithCreditCardPayment();


                // act            
                var service = container.Resolve<IReservationManagerPOCO>();

                long reservationId = -1;
                reservationId = service.InsertReservation(
                    new Reservation.BL.Contracts.Requests.InsertReservationRequest
                    {
                        Guest = reservationBuilder.guest,
                        Reservation = reservationBuilder.reservationDetail,
                        GuestActivities = reservationBuilder.guestActivity,
                        ReservationRooms = reservationBuilder.reservationRooms,
                        ReservationRoomDetails = reservationBuilder.reservationRoomDetails,
                        ReservationRoomExtras = reservationBuilder.reservationRoomExtras,
                        ReservationRoomChilds = reservationBuilder.reservationRoomChild,
                        ReservationPaymentDetail = reservationBuilder.reservationPaymentDetail,
                        ReservationExtraSchedules = reservationBuilder.reservationExtraSchedule,
                        HandleCancelationCost = true,
                        HandleDepositCost = true,
                        ValidateAllotment = true,
                        ReservationsAdditionalData = null,
                        ValidateGuarantee = false,
                        TransactionId = "",
                        UsePaymentGateway = true,
                        Version = null,
                        RuleType = null
                    }).Result;


                service.WaitForAllBackgroundWorkers();


                int numberOfIterations = 5;

                var reservationBuilders = new List<ReservationBuilderSimple>(numberOfIterations);

                for (int i = 0; i < numberOfIterations; i++)
                {
                    reservationBuilders.Add(new ReservationBuilderSimple().WithNewGuest().WithOneRoom().WithOneDay().WithCreditCardPayment());
                }

                var tasks = new List<Task>(numberOfIterations);
                var times = new System.Collections.Concurrent.ConcurrentBag<double>();


                var totalTimeBefore = DateTime.Now.Ticks;

                for (int i = 0; i < numberOfIterations; i++)
                {
                    var builder = reservationBuilders[i];

                    var task = Task.Factory.StartNew(() =>
                    {
                        var localBuilder = builder;//state as ReservationBuilder;
                        var ticksBefore = DateTime.Now.Ticks;

                        reservationId = service.InsertReservation(
                            new Reservation.BL.Contracts.Requests.InsertReservationRequest
                            {
                                Guest = localBuilder.guest,
                                Reservation = localBuilder.reservationDetail,
                                GuestActivities = localBuilder.guestActivity,
                                ReservationRooms = localBuilder.reservationRooms,
                                ReservationRoomDetails = localBuilder.reservationRoomDetails,
                                ReservationRoomExtras = localBuilder.reservationRoomExtras,
                                ReservationRoomChilds = localBuilder.reservationRoomChild,
                                ReservationPaymentDetail = localBuilder.reservationPaymentDetail,
                                ReservationExtraSchedules = localBuilder.reservationExtraSchedule,
                                HandleCancelationCost = true,
                                HandleDepositCost = true,
                                ValidateAllotment = true,
                                ReservationsAdditionalData = null,
                                ValidateGuarantee = false,
                                TransactionId = "",
                                UsePaymentGateway = true,
                                Version = null,
                                RuleType = null
                            }).Result;

                        var ticksAfter = DateTime.Now.Ticks;

                        var ellapsedMilliseconds = TimeSpan.FromTicks(ticksAfter - ticksBefore).TotalMilliseconds;

                        times.Add(ellapsedMilliseconds);

                    }, CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    tasks.Add(task);
                }




                Task.WaitAll(tasks.ToArray());

                var totalTimeAfter = DateTime.Now.Ticks;

                double avgTimeMs = times.Average();
                double minTimeMs = times.Min();
                double maxTimeMs = times.Max();


                double acceptanceLevel = ((double)times.Count(x => x < 2000)) / (double)times.Count;

                double totalTimeMs = times.Sum();

                double totalEllapsedTime = TimeSpan.FromTicks(totalTimeAfter - totalTimeBefore).TotalMilliseconds;

                Debug.WriteLine("AcceptanceLevel (<2000ms): " + Math.Round(acceptanceLevel * 100.0, 2) + "%");
                Debug.WriteLine("AvgTime per InsertReservation:" + avgTimeMs + "ms");
                Debug.WriteLine("MinTime per InsertReservation:" + minTimeMs + "ms");
                Debug.WriteLine("MaxTime per InsertReservation:" + maxTimeMs + "ms");
                Thread.Sleep(1000); // WHY?


                var repositoryFactory = container.Resolve<IRepositoryFactory>();
                var unitOfWork = sessionFactory.GetUnitOfWork(OB.Domain.Reservations.Reservation.DomainScope);
                var reservationRepo = repositoryFactory.GetRepository<OB.Domain.Reservations.Reservation>(unitOfWork);

                var reservation = reservationRepo.Get(reservationId);


                Assert.IsNotNull(reservation, "Reservation was not created in the Database");
            }
            finally
            {
                ThreadPool.SetMinThreads(1, 1);
                ThreadPool.SetMaxThreads(Int32.MaxValue, Int32.MaxValue);
                ThreadPool.SetMinThreads(minWorkerThreads, minCompletionPortThreads);
                ThreadPool.SetMaxThreads(maxWorkerThreads, maxCompletionPortThreads);
            }
        }

        [TestMethod]
        [Ignore]
        [TestCategory("Load Tests")]
        public void TestLocalhostIISPerformance_InsertReservation()
        {
            WebClient client = new WebClient();

            var serializer = new JsonSerializer();

            var times = new List<double>();
            var requests = new List<InsertReservationRequest>();

            var responses = new List<InsertReservationResponse>();

            for (int i = 0; i < 50; i++)
            {
                var builder = new ReservationBuilderSimple().WithNewGuest().WithOneRoom().WithOneDay().WithCreditCardPayment();


                var request = new InsertReservationRequest
                {
                    Guest = builder.guest,
                    GuestActivities = builder.guestActivity,
                    RequestGuid = Guid.NewGuid(),
                    Reservation = builder.reservationDetail,
                    ReservationExtraSchedules = builder.reservationExtraSchedule,
                    ReservationPaymentDetail = builder.reservationPaymentDetail,
                    ReservationRoomChilds = builder.reservationRoomChild,
                    ReservationRoomDetails = builder.reservationRoomDetails,
                    ReservationRoomExtras = builder.reservationRoomExtras,
                    ReservationRooms = builder.reservationRooms
                };
                requests.Add(request);
                var ms = new MemoryStream();
                var writer = new BsonWriter(ms);
                serializer.Serialize(writer, request);

                var before = DateTime.Now;
                client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");

                byte[] binaryResult = client.UploadData("http://localhost/OB.REST.Services/api/Reservation/InsertReservation", "POST", ms.ToArray());


                var after = DateTime.Now;

                times.Add(TimeSpan.FromTicks(after.Ticks - before.Ticks).TotalMilliseconds);

                var reader = new BsonReader(new MemoryStream(binaryResult));
                InsertReservationResponse response = serializer.Deserialize<InsertReservationResponse>(reader);
                responses.Add(response);

            }

            double avgTime = times.Average();
            double maxTime = times.Max();
            double minTime = times.Min();
            double totalTime = times.Sum();

        }



        [TestMethod]
        [TestCategory("Load Tests")]
        [Ignore]
        public void TestLocalhostIISPerformance_InsertReservation_5Threads_SameHotel_SameRoomType_SameRate()
        {
            // NOTE: This test should be reviewed
            //       If it should test threads, it should use threads directly, not tasks

            var serializer = new JsonSerializer();

            var times = new List<double>();
            var requests = new List<InsertReservationRequest>();

            var responses = new List<InsertReservationResponse>();


            var tasks = new List<Task>();

            for (int tIndex = 0; tIndex < 5; tIndex++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    var client = new WebClient();

                    for (int i = 0; i < 50; i++)
                    {
                        var builder = new ReservationBuilderSimple().WithNewGuest().WithOneRoom().WithOneDay().WithCreditCardPayment();


                        var request = new InsertReservationRequest
                        {
                            Guest = builder.guest,
                            GuestActivities = builder.guestActivity,
                            RequestGuid = Guid.NewGuid(),
                            Reservation = builder.reservationDetail,
                            ReservationExtraSchedules = builder.reservationExtraSchedule,
                            ReservationPaymentDetail = builder.reservationPaymentDetail,
                            ReservationRoomChilds = builder.reservationRoomChild,
                            ReservationRoomDetails = builder.reservationRoomDetails,
                            ReservationRoomExtras = builder.reservationRoomExtras,
                            ReservationRooms = builder.reservationRooms
                        };
                        requests.Add(request);
                        var ms = new MemoryStream();
                        var writer = new BsonWriter(ms);
                        serializer.Serialize(writer, request);

                        var before = DateTime.Now;
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");

                        byte[] binaryResult = client.UploadData("http://localhost/Omnibees.Services/api/Reservation/InsertReservation", "POST", ms.ToArray());


                        var after = DateTime.Now;

                        times.Add(TimeSpan.FromTicks(after.Ticks - before.Ticks).TotalMilliseconds);

                        var reader = new BsonReader(new MemoryStream(binaryResult));
                        InsertReservationResponse response = serializer.Deserialize<InsertReservationResponse>(reader);
                        responses.Add(response);

                    }
                }, CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default);
                tasks.Add(task);
            }


            Task.WaitAll(tasks.ToArray());

            double avgTime = times.Average();
            double maxTime = times.Max();
            double minTime = times.Min();
            double totalTime = times.Sum();

        }

        [TestMethod]
        [TestCategory("Load Tests")]
        [Ignore]
        public void TestLocalhostIISPerformance_FindReservationsWithDates()
        {
            WebClient client = new WebClient();

            var serializer = new JsonSerializer();

            var times = new List<double>();
            var requests = new List<InsertReservationRequest>();

            var responses = new List<InsertReservationResponse>();

            for (int i = 0; i < 100; i++)
            {
                var findReservationRequestObj = new OB.BL.Contracts.Requests.ListReservationRequest
                {
                    RequestGuid = Guid.NewGuid(),

                    DateFrom = new DateTime(2012, 7, 11),
                    DateTo = new DateTime(2012, 7, 12),
                    IncludeGuests = false,
                    IncludeReservationPartialPaymentDetails = false,
                    IncludeReservationRoomChilds = false,
                    IncludeReservationPaymentDetail = false,
                    IncludeReservationRoomDetails = false,
                    IncludeReservationRoomExtras = false,
                    IncludeReservationRooms = false
                };




                var ms = new MemoryStream();
                var writer = new BsonWriter(ms);
                serializer.Serialize(writer, findReservationRequestObj);

                var before = DateTime.Now;
                client.Headers.Add(HttpRequestHeader.ContentType, "application/bson");

                byte[] binaryResult = client.UploadData("http://localhost/Omnibees.Services/api/Reservation/Find", "POST", ms.ToArray());


                var after = DateTime.Now;

                times.Add(TimeSpan.FromTicks(after.Ticks - before.Ticks).TotalMilliseconds);

                var reader = new BsonReader(new MemoryStream(binaryResult));
                ListReservationResponse response = serializer.Deserialize<ListReservationResponse>(reader);
                Assert.IsTrue(response.Result != null);
                Assert.IsTrue(response.Result.Count == 4);
                Assert.IsTrue(response.Status == Status.Success);
                Assert.IsTrue(response.Result.First().UID == 3102);
                Assert.IsTrue(response.Result.Last().UID == 3196);

            }

            double avgTime = times.Average();
            double maxTime = times.Max();
            double minTime = times.Min();
            double totalTime = times.Sum();

            Assert.IsTrue(avgTime < 60);
        }
    }
}
