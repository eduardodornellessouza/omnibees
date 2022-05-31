using OB.Reservation.BL.Contracts.Responses;
using OB.BL.Operations.Exceptions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OB.BL.Operations.Extensions
{
    public static class EnumExtensions
    {       

        public static string GetDisplayAttribute(object value)
        {
            DisplayAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static BusinessLayerException ToBusinessLayerException(this Errors value, params string[] strings)
        {
            DisplayAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;
            var description = attribute == null ? value.ToString() : attribute.Description;

            return new BusinessLayerException(string.Format(description, strings), value.ToString(), (int)value);
        }

        public static BusinessLayerException ToBusinessLayerException(this Internal.BusinessObjects.Errors.Errors value, params string[] strings)
        {
            DisplayAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;
            var description = attribute == null ? value.ToString() : attribute.Description;

            return new BusinessLayerException(string.Format(description, strings), value.ToString(), (int)value);
        }
        
        public static BusinessLayerException ToBusinessLayerException(this Errors value, System.Exception e = null, params string[] strings)
        {
            DisplayAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;
            var description = attribute == null ? value.ToString() : attribute.Description;

            return new BusinessLayerException(string.Format(description, strings), value.ToString(), (int)value, e);
        }

        public static Error ToContractError(this Errors value, params string[] strings)
        {
            var data = BuildDataParams(strings);           
            return new Error(value.ToString(), (int)value, string.Format(GetDisplayAttribute(value), strings), data);
        }       

        static Dictionary<string, string> BuildDataParams(string[] strParams)
        {
            return strParams.Select((value, index) => new { Key = $"param{index}", Value = value }).ToDictionary(k => k.Key, k => k.Value);
        }


    }
}
