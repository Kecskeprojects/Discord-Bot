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
    }
}
