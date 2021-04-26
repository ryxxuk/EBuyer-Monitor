using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EBuyer_Monitor.Models;

namespace EBuyer_Monitor.Functions
{
    public class EBuyer
    {
        public static async Task<string> GetResponse(string sku, WebProxy proxy = null)
        {
            try
            {
                var url = $"https://www.ebuyer.com/{sku}";

                using var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip,
                };

                if (proxy != null)
                {
                    handler.Proxy = proxy;
                    handler.Credentials = CredentialCache.DefaultNetworkCredentials;
                }

                using var client = new HttpClient(handler);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url)
                };

               // request.Headers.Add("Accept", "application/json");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.66 Safari/537.36");
                
                var response = await client.SendAsync(request);

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                LoggingService.WriteLine($"ERROR: {e}");
                return null;
            }
        }

        public static async Task<Product> GetProductDetails(string itemSku)
        {
            var response = await GetResponse(itemSku);

            response = response.Replace(Environment.NewLine, "");

            var nameRegex = new Regex("name\":\"(.*?)\",");

            var nameMatches = nameRegex.Matches(response);

            var name = nameMatches[0].Groups[1].Value;

            var imageRegex = new Regex("https:\\/\\/img\\.ebyrcdn\\.net\\/.*?\\.jpg");

            var imageMatches = imageRegex.Matches(response);

            var image = imageMatches[0].Groups[0].Value;

            var product = new Product
            {
                ItemName = name,
                Image = image,
                InStock = false,
                ProductSku = itemSku
            };

            return product;
        }
    }
}
