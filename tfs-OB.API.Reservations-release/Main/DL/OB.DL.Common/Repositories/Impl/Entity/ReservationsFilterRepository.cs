using Dapper;
using OB.Domain.Reservations;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OB.Api.Core;
using OB.DL.Common.Impl;
using OB.DL.Common.Repositories.Interfaces.Entity;
using System;

namespace OB.DL.Common.Repositories.Impl.Entity
{
    internal class ReservationsFilterRepository : Repository<ReservationFilter>, IReservationsFilterRepository
    {
        public ReservationsFilterRepository(IObjectContext context)
            : base(context)
        {
        }

        public IEnumerable<ReservationFilter> FindByReservationUIDs(List<long> reservation_UIDs)
        {
            var result = GetQuery();

            return result.Where(x => reservation_UIDs.Contains(x.UID)).Include(x => x.ReservationRoomFilters);
        }

        /// <summary>
        /// Update reservationfilter/reservationroomfilter to new status
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="reservationStatus"></param>
        /// <returns></returns>
        public void UpdateReservationFilterStatus(long reservationId, int reservationStatus, long? userId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@reservationStatus", reservationStatus, DbType.Int64);
            parameters.Add("@reservationId", reservationId, DbType.Int64);
            parameters.Add("@modifyBy", userId, DbType.Int64);

            Context.Context.Database.Connection.ExecuteScalar<int>("UpdateReservationFilterStatus", parameters, null, null, CommandType.StoredProcedure);
        }


