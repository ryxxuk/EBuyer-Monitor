using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Site_Monitor_Base.Functions;
using Site_Monitor_Base.Models;

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

        private async Task Start()
        {
            var itemsToBeMonitored = await GetAllItemsToBeMonitored();

            foreach (var item in itemsToBeMonitored)
            {
                LoggingService.WriteLine($"Starting new task! {task} [{(task. ? "USING PROXY" : "NOT USING PROXY")}]");
                StartMonitorTask(task);
                LoggingService.WriteLine($"Sleeping 3 seconds!");
                Thread.Sleep(3 * 1000);
            }
        }

        private async Task<List<MonitorTask>> GetAllItemsToBeMonitored()
        {
            var itemsToBeMonitored = new List<Product>();

            dynamic responseObject = JObject.Parse(FormatJson(await File.ReadAllTextAsync(Directory.GetCurrentDirectory() + @"\items.json")));

            for (var i = 0; i < responseObject.items.Count; i++)
            {
                var item = new Item
                {
                    ProductSku = responseObject.items[i].productSku,
                    Interval = responseObject.items[i].interval * 1000,
                    UseProxy = responseObject.items[i].useProxy,
                    ImageUrl = responseObject.items[i].image,
                    Price = responseObject.items[i].price,
                    Name = responseObject.items[i].name,
                    InStock = false
                };

                var webhooks = new List<string>();

                for (var w = 0; w < responseObject.items[i].webhooks.Count; w++)
                {
                    webhooks.Add(responseObject.items[i].webhooks[w].url.ToString());
                }

                item.Webhooks = webhooks;

                itemsToBeMonitored.Add(item);
            }

            return itemsToBeMonitored;
        }

        public void StartMonitorTask(Item item)
        {
            LoggingService.WriteLine($"Starting task for {item.Name}!");
            Task.Run(() => MonitorTask(item));
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
                    var response = await Functions.Monitor.MonitorProduct(task);

                    if (response > 0)
                    {
                        if (!task.Product.InStock)
                        {
                            Globals.DiscordPings++;

                            LoggingService.WriteLine($"{task.Product.ItemName} #INSTOCK NOTIFIYING DISCORD");

                            Functions.Discord.NotifyDiscordAsync(task, response);
                        }
                        else
                        {
                            LoggingService.WriteLine($"{task.Product.ItemName} #STOCKUNCHANGED");
                        }

                        task.Product.InStock = true;
                    }
                    else
                    {
                        LoggingService.WriteLine($"{task.Product.ItemName} #OUTOFSTOCK");
                        task.Product.InStock = false;
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
            Console.Title = $"[SITE] Requests: {Globals.RequestNum} | Pings: {Globals.DiscordPings} | Errors: {Globals.Errors}";
        }
    }
}
