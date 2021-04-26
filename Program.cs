using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EBuyer_Monitor.Functions;
using EBuyer_Monitor.Models;
using Newtonsoft.Json.Linq;
using Monitor = EBuyer_Monitor.Functions.Monitor;

namespace Site_Monitor_Base
{
    internal class Program
    {
        static void Main()
        {
            var app = new Program();
            app.Start();
            Console.ReadLine();
        }

        private async void Start()
        {
            var itemsToBeMonitored = await GetAllItemsToBeMonitored();

            foreach (var task in itemsToBeMonitored)
            {
                LoggingService.WriteLine($"Starting new task! {task.Product.ProductSku}");
                StartMonitorTask(task);
                LoggingService.WriteLine($"Sleeping 3 seconds!");
                Thread.Sleep(3123);
            }
        }

        private async Task<List<MonitorTask>> GetAllItemsToBeMonitored()
        {
            var itemsToBeMonitored = new List<MonitorTask>();

            dynamic responseObject = JObject.Parse(FormatJson(await File.ReadAllTextAsync(Directory.GetCurrentDirectory() + @"\items.json")));

            for (var i = 0; i < responseObject.items.Count; i++)
            {
                Product product = await EBuyer.GetProductDetails(responseObject.items[i].productSku.ToString());

                var task = new MonitorTask
                {
                    Product = product,
                    Interval = responseObject.items[i].interval * 1000,
                    UseProxy = responseObject.items[i].useProxy
                };

                var webhooks = new List<string>();

                for (var w = 0; w < responseObject.items[i].webhooks.Count; w++)
                {
                    webhooks.Add(responseObject.items[i].webhooks[w].url.ToString());
                }

                task.Webhooks = webhooks;

                if (task.UseProxy)
                {
                    task.Proxy = Proxy.GetNewProxy();
                }

                itemsToBeMonitored.Add(task);

                LoggingService.WriteLine($"Added {task.Product.ProductSku} to starting queue!");
            }

            return itemsToBeMonitored;
        }

        public void StartMonitorTask(MonitorTask task)
        {
            LoggingService.WriteLine($"Starting task for {task.Product.ItemName}!");
            Task.Run(() => MonitorTask(task));
        }

        private static string FormatJson(string json)
        {
            json = json.Trim().Replace("\r", string.Empty);
            json = json.Trim().Replace("\n", string.Empty);
            json = json.Replace(Environment.NewLine, string.Empty);

            return json;
        }

        public async Task MonitorTask(MonitorTask task)
        {
            try
            {
                while (true)
                {
                    var response = await Monitor.MonitorProduct(task.Product, task.Proxy);

                    if (response is null)
                    {
                        LoggingService.WriteLine($"{task.Product.ItemName} #OUTOFSTOCK");
                        task.Product.InStock = false;
                    }
                    else
                    {
                        if (!task.Product.InStock)
                        {
                            Globals.DiscordPings++;

                            LoggingService.WriteLine($"{task.Product.ItemName} #INSTOCK NOTIFIYING DISCORD");

                            EBuyer_Monitor.Functions.Discord.NotifyDiscordAsync(task, response);
                        }
                        else
                        {
                            LoggingService.WriteLine($"{task.Product.ItemName} #STOCKUNCHANGED");
                        }

                        task.Product.InStock = true;
                    }

                    Globals.RequestNum++;
                    UpdateTitle();
                    Thread.Sleep(task.Interval);
                }
            }
            catch (Exception e)
            {
                LoggingService.WriteLine(e.ToString());
                Thread.Sleep(120000);
                Globals.Errors++;
                UpdateTitle();
                LoggingService.WriteLine($"Slept 120 seconds. Restarting task for {task.Product.ItemName}!");
                Task.Run(() => MonitorTask(task));
            }
        }
        public void UpdateTitle()
        {
            Console.Title = $"[EBUYER] Requests: {Globals.RequestNum} | Pings: {Globals.DiscordPings} | Errors: {Globals.Errors}";
        }
    }
}
