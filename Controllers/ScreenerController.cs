using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockUtility.Utility;
using StockUtility.Constants;
using StockUtility.Models;
using Microsoft.AspNetCore.Session;

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
            catch (Exception e)
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
                    var data = EligibleStock(symbol, request.Cookie);
                    if (data.IsNotNull() && data.AveragePrice == -1)
                        break;
                    if (data.IsNotNull())
                        stocks.Add(data);
                }
                if (!stocks.HasRecords())
                {
                    return View(new NSEInsiderResponse() { IsCookieRequired = true, ErrorMessage = CommonError.CookieExpired });
                }
                return View(new NSEInsiderResponse() { IsSuccess = true, Stocks = stocks , Cookie = request.Cookie });
            }
            catch (Exception e)
            {
                return View(new NSEInsiderResponse() { IsCookieRequired = true, ErrorMessage = CommonError.CookieExpired });
            }
        }

        [NonAction]
        public StockData EligibleStock(string symbol,string cookie)
        {
            StockData stock = null;
            try
            {
                NSEInsider response = RestServiceUtils.MakeGetRestCallByTimeOut<NSEInsider>(string.Format(URLConstants.NSEInsiderCorporateAPI, symbol), "https://www.nseindia.com/", 1, cookie);
                if (response.IsNotNull())
                {
                    var filteredData = response.data.Where(x => x.symbol == symbol && x.acqMode == "Market Purchase" && (x.personCategory == "Promoter Group" || x.personCategory == "Promoters"));
                    stock =  filteredData.GroupBy(r => r.symbol)
                                        .Select(a => new StockData() { Symbol = a.Key, Value = a.Sum(b => long.Parse(b.secVal)), AveragePrice = a.Sum(b => long.Parse(b.secVal))/a.Sum(b => long.Parse(b.secAcq)) })
                                        .FirstOrDefault();
                }
                return stock;
            }
            catch (Exception e)
            {
                return new StockData() { AveragePrice = -1 };
            }
        }
    }
}