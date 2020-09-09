using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockUtility.Models
{
    public class CorpInfo
    {
        public Corporate corporate { get; set; }
    }
    public class CorpDetailResp
    {
        public bool mfSold { get; set; }
        public bool pledged { get; set; }
        public string PromoterDetails { get; set; }
        public bool IsNotEligible { get; set; }
    }
    public class Corporate
    {
        public ShareholdingPatterns shareholdingPatterns { get; set; }
        public List<Reg29> sastRegulations_29 { get; set; }
        public List<Pledge> pledgedetails { get; set; }

    }
    public class ShareholdingPatterns
    {
        public List<string> cols { get; set; }
        public List<object> data { get; set; }
    }
    public class Reg29
    {
        public string regType { get; set; }
        public string acquirerName { get; set; }
        public string acquirerDate { get; set; }
        public string noOfShareAcq { get; set; }
        public string noOfShareSale { get; set; }
        public string noOfShareAft { get; set; }
        public string attachement { get; set; }
        public string timestamp { get; set; }
    }
    public class Pledge
    {
        public string symbol { get; set; }
        public string disclosureDate { get; set; }
        public string per1 { get; set; }
        public string per2 { get; set; }
        public string per3 { get; set; }
    }


}
