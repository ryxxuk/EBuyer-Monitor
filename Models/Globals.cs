using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Site_Monitor_Base.Models
{
    class Globals
    {
       public static int RequestNum = 0;
       public static int DiscordPings = 0;
       public static int Errors = 0;

       public static Dictionary<int, MonitorTask> MonitoredItems;
       public static readonly Dictionary<WebProxy, int> Proxies;
    }
}
