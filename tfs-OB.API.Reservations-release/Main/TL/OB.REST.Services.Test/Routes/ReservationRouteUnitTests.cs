using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.REST.Services.Controllers;
using System.Net;
using OB.REST.Services.Helper;
using OB.Reservation.BL.Contracts.Requests;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Bson;
using OB.Reservation.BL.Contracts.Responses;

using System.Web.Http;
using System.Net.Http;
using System.Threading;
using OB.REST.Services.Test.Helper;
using OB.REST.Services;

namespace OB.BackOffice.Web.Tests.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class ReservationRouteUnitTests
    {
        public ReservationRouteUnitTests()
        {
            _config = new HttpConfiguration();
            _config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            WebApiConfig.MapRoutes(_config);
        }

        private HttpConfiguration _config;

     
    

        [TestMethod, TestCategory("Routes Unit Tests")]
        public void Test_ReservationRoute_List_HttpPost()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/Reservation/ListReservations");

            var routeTester = new RouteTester(_config, request);

            Assert.AreEqual(typeof(ReservationController), routeTester.GetControllerType());
            Assert.AreEqual(ReflectionHelper.GetMethodName((ReservationController p) => p.ListReservations(default(ListReservationRequest))), routeTester.GetActionName());
        }

  
        [TestMethod, TestCategory("Routes Unit Tests")]
        public void Test_ReservationRoute_InsertReservation_HttpPost()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/Reservation/InsertReservation");

            var routeTester = new RouteTester(_config, request);

            Assert.AreEqual(typeof(ReservationController), routeTester.GetControllerType());
            Assert.AreEqual(ReflectionHelper.GetMethodName((ReservationController p) => p.InsertReservation(default(InsertReservationRequest))), routeTester.GetActionName());
        }

 


        [TestMethod, TestCategory("Routes Unit Tests")]
        public void Test_ReservationRoute_UpdateReservation_HttpPost()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/Reservation/UpdateReservation");

            var routeTester = new RouteTester(_config, request);

            Assert.AreEqual(typeof(ReservationController), routeTester.GetControllerType());
            Assert.AreEqual(ReflectionHelper.GetMethodName((ReservationController p) => p.UpdateReservation(default(UpdateReservationRequest))), routeTester.GetActionName());
        }

        [TestMethod, TestCategory("Routes Unit Tests")]
        public void Test_ReservationRoute_CancelReservation_HttpPost()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/Reservation/CancelReservation");

            var routeTester = new RouteTester(_config, request);

            Assert.AreEqual(typeof(ReservationController), routeTester.GetControllerType());
            Assert.AreEqual(ReflectionHelper.GetMethodName((ReservationController p) => p.CancelReservation(default(CancelReservationRequest))), routeTester.GetActionName());
        }

        //[TestMethod, TestCategory("Routes Unit Tests")]
        //public void Test_ReservationRoute_CancelReservation_HttpDelete()
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Delete, "http://localhost/api/Reservation/CancelReservation");
        //
        //    var routeTester = new RouteTester(_config, request);
        //
        //    Assert.AreEqual(typeof(ReservationController), routeTester.GetControllerType());
        //    Assert.AreEqual(ReflectionHelper.GetMethodName((ReservationController p) => p.CancelReservation(default(CancelReservationRequest))), routeTester.GetActionName());
        //}

        //[TestMethod, TestCategory("Routes Unit Tests")]
        //public void Test_ReservationRoute_Get_HttpGet()
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Reservation/Get/1723");

        //    var routeTester = new RouteTester(_config, request);

        //    Assert.AreEqual(typeof(ReservationController), routeTester.GetControllerType());
        //    Assert.AreEqual(ReflectionHelper.GetMethodName((ReservationController p) => p.Get(default(long))), routeTester.GetActionName());
        //}



    }
}
