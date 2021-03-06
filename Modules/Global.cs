using Discord.Commands;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Collections.Generic;

namespace Discord_Bot.Modules
{
    public class Global
    {
        //Static information during runtime, read from config.json
        public static readonly ConfigHandler Config = new();

        //List of logs, before they are cleared
        public static readonly List<Log> Logs = new();

        //Server information stored in a dictionary, the key is the Context.Guild.Id, the value is a complex class
        public static readonly Dictionary<ulong, Server> servers = new();

        public static bool InstagramChecker { get; set; }

        //Returns the current formatted time for log messages
        public static string Current_Time() { return DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second; }

        //Returns true if command's channel is a music channel
        public static bool IsMusicChannel(SocketCommandContext context)
        {
            if (servers[context.Guild.Id].MusicChannel == 0) return true;
            else return servers[context.Guild.Id].MusicChannel == context.Channel.Id;
        }
    }
}
