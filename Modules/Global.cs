using Discord.Commands;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Collections.Generic;

namespace Discord_Bot.Modules
{
    public class Global
    {
        public static readonly List<Log> Logs = new();

        public static readonly Dictionary<ulong, ServerSetting> servers = new();

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
