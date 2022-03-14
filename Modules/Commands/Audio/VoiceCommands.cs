using System;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.Commands;
using System.Linq;
using Discord_Bot.Modules.ListClasses;

namespace Discord_Bot.Modules.Commands.Audio
{
    public class VoiceCommands : ModuleBase<SocketCommandContext>
    {
        //
        //VOICE CHAT COMMANDS
        //


        //Play music on the channel the user is connected to
        [Command("p")]
        public async Task Play([Remainder] string content)
        {
            try
            {
                if (Global.IsMusicChannel(Context) == false) return;
                ulong sId = Context.Guild.Id;

                if (!Global.servers[sId].AudioVars.Playing)
                {
                    Console.WriteLine(content);
                    Global.servers[sId].AudioVars.Playing = true;
                    await AudioFunctions.RequestHandler(Context, content);
                    if (Global.servers[sId].MusicRequests.Count > 0)
                    {
                        await AudioFunctions.PlayHandler(Context, sId);
                    }
                    Global.servers[sId].AudioVars.Playing = false;
                }
                else
                {
                    await AudioFunctions.RequestHandler(Context, content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "VoiceCommands.cs Play", ex.ToString()));
            }
        }



        //Joins the bot to the user's voice channel
        [Command("join")]
        public async Task Join()
        {
            if (Global.IsMusicChannel(Context) == false) { return; }

            await AudioFunctions.ConnectBot(Context, Context.Guild.Id);
        }



        //Leaves the current voice channel
        [Command("leave")]
        public async Task Leave()
        {
            if (Global.IsMusicChannel(Context) == false) return;

            ulong sId = Context.Guild.Id;

            Global.servers[sId].MusicRequests.Clear();

            Global.servers[sId].AudioVars.JoinedVoice = false;

            var clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id);
            await (clientUser as IGuildUser).VoiceChannel.DisconnectAsync();
        }



        //Current music request queue
        [Command("queue")]
        public async Task Queue()
        {
            ulong sId = Context.Guild.Id;

            if (Global.IsMusicChannel(Context) == false || Global.servers[sId].MusicRequests.Count == 0) return;

            //Embed builder for queued songs
            EmbedBuilder builder = new();

            int time = 0, i = 0;
            foreach (MusicRequest item in Global.servers[sId].MusicRequests.ToList())
            {
                if (i == 0) { builder.WithTitle("Currently Playing:\n" + item.Title + "\nRequested by:  " + item.User); builder.WithUrl(item.URL); builder.WithThumbnailUrl(item.Thumbnail); }

                else if (i < 10) { builder.AddField(i + ".  " + item.Title, "Requested by:  " + item.User, false); }

                TimeSpan youTubeDuration = XmlConvert.ToTimeSpan(item.Duration);
                time += Convert.ToInt32(youTubeDuration.TotalSeconds);

                i++;
            }

            int minute = time / 60;
            int hour = minute / 60;
            int sec = time - minute * 60;
            builder.AddField("Full duration:", (hour > 0 ? (hour + "h" + (minute - hour * 60) + "m" + sec + "s") : (minute + "m" + sec + "s")), true);

            builder.WithTimestamp(DateTime.Now);
            builder.WithColor(Color.Blue);

            await ReplyAsync("", false, builder.Build());
        }



        //The currently playing song
        [Command("np")]
        public async Task Now_Playing()
        {
            ulong sId = Context.Guild.Id;

            if (Global.IsMusicChannel(Context) == false || Global.servers[sId].MusicRequests.Count == 0) return;

            int elapsed = Convert.ToInt32(Global.servers[sId].AudioVars.Stopwatch.Elapsed.TotalSeconds);
            int hour = elapsed / 3600;
            int min = elapsed / 60 - hour * 60;
            int second = elapsed - min * 60 - hour * 3600;

            string elapsed_time = (hour != 0 ? "" + hour + "h" + min + "m" + second + "s" : "" + min + "m" + second + "s");

            await AudioFunctions.NpEmbed(Context, Global.servers[sId].MusicRequests[0], elapsed_time);
        }


        //Clear playlist and leave voice channel
        [Command("clear")]
        public async Task Clear()
        {
            try
            {
                if (Global.IsMusicChannel(Context) == false) return;

                ulong sId = Context.Guild.Id;

                Global.servers[sId].MusicRequests.Clear();

                await Context.Channel.SendMessageAsync("The queue has been cleared!");
                Global.Logs.Add(new Log("LOG", "The queue has been cleared!"));
                Global.servers[sId].AudioVars.FFmpeg.Kill();
                Global.servers[sId].AudioVars.Output.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "VoiceCommands.cs Clear", ex.ToString()));
            }
        }



        //Skip current song
        [Command("skip")]
        public async Task Skip()
        {
            try
            {
                if (Global.IsMusicChannel(Context) == false) return;

                ulong sId = Context.Guild.Id;

                await Context.Channel.SendMessageAsync("Song skipped!");

                if (Global.servers[sId].MusicRequests.Count > 0)
                {
                    Console.WriteLine("Next song found!");
                    Global.Logs.Add(new Log("LOG", "Next song found!"));
                }
                else
                {
                    Console.WriteLine("Empty list!");
                    Global.Logs.Add(new Log("LOG", "Empty list!"));
                }
                Global.servers[sId].AudioVars.FFmpeg.Kill();
                Global.servers[sId].AudioVars.Output.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "VoiceCommands.cs Skip", ex.ToString()));
            }
        }



        //Removes the song at the given position
        [Command("remove")]
        public async Task Remove(int position)
        {
            ulong sId = Context.Guild.Id;

            if (!Global.IsMusicChannel(Context) || position == 0 || position >= Global.servers[sId].MusicRequests.Count) return;

            MusicRequest item = Global.servers[sId].MusicRequests.ElementAt(position);
            if (item != null)
            {
                await ReplyAsync("`" + item.Title + "`" + "has been removed from the playlist!");
                Global.servers[sId].MusicRequests.Remove(item);
            }
        }
    }
}
