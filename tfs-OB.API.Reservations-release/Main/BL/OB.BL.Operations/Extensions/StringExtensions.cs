using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations
{
    public static class StringExtensions
    {
        // Convert the string to camel case.
        public static string ToCamelCase(this string the_string)
        {
            string result = "";

            the_string = the_string.Replace("-", " ");
            the_string = the_string.Replace("_", " ");

            // Primeira de cada palavra maiscula
            foreach (var piece in the_string.Split(' '))
            {
                result += piece.FirstLetterToUpper();
            }

            // Primeira Letra Pequena
            return result.FirstLetterToLower();
        }

        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static string FirstLetterToLower(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToLower(str[0]) + str.Substring(1);

            return str.ToLower();
        }
    }
}
