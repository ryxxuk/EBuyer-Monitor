using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Webhook;
using EBuyer_Monitor.Models;

namespace EBuyer_Monitor.Functions
{
    internal class Discord
    {
        public static async void NotifyDiscordAsync(MonitorTask monitorTask, string price)
        {
            var embed = new EmbedBuilder();

            var embeds = new List<Embed>
            {
                embed
                    .WithAuthor("https://www.ebuyer.com/ - Restock Detected!")
                    .WithFooter("RYXX Monitors | @ryxxuk")
                    .WithColor(Color.Blue)
                    .WithTitle(monitorTask.Product.ItemName)
                    .WithFields(new EmbedFieldBuilder
                    {
                        Name = "Price",
                        Value = price
                    })
                    .WithCurrentTimestamp()
                    .WithThumbnailUrl(monitorTask.Product.Image)
                    .WithUrl("https://www.ebuyer.com/" + monitorTask.Product.ProductSku)
                    .Build()
            };

            foreach (var client in monitorTask.Webhooks.Select(webhook => new DiscordWebhookClient(webhook)))
            {
                await client.SendMessageAsync("", false, embeds: embeds);
            }
        }
    }
}
