using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.Commands;
using Discord_Bot.Modules.ListClasses;

namespace Discord_Bot.Modules.Commands.Audio
{
    public class VoiceCommands : ModuleBase<SocketCommandContext>, Interfaces.IVoiceCommands
    {
        //
        //VOICE CHAT COMMANDS
        //


        //Play music on the channel the user is connected to
        [Command("p")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "play" })]
        public async Task Play([Remainder] string content)
        {
            try
            {
                ulong sId = Context.Guild.Id;

                if (Global.IsMusicChannel(Context) == false) return;

                if (!Global.servers[sId].AudioVars.Playing)
                {
                    Global.servers[sId].AudioVars.Playing = true;

                    await AudioFunctions.RequestHandler(Context, content);

                    if (Global.servers[sId].MusicRequests.Count > 0)
                    {
                        await AudioFunctions.PlayHandler(Context, sId);
                    }

                    Global.servers[sId].AudioVars.Playing = false;

                    Global.servers[sId].AudioVars = new AudioVars();
                    Global.servers[sId].MusicRequests.Clear();
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
        [RequireContext(ContextType.Guild)]
        public async Task Join()
        {
            ulong sId = Context.Guild.Id;

            if (Global.IsMusicChannel(Context) == false) { return; }

            await AudioFunctions.ConnectBot(Context, sId);
        }



        //Leaves the current voice channel
        [Command("leave")]
        [RequireContext(ContextType.Guild)]
        public async Task Leave()
        {
            ulong sId = Context.Guild.Id;

            if (Global.IsMusicChannel(Context) == false) return;

            Global.servers[sId].MusicRequests.Clear();

            var clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id);
            await (clientUser as IGuildUser).VoiceChannel.DisconnectAsync();

            Global.servers[sId].AudioVars.FFmpeg.Kill();
            Global.servers[sId].AudioVars.Output.Dispose();
        }



        //Current music request queue
        [Command("queue")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "q" })]
        public async Task Queue(int index = 1)
        {
            ulong sId = Context.Guild.Id;

            if (Global.IsMusicChannel(Context) == false || Global.servers[sId].MusicRequests.Count == 0) return;

            int songcount = Global.servers[sId].MusicRequests.Count;

            //If queue does not have songs on that page, do not show a queue
            if (index * 10 <= songcount || ((index -1) * 10 < songcount && index * 10 >= songcount))
            {
                //Embed builder for queued songs
                EmbedBuilder builder = new();

                builder.WithTitle($"Queue (page {index} of {Math.Ceiling((songcount - 1) / 10.0)}):");

                int time = 0;
                for (int i = 0; i < Global.servers[sId].MusicRequests.Count; i++)
                {
                    MusicRequest item = Global.servers[sId].MusicRequests[i];

                    if (i == 0)
                    {
                        builder.AddField("\u200b", $"__Currently Playing:__\n**[{item.Title}]({item.URL})**\nRequested by:  {item.User}", false);
                        builder.WithThumbnailUrl(item.Thumbnail);
                    }

                    //Check if song index is smaller than the given page's end but also larger than it is beginning
                    else if (i <= index * 10 && i > (index - 1) * 10) 
                    { 
                        builder.AddField("\u200b", $"**{i}. [{item.Title}]({item.URL})**\nRequested by:  {item.User}", false); 
                    }

                    TimeSpan youTubeDuration = XmlConvert.ToTimeSpan(item.Duration);
                    time += Convert.ToInt32(youTubeDuration.TotalSeconds);
                }

                int hour = time / 3600;
                int minute = time / 60 - hour * 60; ;
                int second = time - minute * 60 - hour * 3600;
                builder.AddField("Full duration:", "" + (hour > 0 ? hour + "h" : "") + minute + "m" + second + "s", true);

                builder.WithTimestamp(DateTime.Now);
                builder.WithColor(Color.Blue);

                await ReplyAsync("", false, builder.Build());
            }
        }



        //The currently playing song
        [Command("np")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "now playing", "nowplaying" })]
        public async Task Now_Playing()
        {
            ulong sId = Context.Guild.Id;

            if (Global.IsMusicChannel(Context) == false || Global.servers[sId].MusicRequests.Count == 0) return;

            int elapsed = Convert.ToInt32(Global.servers[sId].AudioVars.Stopwatch.Elapsed.TotalSeconds);
            int hour = elapsed / 3600;
            int minute = elapsed / 60 - hour * 60;
            int second = elapsed - minute * 60 - hour * 3600;

            string elapsed_time = "" + (hour > 0 ? hour + "h" : "") + minute + "m" + second + "s";

            await AudioFunctions.NpEmbed(Context, Global.servers[sId].MusicRequests[0], elapsed_time);
        }


        //Clear playlist and leave voice channel
        [Command("clear")]
        [RequireContext(ContextType.Guild)]
        public async Task Clear()
        {
            try
            {
                ulong sId = Context.Guild.Id;

                if (Global.IsMusicChannel(Context) == false || Global.servers[sId].MusicRequests.Count == 0) return;

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
        [RequireContext(ContextType.Guild)]
        public async Task Skip()
        {
            try
            {
                ulong sId = Context.Guild.Id;

                if (Global.IsMusicChannel(Context) == false || Global.servers[sId].MusicRequests.Count == 0) return;

                await Context.Channel.SendMessageAsync("Song skipped!");
                Global.Logs.Add(new Log("LOG", "Song skipped!"));

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
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "r" })]
        public async Task Remove(int position)
        {
            ulong sId = Context.Guild.Id;

            if (!Global.IsMusicChannel(Context) || position < 1 || position >= Global.servers[sId].MusicRequests.Count) return;

            await ReplyAsync("`" + Global.servers[sId].MusicRequests[position].Title + "` has been removed from the playlist!");
            Global.Logs.Add(new Log("LOG", "`" + Global.servers[sId].MusicRequests[position].Title + "` has been removed from the playlist!"));

            Global.servers[sId].MusicRequests.RemoveAt(position);
        }


        //Shuffle the current playlist
        [Command("shuffle")]
        [RequireContext(ContextType.Guild)]
        public async Task Shuffle()
        {
            try
            {
                //Get the server's playlist, and remove the currently playing song, but saving it for later
                List<MusicRequest> current = Global.servers[Context.Guild.Id].MusicRequests;
                MusicRequest nowPlaying = current[0];
                current.RemoveAt(0);

                List<MusicRequest> shuffled = new();
                int length = current.Count;
                Random r = new();

                //Go through the entire playlist once
                for (int i = 0; i < length; i++)
                {
                    //generate a random number, accounting for the slowly depleting current playlist
                    int index = r.Next(0, current.Count);

                    //Adding the randomly chosen song and removing it from the original list
                    shuffled.Add(current[index]);
                    current.RemoveAt(index);
                }

                //Adding back the currently playing song to the beginning and switching it out with the unshuffled one
                shuffled.Insert(0, nowPlaying);
                Global.servers[Context.Guild.Id].MusicRequests = shuffled;

                await ReplyAsync("Shuffle complete!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "VoiceCommands.cs Shuffle", ex.ToString()));
            }
        }
    }
}
