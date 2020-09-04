using Microsoft.Extensions.Configuration;
using StockUtility.Constants;
using System;
using System.Collections.Generic;
using System.Linq;


namespace StockUtility.Utility
{
    public class ConfigManager
    {
        public static IConfiguration Configuration { get; set; }
        private ConfigManager(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        private static string GetStringValue(string key)
        {
            string keyValue = Configuration["AppSettings:" + key];
            if (string.IsNullOrEmpty(keyValue))
                throw new Exception(CommonError.ConfigurationKeyMissing);

            return keyValue;
        }

        private int GetIntValue(string key)
        {
            string intValue = GetStringValue(key);
            bool isParsable = int.TryParse(intValue, out int returnValue);
            if (!isParsable)
                throw new Exception(string.Format(CommonError.ConfigurationKeyIntParseError, intValue));

            return returnValue;
        }

        private long GetLong(string key)
        {
            string doubleValue = GetStringValue(key);
            bool isParsable = long.TryParse(doubleValue, out long returnValue);
            if (!isParsable)
                throw new Exception(string.Format(CommonError.ConfigurationKeyLongParseError, doubleValue));

            return returnValue;
        }

        private List<string> GetStringAsList(string key)
        {
            string valueStr = GetStringValue(key);
            if (!String.IsNullOrEmpty(valueStr))
            {
                return new List<string>(valueStr.Split(','));
            }
            return null;
        }

        public static string NSEUrl
        {
            get
            {
                return GetStringValue("NSEUrl");
            }
        }
    }
}
