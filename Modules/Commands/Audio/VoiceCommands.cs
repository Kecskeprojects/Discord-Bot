﻿using System;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.Commands;
using System.Linq;
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
        public async Task Play([Remainder] string content)
        {
            try
            {
                ulong sId = Context.Guild.Id;

                if (Global.IsMusicChannel(Context) == false) return;

                if (!Global.servers[sId].AudioVars.Playing)
                {
                    Global.servers[sId].MusicRequests.Clear();

                    Global.servers[sId].AudioVars.Playing = true;

                    await AudioFunctions.RequestHandler(Context, content);

                    if (Global.servers[sId].MusicRequests.Count > 0)
                    {
                        try
                        {
                            await AudioFunctions.PlayHandler(Context, sId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
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
            ulong sId = Context.Guild.Id;

            if (Global.IsMusicChannel(Context) == false) { return; }

            await AudioFunctions.ConnectBot(Context, sId);
        }



        //Leaves the current voice channel
        [Command("leave")]
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
        public async Task Queue(int index = 1)
        {
            ulong sId = Context.Guild.Id;

            if (Global.IsMusicChannel(Context) == false || Global.servers[sId].MusicRequests.Count == 0) return;

            //Embed builder for queued songs
            EmbedBuilder builder = new();

            int time = 0;

            for (int i = (index - 1) * 10; i < Global.servers[sId].MusicRequests.Count; i++)
            {
                MusicRequest item = Global.servers[sId].MusicRequests[i];

                if (i == (index - 1) * 10) { builder.WithTitle("Currently Playing:\n" + item.Title + "\nRequested by:  " + item.User); builder.WithUrl(item.URL); builder.WithThumbnailUrl(item.Thumbnail); }

                else if (i < index + 10) { builder.AddField(i + ".  " + item.Title, "Requested by:  " + item.User, false); }

                TimeSpan youTubeDuration = XmlConvert.ToTimeSpan(item.Duration);
                time += Convert.ToInt32(youTubeDuration.TotalSeconds);

                i++;
            }
            
            if(time == 0) return;

            int hour = time / 3600;
            int minute = time / 60 - hour * 60; ;
            int second = time - minute * 60 - hour * 3600;
            builder.AddField("Full duration:", "" + (hour > 0 ? hour + "h" : "") + minute + "m" + second + "s", true);

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
            int minute = elapsed / 60 - hour * 60;
            int second = elapsed - minute * 60 - hour * 3600;

            string elapsed_time = "" + (hour > 0 ? hour + "h" : "") + minute + "m" + second + "s";

            await AudioFunctions.NpEmbed(Context, Global.servers[sId].MusicRequests[0], elapsed_time);
        }


        //Clear playlist and leave voice channel
        [Command("clear")]
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
        public async Task Remove(int position)
        {
            ulong sId = Context.Guild.Id;

            if (!Global.IsMusicChannel(Context) || position < 1 || position >= Global.servers[sId].MusicRequests.Count) return;

            await ReplyAsync("`" + Global.servers[sId].MusicRequests[position].Title + "` has been removed from the playlist!");
            Global.Logs.Add(new Log("LOG", "`" + Global.servers[sId].MusicRequests[position].Title + "` has been removed from the playlist!"));

            Global.servers[sId].MusicRequests.RemoveAt(position);
        }
    }
}
