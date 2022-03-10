using Discord_Bot.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public class Global
    {
        public static readonly List<Log> Logs = new();

        public static readonly List<ServerSetting> servers = new();



        //Returns the current formatted time for log messages
        public static string Current_Time() { return DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second; }
    }
}
