using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockUtility.Utility;
using StockUtility.Constants;
using StockUtility.Models;
using Microsoft.AspNetCore.Session;
using ServiceStack.Text;
using ServiceStack;

namespace StockUtility.Controllers
{
    public class ScreenerController : Controller
    {
        [HttpGet]
        public IActionResult Swing()
        {
            NSEInsiderResponse resp = new NSEInsiderResponse();
            if (TempData["Cookie"].IsNotNull())
                resp.Cookie = Convert.ToString(TempData["Cookie"]);
            resp.IsCookieRequired = true;
            return View(resp);
        }
        [HttpPost]
        public IActionResult Swing(NSEInsiderResponse request)
        {
            try
            {
                NSEInsider response = RestServiceUtils.MakeGetRestCallByTimeOut<NSEInsider>(URLConstants.NSEInsiderAPI, "https://www.nseindia.com/", 2,request.Cookie);
                if (response.IsNull())
                    return View(new NSEInsiderResponse() { ErrorMessage = CommonError.FailedToFetchData });
                if (!response.data.HasRecords())
                    return View(new NSEInsiderResponse() { ErrorMessage = CommonError.FailedToFetchData });
                List<StockData> filteredData = response.data.Where(x => x.acqMode == "Market Purchase" && (x.personCategory == "Promoter Group" || x.personCategory == "Promoters"))
                                    .GroupBy(r => r.symbol)
                                    .Select(a => new StockData() { Symbol = a.Key, Value = a.Sum(b => long.Parse(b.secVal)) })
                                    .Where(p => p.Value > long.Parse("10000000")).OrderByDescending(x => x.Value)
                                    .ToList();
                TempData["Cookie"] = request.Cookie;
                TempData["Symbols"] = string.Join(",",filteredData.Select(x => x.Symbol).ToList());
                if (filteredData.HasRecords())
                    return View(new NSEInsiderResponse() { IsSuccess = true, Stocks = filteredData,Cookie = request.Cookie, Symbols = string.Join(",", filteredData.Select(x => x.Symbol).ToList()) });
                return View(new NSEInsiderResponse() { ErrorMessage = CommonError.NoRecordsFound });
            }
            catch (Exception)
            {
                return View(new NSEInsiderResponse() { IsCookieRequired = true, ErrorMessage = CommonError.CookieExpired });
            }
        }
        [HttpGet]
        public IActionResult SwingValue()
        {
            NSEInsiderResponse resp = new NSEInsiderResponse();
            if (TempData["Symbols"].IsNotNull())
                resp.Symbols = Convert.ToString(TempData["Symbols"]);
            if (TempData["Cookie"].IsNotNull())
                resp.Cookie = Convert.ToString(TempData["Cookie"]);
            return View(resp);
        }
        [HttpPost]
        public IActionResult SwingValue(NSEInsiderResponse request)
        {
            try
            {
                if (request.IsNull() || !request.Symbols.HasRecords() || request.Cookie.IsNull())
                    return View(new NSEInsiderResponse() { ErrorMessage = CommonError.InvalidRequest });
                TempData["Cookie"] = request.Cookie;
                TempData["Symbols"] = request.Symbols;
                List<StockData> stocks = new List<StockData>();
                List<string> symbols = request.Symbols.Split(',').ToList();
                foreach (var symbol in symbols)
                {
                    var price = GetAveragePrice(symbol, request.Cookie);
                    if (price == -1)
                        break;
                    else
                    {
                        stocks.Add(new StockData() { Symbol = symbol, AveragePrice = price });
                    } 
                }
                if (!stocks.HasRecords())
                {
                    return View(new NSEInsiderResponse() { IsCookieRequired = true, ErrorMessage = CommonError.CookieExpired });
                }
                return View(new NSEInsiderResponse() { IsSuccess = true, Stocks = stocks , Cookie = request.Cookie });
            }
            catch (Exception)
            {
                return View(new NSEInsiderResponse() { IsCookieRequired = true, ErrorMessage = CommonError.CookieExpired });
            }
        }