        public IEnumerable<long> FindByCriteria(ListReservationFilterCriteria request, out int totalRecords, bool returnTotal = false)
        {
            var query = GetQuery();

            totalRecords = -1;

            if (request.ReservationUIDs != null && request.ReservationUIDs.Count > 0)
            {
                if (request.ReservationUIDs.Count == 1)
                {
                    var reservationUid = request.ReservationUIDs.First();
                    query = query.Where(x => x.UID == reservationUid);
                }
                else query = query.Where(x => request.ReservationUIDs.Contains(x.UID));
            }

            if (request.ReservationNumbers != null && request.ReservationNumbers.Count > 0)
            {
                if (request.ReservationNumbers.Count == 1)
                {
                    var reservationNumber = request.ReservationNumbers.First();
                    query = query.Where(x => x.Number == reservationNumber);
                }
                else query = query.Where(x => request.ReservationNumbers.Contains(x.Number));
            }

            if (request.CreatedDate.HasValue)
                query = query.Where(x => x.CreatedDate == request.CreatedDate.Value);

            if (request.PropertyUIDs != null && request.PropertyUIDs.Count > 0)
                query = query.Where(x => request.PropertyUIDs.Contains(x.PropertyUid));

            if (request.PropertyNames != null && request.PropertyNames.Count > 0)
                query = query.Where(x => request.PropertyNames.Contains(x.PropertyName));

            if (request.ReservationNumbers != null && request.ReservationNumbers.Count > 0)
            {
                if (request.ReservationNumbers.Count == 1)
                {
                    var reservationNumber = request.ReservationNumbers.First();
                    query = query.Where(x => x.Number == reservationNumber);
                }
                else query = query.Where(x => request.ReservationNumbers.Contains(x.Number));
            }

            if (request.IsOnRequest.HasValue)
                query = query.Where(x => x.IsOnRequest == request.IsOnRequest.Value);

            if (request.IsReaded.HasValue)
                query = query.Where(x => x.IsReaded == request.IsReaded.Value);

            if (request.ModifiedFrom.HasValue)
                query = query.Where(x => x.ModifiedDate.Value >= request.ModifiedFrom.Value);

            if (request.ModifiedTo.HasValue)
            {
                request.ModifiedTo = request.ModifiedTo.Value.AddDays(1);
                query = query.Where(x => x.ModifiedDate.Value < request.ModifiedTo.Value);
            }

            if (!string.IsNullOrEmpty(request.GuestName))
                query = query.Where(x => x.GuestName == request.GuestName);

            if (request.NumberOfNights.HasValue)
                query = query.Where(x => x.NumberOfNights == request.NumberOfNights.Value);

            if (request.NumberOfAdults.HasValue)
                query = query.Where(x => x.NumberOfAdults == request.NumberOfAdults.Value);

            if (request.NumberOfChildren.HasValue)
                query = query.Where(x => x.NumberOfChildren == request.NumberOfChildren.Value);

            if (request.NumberOfRooms.HasValue)
                query = query.Where(x => x.NumberOfRooms == request.NumberOfRooms.Value);

            if (request.GuestIds != null && request.GuestIds.Any())
                query = query.Where(x => x.Guest_UID.HasValue && request.GuestIds.Contains(x.Guest_UID.Value));

            if (request.TpiIds != null && request.TpiIds.Any())
                query = query.Where(x => x.TPI_UID.HasValue && request.TpiIds.Contains(x.TPI_UID.Value));

            if (request.PaymentTypeIds != null && request.PaymentTypeIds.Any())
                query =
                    query.Where(
                        x => x.PaymentTypeUid.HasValue && request.PaymentTypeIds.Contains(x.PaymentTypeUid.Value));

            if (request.ReservationStatus != null && request.ReservationStatus.Any())
                query =
                    query.Where(
                        x => x.Status.HasValue && request.ReservationStatus.Contains(x.Status.Value));

            if (request.TotalAmount.HasValue)
                query = query.Where(x => x.TotalAmount == request.TotalAmount);

            if (request.ExternalTotalAmount.HasValue)
                query = query.Where(x => x.ExternalTotalAmount == request.ExternalTotalAmount);

            if (request.ExternalCommissionValue.HasValue)
                query = query.Where(x => x.ExternalCommissionValue == request.ExternalCommissionValue);

            if (request.ExternalIsPaid.HasValue)
                query = query.Where(x => x.ExternalIsPaid == request.ExternalIsPaid.Value);

            if (request.ChannelUIDs != null && request.ChannelUIDs.Count > 0)
                query = query.Where(x => x.ChannelUid.HasValue && request.ChannelUIDs.Contains(x.ChannelUid.Value));

            if (request.ChannelNames != null && request.ChannelNames.Count > 0)
                query = query.Where(x => request.ChannelNames.Contains(x.ChannelName));

            if (request.TPINames != null && request.TPINames.Count > 0)
                query = query.Where(x => request.TPINames.Contains(x.TPI_Name));

            if (request.IsPaid == false)            
               query = query.Where(x => x.IsPaid == false || x.IsPaid == null);
            else if (request.IsPaid == true)  
                 query = query.Where(x => x.IsPaid == true);                

            if (request.ReservationDate.HasValue)
                query = query.Where(x => x.ReservationDate == request.ReservationDate.Value);

            if (request.ExternalChannelUids != null && request.ExternalChannelUids.Count > 0)
                query = query.Where(x => x.ExternalChannelUid.HasValue && request.ExternalChannelUids.Contains(x.ExternalChannelUid.Value));

            if (request.ExternalTPIUids != null && request.ExternalTPIUids.Count > 0)
                query = query.Where(x => x.ExternalTPIUid.HasValue && request.ExternalTPIUids.Contains(x.ExternalTPIUid.Value));

            if (request.ExternalNames != null && request.ExternalNames.Count > 0)
                query = query.Where(x => request.ExternalNames.Contains(x.ExternalName));

            if (request.PartnerIds != null && request.PartnerIds.Any())
                query = query.Where(x => x.PartnerUid.HasValue && request.PartnerIds.Contains((int) x.PartnerUid.Value));

            if (request.PartnerReservationNumbers != null && request.PartnerReservationNumbers.Any())
                query = query.Where(x => request.PartnerReservationNumbers.Contains(x.PartnerReservationNumber));

            if (request.DateFrom.HasValue)
                query = query.Where(x => x.CreatedDate >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
            {
                request.DateTo = request.DateTo.Value.AddDays(1);
                query = query.Where(x => x.CreatedDate < request.DateTo.Value);
            }

            if (request.EmployeeIds != null && request.EmployeeIds.Count > 0)
            {
                if (request.EmployeeIds.Count == 1)
                {
                    var EmployeeId = request.EmployeeIds.First();
                    query = query.Where(x => x.Employee_UID.HasValue && x.Employee_UID == EmployeeId);
                }
                else query = query.Where(x => x.Employee_UID.HasValue && request.EmployeeIds.Contains(x.Employee_UID.Value));
            }

            if (request.BigPullAuthRequestorUIDs != null && request.BigPullAuthRequestorUIDs.Count > 0)
            {
                if (request.BigPullAuthRequestorUIDs.Count == 1)
                {
                    var BigPullUID = request.BigPullAuthRequestorUIDs.First();
                    query = query.Where(x => x.BigPullAuthRequestor_UID.HasValue && x.BigPullAuthRequestor_UID == BigPullUID);
                }
                else query = query.Where(x => x.BigPullAuthRequestor_UID.HasValue && request.BigPullAuthRequestorUIDs.Contains(x.BigPullAuthRequestor_UID.Value));
            }

            if (request.BigPullAuthOwnerUIDs != null && request.BigPullAuthOwnerUIDs.Count > 0)
            {
                if (request.BigPullAuthOwnerUIDs.Count == 1)
                {
                    var BigPullUID = request.BigPullAuthOwnerUIDs.First();
                    query = query.Where(x => x.BigPullAuthOwner_UID.HasValue && x.BigPullAuthOwner_UID == BigPullUID);
                }
                else query = query.Where(x => x.BigPullAuthOwner_UID.HasValue && request.BigPullAuthOwnerUIDs.Contains(x.BigPullAuthOwner_UID.Value));
            }

            if (request.NestedFilters != null)
            {
                TreatRequestKendoFilters(request.NestedFilters);
                query = query.FilterBy(request.NestedFilters);
            }
            if (request.Filters != null && request.Filters.Any())
            {
                TreatRequestFilters(request.Filters);
                query = query.FilterBy(request.Filters);
            }

            if (request.Orders != null && request.Orders.Any())
            {
                var orders = request.Orders.Select(x =>
                            new OB.DL.Common.Filter.SortByInfo
                            {
                                OrderBy = x.OrderBy,
                                Direction = x.Direction,
                                Initial = x.Initial
                            }).ToList();

                TreatRequestOrders(orders);
                query = query.OrderBy(orders);
            }
            else
                query = query.OrderByDescending(x => x.UID);


            if (request.IncludeReservationRoomsFilter)
                query = query.Include(x => x.ReservationRoomFilters);

            if (request.CheckIn.HasValue)
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.CheckIn >= request.CheckIn.Value));

