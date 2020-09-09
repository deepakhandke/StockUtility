using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockUtility.Models
{
    public class NSEInsider
    {
        public List<string> acqNameList { get; set; }
        public List<Datum> data { get; set; }
    }
    public class Datum
    {
        public string symbol { get; set; }
        public string company { get; set; }
        public string anex { get; set; }
        public string acqName { get; set; }
        public string date { get; set; }
        public string pid { get; set; }
        public object tkdAcqm { get; set; }
        public string buyValue { get; set; }
        public string sellValue { get; set; }
        public string buyQuantity { get; set; }
        public string sellquantity { get; set; }
        public string secType { get; set; }
        public string secAcq { get; set; }
        public string did { get; set; }
        public string tdpTransactionType { get; set; }
        public string tdpDerivativeContractType { get; set; }
        public string xbrl { get; set; }
        public string personCategory { get; set; }
        public string befAcqSharesNo { get; set; }
        public string befAcqSharesPer { get; set; }
        public string secVal { get; set; }
        public string securitiesTypePost { get; set; }
        public string afterAcqSharesNo { get; set; }
        public string afterAcqSharesPer { get; set; }
        public string acqfromDt { get; set; }
        public string acqtoDt { get; set; }
        public string intimDt { get; set; }
        public string acqMode { get; set; }
        public string derivativeType { get; set; }
        public string exchange { get; set; }
        public string remarks { get; set; }
    }
    public class NSEInsiderResponse
    {
        public List<StockData> Stocks { get; set; }
        public string Symbols { get; set; }

        public bool IsSuccess { get; set; }
        public bool IsCookieRequired { get; set; }
        public bool IsEligibleResponse { get; set; }
        public string ErrorMessage { get; set; }
        public string Cookie { get; set; }
    }
    public class StockData
    {
        public string Symbol { get; set; }
        public string PromoterInfo { get; set; }
        public long Value { get; set; }
        public long AveragePrice { get; set; }
    }
}