        [HttpGet]
        public IActionResult CorporateInfo()
        {
            NSEInsiderResponse response = new NSEInsiderResponse();
            try
            {
                string allSymbols = default(string);
                if (TempData["Symbols"].IsNotNull())
                    allSymbols = Convert.ToString(TempData["Symbols"]);
                if (TempData["Cookie"].IsNotNull())
                    response.Cookie = Convert.ToString(TempData["Cookie"]);
                if(string.IsNullOrEmpty(allSymbols) || string.IsNullOrEmpty(response.Cookie))
                {
                    response.ErrorMessage = "Invalid Request/Cookie Expired!";
                    return View("Swing", response);
                }
                List<string> Allsymbols = allSymbols.Split(',').ToList();
                List<StockData> filteredStocks = new List<StockData>(); 
                foreach (var symbol in Allsymbols)
                {
                    var priceTask = Task.Factory.StartNew(() => GetAveragePrice(symbol, response.Cookie));
                    var resp = GetCorpInfo(symbol, response.Cookie);
                    priceTask.Wait();
                    if (resp.IsNotNull())
                        filteredStocks.Add(new StockData() { Symbol = symbol , PromoterInfo = resp.PromoterDetails.Split("Promoter & Promoter Group,")[1].Split('}')[0], AveragePrice = priceTask.Result   });
                }
                if (filteredStocks.HasRecords())
                {
                    response.IsEligibleResponse = true;
                    response.Stocks = filteredStocks;
                }
                return View("Swing",response);
            }
            catch (Exception)
            {
                response.IsCookieRequired = true;
                response.ErrorMessage = CommonError.CookieExpired;
                return View("Swing", response);
            }

        }

        [NonAction]
        public long GetAveragePrice(string symbol,string cookie)
        {
            StockData stock = null;
            try
            {
                NSEInsider response = RestServiceUtils.MakeGetRestCallByTimeOut<NSEInsider>(string.Format(URLConstants.NSEInsiderCorporateAPI, symbol), "https://www.nseindia.com/", 1, cookie);
                if (response.IsNotNull())
                {
                    var filteredData = response.data.Where(x => x.symbol == symbol && x.acqMode == "Market Purchase" && (x.personCategory == "Promoter Group" || x.personCategory == "Promoters") && DateTime.Parse(x.acqfromDt) > DateTime.Now.AddMonths(-3));
                    stock =  filteredData.GroupBy(r => r.symbol)
                                        .Select(a => new StockData() { Symbol = a.Key, Value = a.Sum(b => long.Parse(b.secVal)), AveragePrice = a.Sum(b => long.Parse(b.secVal))/a.Sum(b => long.Parse(b.secAcq)) })
                                        .FirstOrDefault();
                }
                return stock.AveragePrice;
            }
            catch (Exception)
            {
                return  -1;
            }
        }

        [NonAction]
        public CorpDetailResp GetCorpInfo(string symbol, string cookie)
        {
            CorpDetailResp resp = new CorpDetailResp();
            try
            {
                CorpInfo response = RestServiceUtils.MakeGetRestCallByTimeOut<CorpInfo>(string.Format(URLConstants.NSEGetCorpDetail, symbol), "https://www.nseindia.com/", 1, cookie);
                long p = default(long);
                resp.mfSold = response.corporate.sastRegulations_29.HasRecords() && response.corporate.sastRegulations_29.Where(x => DateTime.Parse(x.timestamp) > DateTime.Now.AddMonths(-5)).HasRecords() ? response.corporate.sastRegulations_29.Where(x => DateTime.Parse(x.timestamp) > DateTime.Now.AddMonths(-5)).Any(y=> long.TryParse(y.noOfShareSale, out p)) : default(bool) ;
                resp.pledged = response.corporate.pledgedetails.HasRecords() ? decimal.Parse(response.corporate.pledgedetails.First().per3) > default(decimal) : default(bool);
                resp.PromoterDetails = response.corporate.shareholdingPatterns.data.HasRecords() ?response.corporate.shareholdingPatterns.data.First().ToJson() : string.Empty ;
                resp.IsNotEligible = resp.mfSold || resp.pledged;
                return resp.IsNotEligible ? null : resp;

            }
            catch (Exception)
            {

                return null;
            }
        }
    }
}