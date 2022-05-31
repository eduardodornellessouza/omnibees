using Newtonsoft.Json;
using OB.Domain.Reservations;
using OB.Reservation.BL.Contracts.Data.CRM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Helper
{
    [Serializable]
    public class ReservationInputData
    {
        public OB.Domain.Reservations.Reservation reservationDetail;
        public Guest guest;
        public List<ReservationRoom> reservationRooms;
        public List<ReservationRoomDetail> reservationRoomDetails;
        public List<long> guestActivity;
        public List<ReservationRoomExtra> reservationRoomExtras;
        public List<ReservationRoomChild> reservationRoomChild;
        public ReservationPaymentDetail reservationPaymentDetail;
        public List<ReservationRoomExtrasSchedule> reservationExtraSchedule;
        public List<ReservationRoomExtrasAvailableDate> reservationRoomExtrasAvailableDates;
        public Reservation.BL.Contracts.Data.Reservations.ReservationsAdditionalData reservationsAdditionalData;

        public ReservationInputData(bool detailOnly = false)
        {
            reservationDetail = new OB.Domain.Reservations.Reservation();
            reservationRooms = new List<ReservationRoom>();
            reservationRoomDetails = new List<ReservationRoomDetail>();
            if(!detailOnly)
                guest = new Guest();
            guestActivity = new List<long>();
            reservationRoomExtras = new List<ReservationRoomExtra>();
            reservationRoomChild = new List<ReservationRoomChild>();
            reservationPaymentDetail = null;
            reservationExtraSchedule = new List<ReservationRoomExtrasSchedule>();
            reservationRoomExtrasAvailableDates = new List<ReservationRoomExtrasAvailableDate>();
        }
    }

   [Serializable]
    public class ReservationData
    {
        public OB.Domain.Reservations.Reservation reservationDetail;
        public Guest guest;
        public List<ReservationRoom> reservationRooms;
        public List<ReservationRoomDetail> reservationRoomDetails;
        public List<ReservationRoomExtra> reservationRoomExtras;
        public List<ReservationRoomChild> reservationRoomChild;
        public ReservationPaymentDetail reservationPaymentDetail;
        public List<ReservationPartialPaymentDetail> reservationPartialPaymentDetail;
        public List<ReservationRoomExtrasSchedule> reservationExtraSchedule;
        public List<ReservationRoomExtrasAvailableDate> reservationRoomExtrasAvailableDates;
        public List<GuestActivity> guestActivityObj;
        public List<Test.Domain.CRM.PropertyQueue> expectedEmailList;
        public ReservationsAdditionalData reservationsAdditionalData;

        public ReservationData()
        {
            reservationDetail = new OB.Domain.Reservations.Reservation();
            reservationRooms = new List<ReservationRoom>();
            reservationRoomDetails = new List<ReservationRoomDetail>();
            guest = new Guest();
            guestActivityObj = new List<GuestActivity>();
            reservationRoomExtras = new List<ReservationRoomExtra>();
            reservationRoomChild = new List<ReservationRoomChild>();
            reservationPaymentDetail = null;
            reservationExtraSchedule = new List<ReservationRoomExtrasSchedule>();
            reservationRoomExtrasAvailableDates = new List<ReservationRoomExtrasAvailableDate>();
            expectedEmailList = new List<Test.Domain.CRM.PropertyQueue>();
            reservationPartialPaymentDetail = new List<ReservationPartialPaymentDetail>();            
        }
    }

   static class Cloner
   {
       public static object Clone(object objToClone)
       {
           // Don't serialize a null object, simply return the default for that object
           if (Object.ReferenceEquals(objToClone, null))
           {
               return default(object);
           }

           var serializationSettings = new JsonSerializerSettings()
           {
               ReferenceLoopHandling = ReferenceLoopHandling.Ignore
           };

           var serializedObj = JsonConvert.SerializeObject(objToClone, serializationSettings);

           return JsonConvert.DeserializeObject(serializedObj, objToClone.GetType());
        
       }
   }
}