             if (request.CheckInTo.HasValue)
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.CheckIn <= request.CheckInTo.Value));

            if (request.CheckOut.HasValue)
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.CheckOut <= request.CheckOut.Value));

            if (request.CheckOutFrom.HasValue)
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.CheckOut >= request.CheckOutFrom.Value));

            if (request.ApplyDepositPolicy.HasValue)
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.ApplyDepositPolicy.Value
                                                                           &&
                                                                           (y.DepositCost.HasValue &&
                                                                            y.DepositCost.Value > 0 ||
                                                                            y.DepositNumberOfNight.HasValue &&
                                                                            y.DepositNumberOfNight > 0)));
            if (!string.IsNullOrEmpty(request.GuestName))
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.GuestName == request.GuestName));


            if (returnTotal)
                totalRecords = query.Count();

            if (request.PageIndex > 0 && request.PageSize > 0)
                query = query.Skip(request.PageIndex * request.PageSize);

            if (request.PageSize > 0)
                query = query.Take(request.PageSize);            

            return query.Select(x => x.UID);
        }

        public IEnumerable<ReservationFilter> FindReservationFilterByCriteria(ListReservationFilterCriteria request, out int totalRecords, bool returnTotal = false)
        {
            var query = GetQuery();

            totalRecords = -1;

            if (request.ReservationUIDs != null && request.ReservationUIDs.Count > 0)
            {
                if (request.ReservationUIDs.Count == 1)
                {
                    var reservationUid = request.ReservationUIDs.First();
                    query = query.Where(x => x.UID == reservationUid);
                }
                else query = query.Where(x => request.ReservationUIDs.Contains(x.UID));
            }

            if (request.ReservationNumbers != null && request.ReservationNumbers.Count > 0)
            {
                if (request.ReservationNumbers.Count == 1)
                {
                    var reservationNumber = request.ReservationNumbers.First();
                    query = query.Where(x => x.Number == reservationNumber);
                }
                else query = query.Where(x => request.ReservationNumbers.Contains(x.Number));
            }

            if (request.CreatedDate.HasValue)
                query = query.Where(x => x.CreatedDate == request.CreatedDate.Value);

            if (request.PropertyUIDs != null && request.PropertyUIDs.Count > 0)
                query = query.Where(x => request.PropertyUIDs.Contains(x.PropertyUid));

            if (request.PropertyNames != null && request.PropertyNames.Count > 0)
                query = query.Where(x => request.PropertyNames.Contains(x.PropertyName));

            if (request.ReservationNumbers != null && request.ReservationNumbers.Count > 0)
            {
                if (request.ReservationNumbers.Count == 1)
                {
                    var reservationNumber = request.ReservationNumbers.First();
                    query = query.Where(x => x.Number == reservationNumber);
                }
                else query = query.Where(x => request.ReservationNumbers.Contains(x.Number));
            }

            if (request.IsOnRequest.HasValue)
                query = query.Where(x => x.IsOnRequest == request.IsOnRequest.Value);

            if (request.IsReaded.HasValue)
                query = query.Where(x => x.IsReaded == request.IsReaded.Value);

            if (request.ModifiedFrom.HasValue)
                query = query.Where(x => x.ModifiedDate.Value >= request.ModifiedFrom.Value);

            if (request.ModifiedTo.HasValue)
            {
                request.ModifiedTo = request.ModifiedTo.Value.AddDays(1);
                query = query.Where(x => x.ModifiedDate.Value < request.ModifiedTo.Value);
            }

            if (!string.IsNullOrEmpty(request.GuestName))
                query = query.Where(x => x.GuestName == request.GuestName);

            if (request.NumberOfNights.HasValue)
                query = query.Where(x => x.NumberOfNights == request.NumberOfNights.Value);

            if (request.NumberOfAdults.HasValue)
                query = query.Where(x => x.NumberOfAdults == request.NumberOfAdults.Value);

            if (request.NumberOfChildren.HasValue)
                query = query.Where(x => x.NumberOfChildren == request.NumberOfChildren.Value);

            if (request.NumberOfRooms.HasValue)
                query = query.Where(x => x.NumberOfRooms == request.NumberOfRooms.Value);

            if (request.TpiIds != null && request.TpiIds.Any())
                query = query.Where(x => x.TPI_UID.HasValue && request.TpiIds.Contains(x.TPI_UID.Value));

            if (request.PaymentTypeIds != null && request.PaymentTypeIds.Any())
                query =
                    query.Where(
                        x => x.PaymentTypeUid.HasValue && request.PaymentTypeIds.Contains(x.PaymentTypeUid.Value));

            if (request.ReservationStatus != null && request.ReservationStatus.Any())
                query =
                    query.Where(
                        x => x.PaymentTypeUid.HasValue && request.ReservationStatus.Contains(x.PaymentTypeUid.Value));

            if (request.TotalAmount.HasValue)
                query = query.Where(x => x.TotalAmount == request.TotalAmount);

            if (request.ExternalTotalAmount.HasValue)
                query = query.Where(x => x.ExternalTotalAmount == request.ExternalTotalAmount);

            if (request.ExternalCommissionValue.HasValue)
                query = query.Where(x => x.ExternalCommissionValue == request.ExternalCommissionValue);

            if (request.ExternalIsPaid.HasValue)
                query = query.Where(x => x.ExternalIsPaid == request.ExternalIsPaid.Value);

            if (request.ChannelUIDs != null && request.ChannelUIDs.Count > 0)
                query = query.Where(x => x.ChannelUid.HasValue && request.ChannelUIDs.Contains(x.ChannelUid.Value));

            if (request.ChannelNames != null && request.ChannelNames.Count > 0)
                query = query.Where(x => request.ChannelNames.Contains(x.ChannelName));

            if (request.TPINames != null && request.TPINames.Count > 0)
                query = query.Where(x => request.TPINames.Contains(x.TPI_Name));

            if (request.IsPaid == false)
                query = query.Where(x => x.IsPaid == false || x.IsPaid == null);
            else if (request.IsPaid == true)
                query = query.Where(x => x.IsPaid == true);

            if (request.ReservationDate.HasValue)
                query = query.Where(x => x.ReservationDate == request.ReservationDate.Value);

            if (request.ExternalChannelUids != null && request.ExternalChannelUids.Count > 0)
                query = query.Where(x => x.ExternalChannelUid.HasValue && request.ExternalChannelUids.Contains(x.ExternalChannelUid.Value));

            if (request.ExternalTPIUids != null && request.ExternalTPIUids.Count > 0)
                query = query.Where(x => x.ExternalTPIUid.HasValue && request.ExternalTPIUids.Contains(x.ExternalTPIUid.Value));

            if (request.ExternalNames != null && request.ExternalNames.Count > 0)
                query = query.Where(x => request.ExternalNames.Contains(x.ExternalName));

            if (request.PartnerIds != null && request.PartnerIds.Any())
                query = query.Where(x => x.PartnerUid.HasValue && request.PartnerIds.Contains((int)x.PartnerUid.Value));

            if (request.PartnerReservationNumbers != null && request.PartnerReservationNumbers.Any())
                query = query.Where(x => request.PartnerReservationNumbers.Contains(x.PartnerReservationNumber));

            if (request.DateFrom.HasValue)
                query = query.Where(x => x.CreatedDate >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
            {
                request.DateTo = request.DateTo.Value.AddDays(1);
                query = query.Where(x => x.CreatedDate < request.DateTo.Value);
            }

            if (request.NestedFilters != null)
            {
                TreatRequestKendoFilters(request.NestedFilters);
                query = query.FilterBy(request.NestedFilters);
            }
            if (request.Filters != null && request.Filters.Any())
            {
                TreatRequestFilters(request.Filters);
                query = query.FilterBy(request.Filters);
            }

            if (request.Orders != null && request.Orders.Any())
            {
                var orders = request.Orders.Select(x =>
                            new OB.DL.Common.Filter.SortByInfo
                            {
                                OrderBy = x.OrderBy,
                                Direction = x.Direction,
                                Initial = x.Initial
                            }).ToList();

                TreatRequestOrders(orders);
                query = query.OrderBy(orders);
            }
            else
                query = query.OrderByDescending(x => x.UID);


            if (request.IncludeReservationRoomsFilter)
                query = query.Include(x => x.ReservationRoomFilters);

            if (request.CheckIn.HasValue)
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.CheckIn >= request.CheckIn.Value));

            if (request.CheckOut.HasValue)
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.CheckOut <= request.CheckOut.Value));

            if (request.ApplyDepositPolicy.HasValue)
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.ApplyDepositPolicy.Value
                                                                           &&
                                                                           (y.DepositCost.HasValue &&
                                                                            y.DepositCost.Value > 0 ||
                                                                            y.DepositNumberOfNight.HasValue &&
                                                                            y.DepositNumberOfNight > 0)));
            if (!string.IsNullOrEmpty(request.GuestName))
                query = query.Where(x => x.ReservationRoomFilters.Any(y => y.GuestName == request.GuestName));

            //TODO: remove this when nested filters support aggregated COUNT 
            if (request.FilterQtyExcludeReservationRoomsCancelled.HasValue && request.FilterQtyExcludeReservationRoomsCancelled.Value
                && request.Filters != null && request.Filters.Any(x => x.FilterBy == "NumberOfRooms"))
            {
                int value = 0;
                int.TryParse(request.Filters.First(x => x.FilterBy == "NumberOfRooms").Value.ToString(), out value);
                query = query.Where(x => x.ReservationRoomFilters.Count(y => y.Status != (int)OB.Reservation.BL.Constants.ReservationStatus.Cancelled) == value);
            }
          

            if (returnTotal)
                totalRecords = query.Count();

            if (request.PageIndex > 0 && request.PageSize > 0)
                query = query.Skip(request.PageIndex * request.PageSize);

            if (request.PageSize > 0)
                query = query.Take(request.PageSize);

            return query.ToList();
        }

        private void TreatRequestKendoFilters(Kendo.DynamicLinq.Filter kendoFilter)
        {
            if (kendoFilter.Filters == null || !kendoFilter.Filters.Any())
            {
                switch (kendoFilter.Field)
                {
                    case "ReservationRooms.DateFrom":
                        kendoFilter.Field = "ReservationRoomFilters.CheckIn";
                        break;
                    case "ReservationRooms.DateTo":
                        kendoFilter.Field = "ReservationRoomFilters.CheckOut";
                        break;
                    case "DateFrom":
                        kendoFilter.Field = "ReservationRoomFilters.CheckIn";
                        break;
                    case "DateTo":
                        kendoFilter.Field = "ReservationRoomFilters.CheckOut";
                        break;
                    case "IsRead":
                        kendoFilter.Field = "IsReaded";
                        break;
                    case "Qty":
                        kendoFilter.Field = "NumberOfRooms";
                        break;
                    case "TPIName":
                        kendoFilter.Field = "TPI_Name";
                        break;
                    case "ReservationStatus_UID":
                        kendoFilter.Field = "Status";
                        break;
                    case "ReservationStatus":
                        kendoFilter.Field = "Status";
                        break;
                    case "PaymentMethodType_UID":
                        kendoFilter.Field = "PaymentTypeUid";
                        break;
                    case "Property_UID":
                        kendoFilter.Field = "PropertyUid";
                        break;
                    case "Channel_UID":
                        kendoFilter.Field = "ChannelUid";
                        break;
                    case "Channel_Name":
                        kendoFilter.Field = "ChannelName";
                        break;  
                    case "date":
                        kendoFilter.Field = "ReservationDate";
                        break;
                    case "GuestFirstName":
                        kendoFilter.Field = "GuestName";
                        break;
                    case "GuestLastName":
                        kendoFilter.Field = "GuestName";
                        break;
                    case "ModifyDate":
                        kendoFilter.Field = "ModifiedDate";
                        break;
                    case "Adults":
                        kendoFilter.Field = "NumberOfAdults";
                        break;
                    case "Children":
                        kendoFilter.Field = "NumberOfChildren";
                        break;
                    case "CreateBy":
                        kendoFilter.Field = "CreatedBy";
                        break;
                    case "ModifyBy":
                        kendoFilter.Field = "ModifiedBy";
                        break;
                    case "TotalAmount":
                        kendoFilter.Value = kendoFilter.Value != null ? Convert.ToDecimal(kendoFilter.Value): kendoFilter.Value;
                        break;
                }
            }
            else
            {
                foreach (Kendo.DynamicLinq.Filter f in kendoFilter.Filters)
                {
                    TreatRequestKendoFilters(f);
                }
            }
        }

        private void TreatRequestFilters(List<Filter.FilterByInfo> filters)
        {
            foreach(var ftr in filters)
            {
                switch (ftr.FilterBy) {
                    case "ReservationRooms.DateFrom":
                        ftr.FilterBy = "ReservationRoomFilters.CheckIn";
                        break;
                    case "ReservationRooms.DateTo":
                        ftr.FilterBy = "ReservationRoomFilters.CheckOut";
                        break;
                    case "DateFrom":
                        ftr.FilterBy = "ReservationRoomFilters.CheckIn";
                        break;
                    case "DateTo":
                        ftr.FilterBy = "ReservationRoomFilters.CheckOut";
                        break;
                    case "IsRead":
                        ftr.FilterBy = "IsReaded";
                        break;
                    case "Qty":
                        ftr.FilterBy = "NumberOfRooms";
                        break;
                    case "TPIName":
                        ftr.FilterBy = "TPI_Name";
                        break;
                    case "ReservationStatus_UID":
                        ftr.FilterBy = "Status";
                        break;
                    case "ReservationStatus":
                        ftr.FilterBy = "Status";
                        break;
                    case "PaymentMethodType_UID":
                        ftr.FilterBy = "PaymentTypeUid";
                        break;
                    case "Property_UID":
                        ftr.FilterBy = "PropertyUid";
                        break;
                    case "Channel_UID":
                        ftr.FilterBy = "ChannelUid";
                        break;
                    case "Channel_Name":
                        ftr.FilterBy = "ChannelName";
                        break;
                    case "date":
                        ftr.FilterBy = "ReservationDate";
                        break;
                    case "GuestFirstName":
                        ftr.FilterBy = "GuestName";
                        break;
                    case "GuestLastName":
                        ftr.FilterBy = "GuestName";
                        break;
                    case "ModifyDate":
                        ftr.FilterBy = "ModifiedDate";
                        break;
                    case "Adults":
                        ftr.FilterBy = "NumberOfAdults";
                        break;
                    case "Children":
                        ftr.FilterBy = "NumberOfChildren";
                        break;
                    case "CreateBy":
                        ftr.FilterBy = "CreatedBy";
                        break;
                    case "ModifyBy":
                        ftr.FilterBy = "ModifiedBy";
                        break;
                }
            }
        }

        private void TreatRequestOrders(List<Filter.SortByInfo> orders)
        {
            foreach (var ftr in orders)
            {
                switch (ftr.OrderBy)
                {
                    case "ReservationRooms.DateFrom":
                        ftr.OrderBy = "ReservationRoomFilters.CheckIn";
                        break;
                    case "ReservationRooms.DateTo":
                        ftr.OrderBy = "ReservationRoomFilters.CheckOut";
                        break;
                    case "DateFrom":
                        ftr.OrderBy = "ReservationRoomFilters.CheckIn";
                        break;
                    case "DateTo":
                        ftr.OrderBy = "ReservationRoomFilters.CheckOut";
                        break;
                    case "IsRead":
                        ftr.OrderBy = "IsReaded";
                        break;
                    case "Qty":
                        ftr.OrderBy = "NumberOfRooms";
                        break;
                    case "TPIName":
                        ftr.OrderBy = "TPI_Name";
                        break;
                    case "ReservationStatus_UID":
                        ftr.OrderBy = "Status";
                        break;
                    case "ReservationStatus":
                        ftr.OrderBy = "Status";
                        break;
                    case "PaymentMethodType_UID":
                        ftr.OrderBy = "PaymentTypeUid";
                        break;
                    case "Property_UID":
                        ftr.OrderBy = "PropertyUid";
                        break;
                    case "Channel_UID":
                        ftr.OrderBy = "ChannelUid";
                        break;
                    case "date":
                        ftr.OrderBy = "ReservationDate";
                        break;
                    case "GuestFirstName":
                        ftr.OrderBy = "GuestName";
                        break;
                    case "GuestLastName":
                        ftr.OrderBy = "GuestName";
                        break;
                    case "ModifyDate":
                        ftr.OrderBy = "ModifiedDate";
                        break;
                    case "Adults":
                        ftr.OrderBy = "NumberOfAdults";
                        break;
                    case "Children":
                        ftr.OrderBy = "NumberOfChildren";
                        break;
                    case "CreateBy":
                        ftr.OrderBy = "CreatedBy";
                        break;
                    case "ModifyBy":
                        ftr.OrderBy = "ModifiedBy";
                        break;
                }
            }
        }
    }
}
