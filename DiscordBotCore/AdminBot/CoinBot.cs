using Discord.WebSocket;
using DiscordBotCore.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;

namespace DiscordBotCore.AdminBot
{
    public class CoinBot
    {
        public static IConfigurationRoot Configuration { get; set; }
        public List<Coin> Coins { get; set; }
        string TickerURL { get; set; }
        WebClient Client;
        DiscordSocketClient _client;
        public CoinBot(DiscordSocketClient _client)
        {
            this._client = _client;

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("coinsconfig.json", false, true);

            Configuration = builder.Build();
            Client = new WebClient();

            TickerURL = Configuration["config:tickerURL"];

            using (StreamReader r = new StreamReader("coins.json"))
            {
                string json = r.ReadToEnd();
                Coins = JsonConvert.DeserializeObject<List<Coin>>(json);
            }

            RefreshCoins();

            Timer aTimer = new Timer(60 * 60 * 1000); //one hour in milliseconds
            aTimer.AutoReset = true;
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            RefreshCoins();
        }

        private void RefreshCoins()
        {
            for (int i = 0; i < Coins.Count; i++)
            {
                string json = Client.DownloadString(TickerURL + Coins[i].Id);
                List<Coin> Items = JsonConvert.DeserializeObject<List<Coin>>(json);
                Coins[i] = Items.Find(x => x.Id == Coins[i].Id);
            }

            foreach (Coin coin in Coins)
            {
                if (coin.Alert == "price")
                {
                    if (coin.Percent_change_hour >= 5)
                    {
                        string message = "@everyone " + coin.Name + " went up by " + coin.Percent_change_hour;
                        _client.GetGuild(358635130430029834).GetTextChannel(361595846518636544).SendMessageAsync(message);
                    }
                    else if (coin.Percent_change_hour <= -5)
                    {
                        string message = "@everyone " + coin.Name + " went down by " + coin.Percent_change_hour;
                        _client.GetGuild(358635130430029834).GetTextChannel(361595846518636544).SendMessageAsync(message);
                    }
                }
            }
        }
    }
}
