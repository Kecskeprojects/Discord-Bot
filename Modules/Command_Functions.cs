using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Jiubot
{
    class Command_Functions
    {
        /*
        //Embed making for song request in voice chat
        public static async Task Request_Embed(SocketCommandContext Context, string[] temp)
        {
            int i = Global.Music_requests.Where(n => n.server_id == Context.Guild.Id).Count();

            //Embed builder for queued songs
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(temp[1]);
            builder.WithUrl(temp[0]);

            builder.WithDescription("Song has been added to the queue!");

            builder.WithThumbnailUrl(temp[2]);

            builder.AddField("Song duration:", temp[3].ToLower(), true);
            builder.AddField("Position in queue:", i - 1, true);


            builder.WithTimestamp(DateTime.Now);
            builder.WithColor(Color.Red);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        //Embed making for currently playing song in voice chat
        public static async Task Np_Embed(SocketCommandContext Context, Music_request item, string elapsed_time)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(item.title);
            builder.WithUrl(item.url);

            builder.WithThumbnailUrl(item.thumbnail);

            builder.AddField("Requested by:", item.user, false);
            builder.AddField("Song duration:", elapsed_time + " / " + item.duration.ToLower(), false);

            builder.WithTimestamp(DateTime.Now);
            builder.WithColor(Color.DarkBlue);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        //Checking algorithm for different Last.fm commands
        public static async Task<string> TopLastfmCheck(string parameters)
        {
            string[] periods = new string[] { "overall", "7day", "1month", "3month", "6month", "12month" };
            if(parameters.Split(' ').Length > 2) throw new Exception("Too many parameters!");
            else if(parameters.Split(' ').Length == 2)
            {
                string[] temp = parameters.Split(' ');
                if (int.TryParse(temp[0], out _) && periods.Contains(temp[1])) parameters = temp[0] + " " + temp[1];
                else if (int.TryParse(temp[1], out _) && periods.Contains(temp[0])) parameters = temp[1] + " " + temp[0];
                else throw new Exception("Wrong input format!");
            }
            else
            {
                if (int.TryParse(parameters, out _)) parameters += " overall";
                else if (periods.Contains(parameters)) parameters = "10 " + parameters;
                else throw new Exception("Wrong input format!");
            }

            await Task.Delay(1);

            return parameters;
        }
        */
    }
}
