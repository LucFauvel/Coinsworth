using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotCore.Models
{
    public class Coin
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("alert")]
        public string Alert { get; set; }

        [JsonProperty("price_usd")]
        public double Price_usd { get; set; }

        [JsonProperty("price_btc")]
        public double Price_btc { get; set; }

        [JsonProperty("24h_volume_usd")]
        public double Day_volume_usd { get; set; }

        [JsonProperty("market_cap_usd")]
        public double Market_cap_usd { get; set; }

        [JsonProperty("available_supply")]
        public double Available_supply { get; set; }

        [JsonProperty("total_supply")]
        public double Total_supply { get; set; }

        [JsonProperty("percent_change_1h")]
        public float Percent_change_hour { get; set; }

        [JsonProperty("percent_change_24h")]
        public float Percent_change_Day { get; set; }

        [JsonProperty("percent_change_7d")]
        public float Percent_change_Week { get; set; }

        public string ShowInfo(DiscordSocketClient client)
        {
            List<Discord.GuildEmote> Emotes = new List<Discord.GuildEmote>(client.GetGuild(358635130430029834).Emotes);
            string Emote = "<:" + Name + ":" + Emotes.Find(x => x.Name == Name).Id + ">";

            return "Name : " + Name + " " + Emote + "\n"
                 + "Symbol : " + Symbol + "\n"
                 + "Price USD : " + Price_usd + "$\n"
                 + "Percent change 1h : " + Math.Abs(Percent_change_hour) + "% " + (Percent_change_hour < 0 ? "<:Red:361650806409396224>" : "<:Green:361650797802684416>") + "\n"
                 + "Price BTC : " + Price_btc + "\n";
        }
    }
}
