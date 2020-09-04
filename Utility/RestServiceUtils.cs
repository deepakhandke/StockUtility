using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace StockUtility.Utility
{
    public class RestServiceUtils
    {
        private static HttpClient CreateClient(string uri, string AccessToken = null, Dictionary<string, string> Headers = null)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(uri)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(AccessToken))
            {
                client.DefaultRequestHeaders.Add("AccessToken", AccessToken);
            }
            if (Headers.HasRecords())
            {
                Headers
                    .ToList()
                    .Where(x => string.IsNullOrEmpty(AccessToken) || x.Key != "AccessToken")
                    .ToList()
                    .ForEach(x =>
                    {
                        client.DefaultRequestHeaders.Add(x.Key, x.Value);
                    });
            }
            return client;
        }
        public static V MakeRestCall<K, V>(K request, String path, String serviceUri, Dictionary<string, string> Headers = null) where K : new()
        {
            V returnval = default;
            using (HttpClient restClient = CreateClient(serviceUri, null, Headers))
            {
                var stringContent = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = restClient.PostAsync(path, stringContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseAsString = response.Content.ReadAsStringAsync().Result;
                    returnval = responseAsString.FromJson<V>();
                }
                return returnval;
            }
        }

        public static V MakeRestCallByTimeOut<K, V>(K request, String path, String serviceUri, int timeOut) where K : new()
        {
            V returnval = default;
            using (HttpClient restClient = CreateClient(serviceUri))
            {
                restClient.Timeout = TimeSpan.FromMinutes(timeOut);
                var stringContent = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = restClient.PostAsync(path, stringContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseAsString = response.Content.ReadAsStringAsync().Result;
                    returnval = responseAsString.FromJson<V>();
                }
                return returnval;
            }
        }

        public static V MakeGetRestCall<V>(String path, String serviceUri)
        {
            V returnval = default;
            using (HttpClient restClient = CreateClient(serviceUri))
            {
                HttpResponseMessage response = restClient.GetAsync(path).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseAsString = response.Content.ReadAsStringAsync().Result;
                    returnval = responseAsString.FromJson<V>();
                }
                return returnval;
            }
        }

        public static V MakeGetRestCallByTimeOut<V>(String path, String serviceUri, int timeOut,string Cookie)
        {
            V returnval = default;
            using (HttpClient restClient = CreateClient(serviceUri,null,new Dictionary<string, string>() { { "Cookie", Cookie } }))
            {
                restClient.Timeout = TimeSpan.FromMinutes(timeOut);
                HttpResponseMessage response = restClient.GetAsync(path).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseAsString = response.Content.ReadAsStringAsync().Result;
                    returnval = responseAsString.FromJson<V>();
                }
                return returnval;
            }
        }
    }
}
