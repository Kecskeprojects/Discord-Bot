using Discord;
using Discord.WebSocket;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace Discord_Bot.Modules.API
{
    public class TwitchAPI
    {
        //Saved variables
        private LiveStreamMonitorService Monitor;
        private TwitchLib.Api.TwitchAPI API;
        private static DiscordSocketClient Client;
        private static string Token;



        //Running the Twitch api request and catching errors
        public static void Twitch(DiscordSocketClient client)
        {
            try
            {
                Client = client;
                Token = Generate_Token();

                Console.WriteLine("Twitch function called!");
                Global.Logs.Add(new Log("QUERY", "Twitch function called!"));

                new TwitchAPI().Check().Wait();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("ERROR", "TwitchAPI.cs Twitch", ex.ToString()));
                Global.Logs.Add(new Log("DEV", ex.Message));
            }
        }



        //The main function that keeps checking the stream state
        private async Task Check()
        {
            Console.WriteLine("Monitoring starting!");
            Global.Logs.Add(new Log("QUERY", "Monitoring starting!"));

            API = new TwitchLib.Api.TwitchAPI();

            API.Settings.ClientId = Global.Config.Twitch_Client_Id;
            API.Settings.AccessToken = Token;

            Monitor = new LiveStreamMonitorService(API);

            List<string> lst = new();

            foreach (var item in Global.servers)
            {
                lst.Add(item.Value.TChannelId);
            }
            
            Monitor.SetChannelsById(lst);

            Monitor.OnStreamOnline += Monitor_OnStreamOnline;
            Monitor.OnStreamOffline += Monitor_OnStreamOffline;
            Monitor.OnServiceTick += Monitor_OnServiceTick;

            Monitor.Start(); //Keep at the end!

            await Task.Delay(-1);
        }

        //Make announcement when stream comes online
        private void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine("Streamer user id test: " + e.Stream.UserId);
            foreach (var server in Global.servers)
            {
                if (!server.Value.TwitchOnline && server.Value.TChannelId == e.Stream.UserId)
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": Stream is now online!");
                    Global.Logs.Add(new Log("QUERY", "Stream is now online!"));

                    var channel = Client.GetChannel(server.Value.TNotifChannel) as IMessageChannel;

                    string thumbnail = e.Stream.ThumbnailUrl.Replace("{width}", "1024").Replace("{height}", "576");
                    Console.WriteLine(thumbnail);

                    EmbedBuilder builder = new();
                    builder.WithTitle("Stream is now online!");
                    builder.AddField(e.Stream.Title, server.Value.TChannelLink, false);
                    builder.WithImageUrl(thumbnail);
                    builder.WithTimestamp(e.Stream.StartedAt);

                    builder.WithColor(Color.Purple);
                    channel.SendMessageAsync($"<@&{server.Value.TNotifRole}>", false, builder.Build());
                    server.Value.TwitchOnline = true;
                }
            }
        }



        //Make console message when stream goes offline
        private void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine(Global.Current_Time() + ": Stream is now offline!");
            Global.Logs.Add(new Log("QUERY", "Stream is now offline!"));

            foreach (var server in Global.servers)
            {
                if (server.Value.TwitchOnline && server.Value.TChannelId == e.Stream.UserId)
                {
                    server.Value.TwitchOnline = false;
                }
            }
        }



        //On every tick(3 minutes), send message on console, every 24 hours, refresh token and reset counter
        private static int Token_tick = 0;
        private void Monitor_OnServiceTick(object sender, OnServiceTickArgs e)
        {
            Token_tick++;
            if(Token_tick > 1440) { Token = Generate_Token(); Token_tick = 0; }
            if(Token_tick % 120 == 0)
            {
                Console.WriteLine("========================");
                Console.WriteLine(Global.Current_Time() + ": 120 queries have been completed!");
                Console.WriteLine("========================");
                Global.Logs.Add(new Log("QUERY", "120 queries have been completed!"));
            }
        }



        //Responsible for generating the access tokens to Twitch's api requests
        private static string Generate_Token()
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = "/C twitch.exe token",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            string response = process.StandardError.ReadToEnd();
            process.WaitForExit();
            response = response.Substring(response.IndexOf("Token: ") + 7, 30);
            Console.WriteLine("Twitch API token: " + response);
            Global.Logs.Add(new Log("QUERY", "Twitch API token: " + response));
            return response;
        }
    }
}