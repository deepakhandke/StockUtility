using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockUtility.Constants
{
    public class URLConstants
    {
        public static string NSEInsiderAPI = string.Format("/api/corporates-pit?index=equities&from_date={0}&to_date={1}",DateTime.Now.Date.AddMonths(-3).ToString("dd-MM-yyyy"), DateTime.Now.Date.ToString("dd-MM-yyyy"));
        public static string NSEInsiderCorporateAPI = "/api/corporates-pit?index=equities&symbol={0}";
        public static string NSEGetCorpDetail = "/api/quote-equity?symbol={0}&section=corp_info";


    }
}
