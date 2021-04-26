using System.Collections.Generic;
using System.Net;

namespace EBuyer_Monitor.Models
{
    public class MonitorTask
    {
        public int TaskNumber { get; set; }
        public bool UseProxy { get; set; }
        public WebProxy Proxy { get; set; }
        public Product Product { get; set; }
        public int Interval { get; set; }
        public List<string> Webhooks { get; set; }
    }
}
