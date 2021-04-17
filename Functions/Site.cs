using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Site_Monitor_Base.Functions
{
    public class Site
    {
        public static async Task<int> GetProductDetails(string sku)
        {
            try
            {
                var url = $"https://www.argos.co.uk/product/{sku}";

                using var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip,
                };
                
                using var client = new HttpClient(handler);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url)
                };

                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.66 Safari/537.36");
                
                var response = await client.SendAsync(request);

                var source = await response.Content.ReadAsStringAsync();

                var title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                title = Regex.Match(title, @"Buy (.*?) \|", RegexOptions.IgnoreCase).Groups[1].Value;
            }
            catch
            {
                return 0;
            }

            return 1;
        }

        public static CookieContainer GetSessionCookie(WebProxy proxy, bool useProxy)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://www.argos.co.uk/");
            if (useProxy)
            {
                request.Proxy = proxy;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
            }

            request.CookieContainer = new CookieContainer();
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36";

            var resp = new HttpWebResponse();

            try
            {
                resp = (HttpWebResponse)request.GetResponse();
                if (resp.StatusCode != HttpStatusCode.OK) throw new Exception($"Status code from proxy: {resp.StatusCode}");
            }
            catch
            {
                LoggingService.WriteLine("IP temp banned from www.argos.co.uk");
                resp.Close();
                resp.Dispose();
                return null;
            }

            LoggingService.WriteLine("Session id: " + resp.Headers.Get("sessionId"));

            resp.Close();
            resp.Dispose();

            return request.CookieContainer;
        }
    }
}
