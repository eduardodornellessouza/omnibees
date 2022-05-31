using OB.BL.Contracts.Data.Rates;
using OB.Reservation.BL.Contracts.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OB.BL.Operations.Helper
{
    /// <summary>
    /// Custom comparer for the RateRoomDetailsDataWithChildRecordsCustomComparer class 
    /// </summary>
    public class RateRoomDetailsDataWithChildRecordsCustomComparerByRateRoom : IEqualityComparer<RateRoomDetailsDataWithChildRecordsCustom>
    {
        // Products are equal if their names and product numbers are equal. 
        public bool Equals(RateRoomDetailsDataWithChildRecordsCustom x, RateRoomDetailsDataWithChildRecordsCustom y)
        {

            //Check whether the compared objects reference the same data. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null. 
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal. 
            return x.RateId == y.RateId && x.RoomTypeId == y.RoomTypeId;
        }

        // If Equals() returns true for a pair of objects  
        // then GetHashCode() must return the same value for these objects. 

        public int GetHashCode(RateRoomDetailsDataWithChildRecordsCustom obj)
        {
            //Check whether the object is null 
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Name field if it is not null. 
            int hashProductName = obj.RateId.GetHashCode();

            //Get hash code for the Code field. 
            int hashProductCode = obj.RoomTypeId.GetHashCode();

            //Calculate the hash code for the product. 
            return hashProductName ^ hashProductCode;
        }
    }

    /// <summary>
    /// Custom comparer for the RateRoomDetailsDataWithChildRecordsCustomComparer class 
    /// </summary>
    public class RateRoomDetailsDataWithChildRecordsCustomComparerByWeekDays : IEqualityComparer<RateRoomDetailsDataWithChildRecordsCustom>
    {
        // Products are equal if their names and product numbers are equal. 
        public bool Equals(RateRoomDetailsDataWithChildRecordsCustom x, RateRoomDetailsDataWithChildRecordsCustom y)
        {

            //Check whether the compared objects reference the same data. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null. 
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal. 
            return x.RateId == y.RateId && x.RoomTypeId == y.RoomTypeId && x.IsMonday == y.IsMonday && x.IsTuesday == y.IsTuesday
                    && x.IsWednesday == y.IsWednesday && x.IsThursday == y.IsThursday && x.IsFriday == y.IsFriday && x.IsSaturday == y.IsSaturday && x.IsSunday == y.IsSunday;
        }

        // If Equals() returns true for a pair of objects  
        // then GetHashCode() must return the same value for these objects. 

        public int GetHashCode(RateRoomDetailsDataWithChildRecordsCustom obj)
        {
            //Check whether the object is null 
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Name field if it is not null. 
            int hashProductName = obj.RateId.GetHashCode();

            //Get hash code for the Code field. 
            int hashProductCode = obj.RoomTypeId.GetHashCode();

            //Calculate the hash code for the product. 
            return hashProductName ^ hashProductCode;
        }
    }


    /// <summary>
    /// Custom comparer for the DatesLogging class 
    /// </summary>
    public class DatesLoggingComparer : IEqualityComparer<DatesLogging>
    {
        // Products are equal if their names and product numbers are equal. 
        public bool Equals(DatesLogging x, DatesLogging y)
        {

            //Check whether the compared objects reference the same data. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null. 
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal. 
            return x.DateFrom == y.DateFrom && x.DateTo == y.DateTo;
        }

        // If Equals() returns true for a pair of objects  
        // then GetHashCode() must return the same value for these objects. 

        public int GetHashCode(DatesLogging obj)
        {
            //Check whether the object is null 
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Name field if it is not null. 
            int hashProductName = obj.DateFrom.GetHashCode();

            //Get hash code for the Code field. 
            int hashProductCode = obj.DateTo.GetHashCode();

            //Calculate the hash code for the product. 
            return hashProductName ^ hashProductCode;
        }
    }

    /// <summary>
    /// Custom comparer for One property of the object
    /// </summary>
    public class PropertyComparer<T> : IEqualityComparer<T>
    {
        private readonly System.Reflection.PropertyInfo _PropertyInfo;

        /// <summary>
        /// Creates a new instance of PropertyComparer.
        /// </summary>
        /// <param name="propertyName">The name of the property on type T 
        /// to perform the comparison on.</param>
        public PropertyComparer(string propertyName)
        {
            //store a reference to the property info object for use during the comparison
            _PropertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
            if (_PropertyInfo == null)
            {
                throw new ArgumentException(string.Format("{0} is not a property of type {1}.", propertyName, typeof(T)));
            }
        }

        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            //get the current value of the comparison property of x and of y
            object xValue = _PropertyInfo.GetValue(x, null);
            object yValue = _PropertyInfo.GetValue(y, null);

            //if the xValue is null then we consider them equal if and only if yValue is null
            if (xValue == null)
                return yValue == null;

            //use the default comparer for whatever type the comparison property is.
            return xValue.Equals(yValue);
        }

        public int GetHashCode(T obj)
        {
            //get the value of the comparison property out of obj
            object propertyValue = _PropertyInfo.GetValue(obj, null);

            if (propertyValue == null)
                return 0;

            else
                return propertyValue.GetHashCode();
        }

        #endregion
    }  

}
