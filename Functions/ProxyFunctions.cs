using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Site_Monitor_Base.Models;

namespace Site_Monitor_Base.Functions
{
    class ProxyFunctions
    {
        public WebProxy GetNewProxy()
        {
            if (Globals.Proxies.Count is 0)
            {
                var lines = File.ReadAllLines($"{Directory.GetCurrentDirectory()}/proxies.txt");

                foreach (var proxy in lines)
                {
                    var split = proxy.Split(':');

                    try
                    {
                        Globals.Proxies.Add(Parse(split[0], split[1], split[2], split[3]), 0);
                    }
                    catch
                    {
                        try
                        {
                            Globals.Proxies.Add(Parse(split[0], split[1], null, null), 0);
                        }
                        catch
                        {
                            // do nothing
                        }
                    }
                }

                if (!Globals.Proxies.Any()) throw new Exception("No proxies in proxy file!");
            }

            var random = new Random();
            var randomIndex = random.Next(Globals.Proxies.Count);

            KeyValuePair<WebProxy, int> randomProxy;

            do
            {
                randomProxy = _proxies.ElementAt(randomIndex); // could get infinite loop here
            } while (randomProxy.Value > 4);

            LoggingService.WriteLine($"Trying following proxy: {randomIndex}");

            _proxies[randomProxy.Key]++;

            return randomProxy.Key;
        }
        public static WebProxy Parse(string hostname, string port, string username, string password)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var parsedProxy = new WebProxy();

            var ip = Dns.GetHostAddresses(hostname); // will throw exception if it cant resolve IP

            parsedProxy.Address = new Uri("http://" + ip[0] + ":" + port);

            if (username != null)
            {
                parsedProxy.Credentials = new NetworkCredential(username, password);
            }

            return parsedProxy;
        }
    }
}
