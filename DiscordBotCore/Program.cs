using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using Newtonsoft.Json;
using DiscordBotCore;
using System.Net.NetworkInformation;
using System.Net;
using DiscordBotCore.AdminBot;

namespace DiscordBot
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        private DiscordSocketClient _client;
        private AdminBot adminBot;
        private CoinBot coinBot;
        private string BotUsername;
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("authentication.json", false, false);

            Configuration = builder.Build();

            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                DefaultRetryMode = RetryMode.AlwaysRetry,
            });
            _client.Log += Log;

            string token = Configuration["auth:token"];
            BotUsername = Configuration["auth:BotUsername"];

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            coinBot = new CoinBot(_client);
            adminBot = new AdminBot(coinBot, _client);

            _client.MessageReceived += MessageReceived;
            _client.GuildAvailable += GuildAvailable;

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            string content = SanitizeContent(message.Content);
            string response = "";

            if (content.Substring(0, 1) == adminBot.CommandPrefix)
            {
                string command = content.Substring(1, content.Length - 1);
                response = adminBot.RunCommand(command, message);
            }
        
            if (!string.IsNullOrEmpty(response))
            {
                await message.Channel.SendMessageAsync(response);
            }
        }

        private async Task GuildAvailable(SocketGuild guild)
        {
            coinBot.AlertChannel = guild.GetTextChannel(361595846518636544);
            coinBot.Emotes = new List<GuildEmote>(guild.Emotes);
            await coinBot.RefreshCoins();
        }

        private string SanitizeContent(string message)
        {
            string sanitized = message;
            sanitized = Regex.Replace(sanitized, "<.*?>", string.Empty);
            if (sanitized.Substring(0, 1) == " ")
            {
                sanitized = sanitized.Substring(1, sanitized.Length - 1);
            }
            return sanitized;
        }
    }
}

