using System.Collections.Generic;
using System.Net;
using ArgosMonitor;

namespace Site_Monitor_Base.Models
{
    public class MonitorTask
    {
        public int TaskNumber { get; set; }
        public bool IsRunning { get; set; }
        public bool UseProxy { get; set; }
        public WebProxy Proxy { get; set; }
        public CookieContainer Cookies { get; set; }
        public Product Product { get; set; }
        public int Interval { get; set; }
        public List<string> Webhooks { get; set; }
    }
}
