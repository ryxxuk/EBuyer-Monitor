using System;
using System.ComponentModel.Design;
using System.Net;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EBuyer_Monitor.Models;

namespace EBuyer_Monitor.Functions
{
    public class Monitor
    {
        public static async Task<string> MonitorProduct(Product item, WebProxy proxy = null)
        {
            var response = await EBuyer.GetResponse(item.ProductSku, proxy);

            if (!response.Contains("https://schema.org/InStock")) return null;

            response = response.Replace(Environment.NewLine, string.Empty);

            var priceRegex = new Regex("price\\\": \\\"(.*?)\\\",");

            var priceMatches = priceRegex.Matches(response);

            var price = priceMatches[0].Groups[1].Value;

            return price;
        }
    }
}