using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Extensions.Configuration.Binder;
using System.IO;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Threading;

namespace DiscordBotCore.AdminBot
{
    public class AdminBot
    {
        public static IConfigurationRoot Configuration { get; set; }
        public CoinBot coinBot { get; set; }
        public string CommandPrefix
        {
            get
            {
                return Configuration["options:prefix"];
            }
        }
        public AdminBot(CoinBot coinBot)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("commands.json", false, true);

            Configuration = builder.Build();

            this.coinBot = coinBot;
        }

        public string RunCommand(string command, SocketMessage message)
        {
            string response = "";
            string commandWord = "";
            string[] commandArray = command.Split(' ');
            if (commandArray.Length > 0)
            {
                commandWord = commandArray[0].ToLower();
            }
            string commandParameters = command.Substring(commandWord.Length, command.Length - commandWord.Length);
            string parameterError = "This command requires parameters.";
            string authorMention = message.Author.Mention;

            if (coinBot.Coins.Any(x => x.Symbol.ToLower() == commandWord.ToLower()))
            {
                response = coinBot.Coins.FirstOrDefault(x => x.Symbol.ToLower() == commandWord.ToLower()).ShowInfo();
            }
            else
            {

                switch (commandWord)
                {
                    case "":
                        response = "I didn't hear a command in there.";
                        break;

                    case "help":
                        var commandModels = new List<CommandModel>();
                        Configuration.GetSection("commands").Bind(commandModels);
                        response += "Commands: ";
                        foreach (var x in commandModels)
                        {
                            response += string.Format("\n{0}", x.Command);
                        }
                        response += "\nstatus (server name)\nrestart (server name)\nservers";
                        break;

                    default:
                        int commandIndex = 0;
                        bool nullCommand = false;
                        while (!nullCommand)
                        {
                            string sharedKey = "commands:" + commandIndex + ":";
                            string commandName = Configuration[sharedKey + "Command"];
                            nullCommand = commandName == null;
                            if (nullCommand)
                            {
                                response = authorMention + " I do not know this command.";
                                break;
                            }
                            else if (command == commandName)
                            {
                                response = authorMention + " " + Configuration[sharedKey + "Response"];

                                break;
                            }
                            else
                            {
                                commandIndex++;
                            }
                        }
                        break;
                }
            }

            return response;
        }


        private void RunBatch(string filePath)
        {
            System.Diagnostics.Process.Start(filePath);
        }

        //private long IPStringToInt(string addr)
        //{
        //    // careful of sign extension: convert to uint first;
        //    // unsigned NetworkToHostOrder ought to be provided.
        //    IPAddress address = IPAddress.Parse(addr);
        //    byte[] bytes = address.GetAddressBytes();
        //    //Array.Reverse(bytes); // flip big-endian(network order) to little-endian
        //    uint intAddress = BitConverter.ToUInt32(bytes, 0);
        //    return (long)intAddress;
        //}
    }
}
