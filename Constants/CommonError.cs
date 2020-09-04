using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockUtility.Constants
{
    public class CommonError
    {
        public const string ConfigurationKeyMissing = "Are you crazy, this configuration key doesn't exists!!";
        public const string ConfigurationKeyIntParseError = "Cannot be parsed to integer. Value: {0}";
        public const string ConfigurationKeyLongParseError = "Cannot be parsed to long. Value: {0}";
        public const string InvalidRequest = "Invalid Request";
        public const string FailedToFetchData = "Failed To Fetch Data";
        public const string NoRecordsFound = "No Records Found";
        public const string ProblemWithServer = "Problem With Server";
        public const string CookieExpired = "Cookie Expired! Please Provide a new one.";
    }
}
