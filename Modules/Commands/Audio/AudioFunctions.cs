﻿using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Modules.API;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Commands.Audio
{
    public class AudioFunctions : AudioService
    {
        //Possible result values 
        //-2 : spotify result found, youtube result not found
        //-1 : Result not found
        //0 : Result found
        //1 : Playlist found
        public static async Task RequestHandler(SocketCommandContext context, string input)
        {
            if (input == "") return;

            input = input.Replace("<", "").Replace(">", "");

            //In case of a spotify link, do a spotify API query before the youtube API query
            int result;
            if (input.Contains("spotify")) result = API.SpotifyAPI.SpotifySearch(context, input);
            else result = YoutubeAPI.Searching(context, input);

            //If search results come back empty, return
            if (result < 0) { await context.Channel.SendMessageAsync("No results found!"); return; }

            //Make embedded message if result was not a playlist and it's not the first song
            if (result != 1 && Global.servers[context.Guild.Id].MusicRequests.Count > 1) 
            { 
                await RequestEmbed(context); 
            }
        }



        //Handling the continuous playing of audio
        public static async Task PlayHandler(SocketCommandContext context, ulong sId)
        {
            //Check connection
            if (!await ConnectBot(context, sId)) return;

            while (Global.servers[sId].MusicRequests.Count > 0)
            {
                MusicRequest current = Global.servers[sId].MusicRequests[0];

                await context.Channel.SendMessageAsync("Now Playing:\n`" + current.Title + "`");

                //Streaming the music
                await Stream(context, Global.servers[sId].AudioVars.AudioClient, current.URL.Split('&')[0]);

                //Deleting the finished song
                if(Global.servers[sId].MusicRequests.Count > 0)
                {
                    Global.servers[sId].MusicRequests.RemoveAt(0);
                }

                //If the playlist is empty and there is no song playing, start counting down
                if (Global.servers[sId].MusicRequests.Count == 0)
                {
                    Console.WriteLine("Playlist empty!");
                    Global.Logs.Add(new Log("LOG", "Playlist empty!"));

                    //In case counter reached it's limit, disconnect
                    int j = 0; 
                    IGuildUser clientUser;
                    while (Global.servers[sId].MusicRequests.Count == 0 && j < 60)
                    {
                        j++;

                        clientUser = await context.Channel.GetUserAsync(context.Client.CurrentUser.Id) as IGuildUser;
                        if (clientUser.VoiceChannel == null)
                        {
                            Global.servers[sId].AudioVars.JoinedVoice = false;
                            break;
                        }

                        await Task.Delay(1000);
                    }

                    //In case counter reached it's limit, disconnect
                    if (j > 59 || Global.servers[sId].AudioVars.JoinedVoice == false)
                    {
                        clientUser = await context.Channel.GetUserAsync(context.Client.CurrentUser.Id) as IGuildUser;

                        if (j > 59 && clientUser.VoiceChannel != null)
                        {
                            await context.Channel.SendMessageAsync("`Disconnected due to inactivity.`");

                            await clientUser.VoiceChannel.DisconnectAsync();
                        }

                        Global.servers[sId].MusicRequests.Clear();

                        break;
                    }
                }
            }
        }



        //Handling Bot Voice connection
        public static async Task<bool> ConnectBot(SocketCommandContext context, ulong sId)
        {
            try
            {
                var clientUser = await context.Channel.GetUserAsync(context.Client.CurrentUser.Id) as IGuildUser;

                if (Global.servers[sId].AudioVars.JoinedVoice == false)
                {
                    if(clientUser.VoiceChannel != null) 
                    {
                        await clientUser.VoiceChannel.DisconnectAsync(); 
                    }

                    IVoiceChannel channel = (context.User as SocketGuildUser).VoiceChannel;

                    if(channel != null)
                    {
                        Global.servers[sId].AudioVars.AudioClient = await channel.ConnectAsync();

                        if (Global.servers[sId].AudioVars.AudioClient != null)
                        {
                            Global.servers[sId].AudioVars.JoinedVoice = true;
                            return true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("User must be in a voice channel, or a voice channel must be passed as an argument!");
                        Global.Logs.Add(new Log("LOG", "User must be in a voice channel, or a voice channel must be passed as an argument!"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "AudioService.cs Check_Connection", ex.ToString()));
            }
            return false;
        }



        //Embed making for song request in voice chat
        public static async Task RequestEmbed(SocketCommandContext context)
        {
            MusicRequest request = Global.servers[context.Guild.Id].MusicRequests.Last();
            int count = Global.servers[context.Guild.Id].MusicRequests.Count;

            //Embed builder for queued songs
            EmbedBuilder builder = new();
            builder.WithTitle(request.Title);
            builder.WithUrl(request.URL);

            builder.WithDescription("Song has been added to the queue!");

            builder.WithThumbnailUrl(request.Thumbnail);

            builder.AddField("Song duration:", request.Duration.ToLower(), true);

            builder.AddField("Position in queue:", count - 1, true);


            builder.WithTimestamp(DateTime.Now);
            builder.WithColor(Color.Red);

            await context.Channel.SendMessageAsync("", false, builder.Build());
        }



        //Embed making for currently playing song in voice chat
        public static async Task NpEmbed(SocketCommandContext Context, MusicRequest item, string elapsed_time)
        {
            EmbedBuilder builder = new();
            builder.WithTitle(item.Title);
            builder.WithUrl(item.URL);

            builder.WithThumbnailUrl(item.Thumbnail);

            builder.AddField("Requested by:", item.User, false);
            builder.AddField("Song duration:", elapsed_time + " / " + item.Duration.ToLower(), false);

            builder.WithTimestamp(DateTime.Now);
            builder.WithColor(Color.DarkBlue);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}
