using Discord.WebSocket;
using DiscordBotCore.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        public List<Discord.GuildEmote> Emotes;
        public SocketTextChannel AlertChannel { get; set; }
        public Dictionary<string, DateTime> UpdateHistoy { get; set; }
        public CoinBot(DiscordSocketClient _client)
        {
            this._client = _client;

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("coinsconfig.json", false, true);

            Configuration = builder.Build();
            Client = new WebClient();

            UpdateHistoy = new Dictionary<string, DateTime>();

            TickerURL = Configuration["config:tickerURL"];

            using (StreamReader r = new StreamReader("coins.json"))
            {
                string json = r.ReadToEnd();
                Coins = JsonConvert.DeserializeObject<List<Coin>>(json);
            }

            Task.Run(() => RefreshCoins());

            aTimer = new Timer(5 * 60 * 1000); //one hour in milliseconds
            aTimer.AutoReset = true;
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Start();
        }

        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
           await RefreshCoins();
        }

        public async Task RefreshCoins()
        {

            for (int i = 0; i < Coins.Count; i++)
            {
                string json = Client.DownloadString(TickerURL + Coins[i].Id);
                List<Coin> Items = JsonConvert.DeserializeObject<List<Coin>>(json);
                Coin FoundCoin = Items.Find(x => x.Id == Coins[i].Id);
                FoundCoin.Alert = Coins[i].Alert;
                Coins[i] = FoundCoin;
            }

            if (AlertChannel != null)
            {
                foreach (Coin coin in Coins)
                {
                    string message = null;
                    bool NeedsUpdate = !UpdateHistoy.TryGetValue(coin.Name, out DateTime LastUpdate);

                    if (coin.Alert == "price" && NeedsUpdate && TimeSpan.FromHours(1) <= (DateTime.Now - LastUpdate))
                    {
                        string Emote = null;

                        if (Emotes.Exists(x => x.Name.ToLower() == coin.Name.ToLower()))
                        {
                            Emote = "<:" + coin.Name + ":" + Emotes.Find(x => x.Name.ToLower() == coin.Name.ToLower()).Id + ">";
                        }

                        if (coin.Percent_change_hour >= 4)
                        {
                            message = " Hey humans! " + coin.Name + " " + (Emote ?? "") + " went up by <:Green:361650797802684416> " + Math.Abs(coin.Percent_change_hour) + "%";
                        }
                        else if (coin.Percent_change_hour <= -4)
                        {
                            message = " Hey humans! " + coin.Name + " " + (Emote ?? "") + " went down by <:Red:361650806409396224> " + Math.Abs(coin.Percent_change_hour) + "%";

                        }

                        if (message != null)
                        {
                            UpdateHistoy.Add(coin.Name, DateTime.Now);
                            await AlertChannel.SendMessageAsync(message);
                        }
                    }
                }               
            }
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
