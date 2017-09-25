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
        Timer aTimer;
        DiscordSocketClient _client { get; set; }
        List<Discord.GuildEmote> Emotes;
        SocketTextChannel AlertChannel { get; set; }
        public CoinBot(DiscordSocketClient _client, List<Discord.GuildEmote> Emotes)
        {
            this._client = _client;
            this.Emotes = Emotes;

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

            //AlertChannel = textChannel;
            RefreshCoins(_client);

            aTimer = new Timer(5 * 60 * 1000); //one hour in milliseconds
            aTimer.AutoReset = true;
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Start();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            RefreshCoins(_client);
        }

        private void RefreshCoins(DiscordSocketClient client)
        {

            for (int i = 0; i < Coins.Count; i++)
            {
                string json = Client.DownloadString(TickerURL + Coins[i].Id);
                List<Coin> Items = JsonConvert.DeserializeObject<List<Coin>>(json);
                Coins[i] = Items.Find(x => x.Id == Coins[i].Id);
            }

            foreach (Coin coin in Coins)
            {
                string message = "Test";
                if (coin.Alert == "price")
                {
                    string Emote = "<:" + coin.Name + ":" + Emotes.Find(x => x.Name == coin.Name).Id + ">";

                    if (coin.Percent_change_hour >= 4)
                    {
                        message = "@everyone " + coin.Name + " " + Emote + " went up by " + Math.Abs(coin.Percent_change_hour) + " <:Green:361650797802684416>";
                    }
                    else if (coin.Percent_change_hour <= -4)
                    {
                        message = "@everyone " + coin.Name + " " + Emote + " went down by " + Math.Abs(coin.Percent_change_hour) + " <:Red:361650806409396224>";

                    }
                }
                
            }

            //AlertChannel.SendMessageAsync("Test");
        }

        public string LookupCoin(string CoinId)
        {
            string CleanId = CoinId.Replace(" ", String.Empty);
            List<Coin> Items = null;
            string json = null;

            try
            {
                json = Client.DownloadString(TickerURL + CleanId);
            }
            catch (WebException) { }

            if (json != null)
            {
                try
                {
                    Items = JsonConvert.DeserializeObject<List<Coin>>(json);
                }
                catch (JsonSerializationException) { }
            }

            if (Items != null)
            {
                return Items.Find(x => x.Id == CleanId).ShowInfo(_client);
            }
            else
            {
                return "Coin does not exist in the CMC database";
            }
        }
    }
}
