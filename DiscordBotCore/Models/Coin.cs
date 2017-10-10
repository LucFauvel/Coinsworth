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

        public float? PreviousPercent { get; set; }

        public string ShowInfo(DiscordSocketClient client)
        {
            List<Discord.GuildEmote> Emotes = new List<Discord.GuildEmote>(client.GetGuild(358635130430029834).Emotes);
            string Emote = null;

            if (Emotes.Exists(x => x.Name.ToLower() == Name.ToLower()))
            {
                Emote = "<:" + Name + ":" + Emotes.Find(x => x.Name.ToLower() == Name.ToLower()).Id + ">";
            }

            return "Name : " + (Emote ?? "") + " " + Name + "\n"
                 + "Symbol : " + Symbol + "\n"
                 + "\nPrice USD : $" + Price_usd + "\n"
                 + "Price BTC : " + Price_btc + " Ƀ\n"
                 + "\nPercent change 1h : " + (Percent_change_hour < 0 ? "<:Red:361650806409396224> " : "<:Green:361650797802684416> ") + Math.Abs(Percent_change_hour) + "%\n"
                 + "Percent change 24h : " + (Percent_change_Day < 0 ? "<:Red:361650806409396224> " : "<:Green:361650797802684416> ") + Math.Abs(Percent_change_Day) + "%\n"
                 + "Percent change 7d : " + (Percent_change_Week < 0 ? "<:Red:361650806409396224> " : "<:Green:361650797802684416> ") + Math.Abs(Percent_change_Week) + "%\n";
        }
    }
}
