using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Webhook;
using Site_Monitor_Base.Models;

namespace Site_Monitor_Base.Functions
{
    class Discord
    {
        public static async void NotifyDiscordAsync(MonitorTask monitorTask, int stock)
        {
            var embed = new EmbedBuilder();

            var embeds = new List<Embed>();

            var message = "";

            embeds.Add(embed
                .WithAuthor("Restock Detected!")
                .WithFooter("RYXX Monitors | @ryxxuk")
                .WithColor(Color.Blue)
                .WithTitle(monitorTask.Product.ItemName)
                .WithFields(new EmbedFieldBuilder
                {
                    Name = "Available at:",
                    Value = message
                })
                .WithCurrentTimestamp()
                .WithThumbnailUrl(
                    $"https://media.4rgos.it/s/Argos/{monitorTask.Product.ProductSku}_R_SET?$Main768$&amp;w=620&amp;h=620")
                .WithUrl("https://www.argos.co.uk/product/" + monitorTask.Product.ProductSku)
                .Build());


            foreach (var client in monitorTask.Webhooks.Select(webhook => new DiscordWebhookClient(webhook)))
            {
                await client.SendMessageAsync("", false, embeds: embeds);
            }
        }
    }
}
