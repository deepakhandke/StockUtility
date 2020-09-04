using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockUtility.Utility
{
    public static class ExtensionMethod
    {
        public static bool IsNull(this object value)
        {
            return (value == null);
        }

        public static bool IsNotNull(this object value)
        {
            return (value != null);
        }

        public static bool HasRecords(this object value)
        {
            bool response = true;
            response = value != null;
            if (value is ICollection)
            {
                response = response && (value as ICollection).Count > 0;
            }
            return response;
        }

        public static string NullCheckTrim(this string value)
        {
            string Response = value;
            if (Response != null)
            {
                Response = Response.Trim();
            }
            return Response;
        }

        public static string NullCheckLower(this string Value)
        {
            return string.IsNullOrEmpty(Value) ? Value : Value.ToLower();
        }

        public static double Compare(this string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2) || str1.NullCheckTrim() == "" || str2.NullCheckTrim() == "")
                return 0;

            str1 = " " + str1.Replace(" ", string.Empty);
            str2 = " " + str2.Replace(" ", string.Empty);

            int possible = str1.Length + str2.Length - 2;
            int hits = 0;
            Task<int> Task1 = Task.Factory.StartNew<int>(() => Utility.GetHitCount(str1, str2));
            Task<int> Task2 = Task.Factory.StartNew<int>(() => Utility.GetHitCount(str1, str2));
            Task1.Wait();
            hits += Task1.Result;
            Task2.Wait();
            hits += Task2.Result;
            return Math.Round((double)((100 * hits) / possible), 2);
        }


        public static string StringWithPrePostSpaces(this object value)
        {
            if (value == null)
                return " ";

            return " " + value.ToString() + " ";
        }


        public static long ToElasticDate(this DateTime obj)
        {
            return (obj.CompareTo(Utility.StartDate) < 0) ? 0 : (long)obj.Subtract(Utility.StartDate).TotalSeconds;
        }
        public static bool CheckDefaultDate(this DateTime dt)
        {
            return dt.Date == default(DateTime);
        }
        public static DateTime ParseStringToDateTime(this string Date)
        {
            DateTime result = default(DateTime);
            try
            {
                result = DateTime.Parse(Date.Trim());
            }
            catch
            {
            }

            if (result != default(DateTime))
                return result;

            string[] formats = new string[]
            {
                "dd/MM/yyyy hh:mm:ss",
                "yyyy/MM/dd hh:mm:ss",
                "yyyy-MM-dd hh:mm:ss",
                "yyyy-dd-MM hh:mm:ss",
                "dd-MM-yyyy hh:mm:ss",
                "MM/dd/yyyy hh:mm:ss",
                "dd/MM/yyyy H:mm:ss",
                "yyyy/MM/dd H:mm:ss",
                "yyyy-MM-dd H:mm:ss",
                "yyyy-dd-MM H:mm:ss",
                "dd-MM-yyyy H:mm:ss",
                "MM/dd/yyyy H:mm:ss",
                "dd/MM/yyyy",
                "yyyy/MM/dd",
                "yyyy-MM-dd",
                "yyyy-dd-MM",
                "dd-MM-yyyy",
                "MM/dd/yyyy",
                "dd/MM/yyyy h:mm:ss tt",
                "yyyy/MM/dd h:mm:ss tt",
                "yyyy-MM-dd h:mm:ss tt",
                "yyyy-dd-MM h:mm:ss tt",
                "dd-MM-yyyy h:mm:ss tt",
                "MM/dd/yyyy h:mm:ss tt",
                "M/dd/yyyy  h:mm:ss tt"
            };

            try
            {
                result = DateTime.ParseExact(Date.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch
            {
            }
            return result;
        }
    }
    public class Utility
    {
        static ILoggerFactory _logFactory = null;
        public Utility(ILoggerFactory loggerFactory)
        {
            _logFactory = loggerFactory;
        }
        public static DateTime StartDate = new DateTime(1900, 1, 1);

        internal static Expression<Func<TSource, object>> GetExpression<TSource>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TSource), "x");
            //important to use the Expression.Convert
            Expression conversion = Expression.Convert(Expression.Property(param, propertyName), typeof(object));
            return Expression.Lambda<Func<TSource, object>>(conversion, param);
        }

        internal static int GetHitCount(string str1, string str2)
        {
            int hits = 0;
            for (int i = 0; i <= str1.Length - 1; i++)
            {
                int length = (i == str1.Length - 1) ? 1 : 2;
                if (str2.IndexOf(str1.Substring(i, length)) != -1)
                    hits++;
            }
            return hits;
        }

        public static void LogError(Type type, string Exception, Exception ex = null)
        {
            if (_logFactory.IsNotNull())
            {
                if (ex.IsNotNull())
                    _logFactory.CreateLogger(type.Name).LogError(Exception, ex);
                else
                    _logFactory.CreateLogger(type.Name).LogError(Exception);
            }
        }
    }
}
