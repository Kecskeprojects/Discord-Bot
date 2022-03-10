using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Database;
using Discord_Bot.Classes;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Discord_Bot
{
    class Program
    {
        //The main program, runs even if the bot crashes, and restarts it
        static void Main()
        {
            System.Timers.Timer aTimer = new(60000) { AutoReset = true }; //1 minute
            aTimer.Elapsed += new ElapsedEventHandler(ProgramFunctions.OnTimedEvent);

            while (true)
            {
                if (ProgramFunctions.Connection())
                {
                    aTimer.Start();

                    Console.WriteLine("Application starting...");

                    try
                    {
                        new Program().RunBotAsync().GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Global.Logs.Add(new Log("DEV", ex.Message));
                        Global.Logs.Add(new Log("ERROR", "Program.cs Main", ex.ToString()));
                    }

                    aTimer.Stop();
                }

                //Waiting 5 minutes before checking connection again
                Thread.Sleep(300000);
            }
        }



        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        public static ConfigHandler Config;



        //Main Service
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig() { LogGatewayIntentWarnings = false, LogLevel = LogSeverity.Info });
            _commands = new CommandService(new CommandServiceConfig() { DefaultRunMode = RunMode.Async });
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<ConfigHandler>()
                //.AddSingleton<AudioService>()
                .BuildServiceProvider();

            StartupFunctions.DBCheck();
            StartupFunctions.Check_Folders();
            StartupFunctions.ServerList();

            await _services.GetService<ConfigHandler>().PopulateConfig();

            Config = _services.GetService<ConfigHandler>();

            _client.Log += Client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, Config.Token);

            await _client.StartAsync();

            /*new Thread(delegate ()
            {
                Twitch_API.Twitch(_services.GetService<ConfigHandler>().Get_Default_Twitch_Notif_Channel(), _client, _services.GetService<ConfigHandler>().Get_Default_Twitch_Notif_Role());
            }).Start();*/

            await Task.Delay(-1);
        }
        


        //Client Messages
        private Task Client_Log(LogMessage arg)
        {
            if (arg.Exception != null)
            {
                switch (arg.Exception.Message)
                {
                    case "Server requested a reconnect":
                    case "WebSocket connection was closed":
                    case "WebSocket session expired":
                    {
                            Console.WriteLine(arg.Exception.Message + "!");
                            Global.Logs.Add(new Log("CLIENT", arg.Exception.Message + "!"));
                            break;
                    }
                    default:
                    {
                            Console.WriteLine("Something went wrong!\n" + arg.Exception);
                            Global.Logs.Add(new Log("DEV", arg.Exception.Message));
                            Global.Logs.Add(new Log("ERROR", "Program.cs Client_Log", arg.ToString()));
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine(arg);
                Global.Logs.Add(new Log("CLIENT", arg.ToString()));
            }
            return Task.CompletedTask;
        }



        //Watching Messages
        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }



        //Handling commands and special cases
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            //In case the message was a system message (eg. the message seen when someone a pin is made), a webhook's or a bot's message, the function stops
            if (arg.Source == MessageSource.System || arg.Source == MessageSource.Webhook || arg.Source == MessageSource.Bot) return;


            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            int argPos = 0;


            //Check if the message is an embed or not
            if (message.Content.Length < 1) { Global.Logs.Add(new Log("MES_USER", "Embedded message")); return; }
            else Global.Logs.Add(new Log("MES_USER", message.Content));

            if (context.Guild == null) { await DirectMessageHandler.Handle(context); return; }


            //If the server is not on the list, add it to the database and the list
            if (!Global.servers.Any(x => x.ServerId == context.Guild.Id))
            {
                DBManagement.Insert($"INSERT INTO `serversetting`(`serverId`, `musicChannel`, `roleChannel`, `tNotifChannel`, `tNotifRole`) VALUES ('{context.Guild.Id}',0,0,0,0)");
                Global.servers.Add(new ServerSetting(context.Guild.Id));

                Console.WriteLine($"{context.Guild.Name} added to the server list!");
                Global.Logs.Add(new Log("LOG", $"{context.Guild.Name} added to the server list!"));
            }


            if(message.HasCharPrefix('!', ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                //In case there is no such hard coded command, check the list of custom commands
                if (!result.IsSuccess)
                {
                    if (await ProgramFunctions.Custom_Commands(context)) return;

                    Console.WriteLine(result.ErrorReason);
                    if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);

                    Global.Logs.Add(new Log("ERROR", "Program.cs HandleCommandAsync", result.Error.ToString()));
                }
            }
            else if(message.HasCharPrefix('+', ref argPos) || message.HasCharPrefix('-', ref argPos))
            {
                var table = DBManagement.Read($"SELECT `roleChannel` FROM `serversetting` WHERE `serverId` = '{context.Channel.Id}'");
                if(table != null)
                {
                    //self roles
                    await ProgramFunctions.Self_role(context);
                }
            }
            else
            {
                //Responses to triggers
                if (!await ProgramFunctions.Feature_Check(message) && (message.Content.Contains(_client.CurrentUser.Mention) || message.Content.Contains(_client.CurrentUser.Mention.Remove(2, 1))))
                {
                    var table = DBManagement.Read("SELECT * FROM `greeting`");
                    await message.Channel.SendMessageAsync(table.Rows[new Random().Next(0, table.Rows.Count)][0].ToString());
                }
            }

            await Task.CompletedTask;
        }

        
        //Checking for music channel, come back to it when you get to that point

        /*if (server != -1 && (Global.Server_settings[server].music_channel == context.Channel.Id 
         * || Global.Server_settings[server].music_channel == 0)) Global.Is_music_channel = true;
        else Global.Is_music_channel = false;*/


        /*public static ServerSetting Which_server(SocketCommandContext context) 
        //Which server the command came from
        {
            try
            {
                var table = Management.Read($"SELECT * in `serversetting` WHERE `serverId` = {context.Guild.Id}");
                if(table != null)
                {
                    return new ServerSetting(table);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Log to File", ex.ToString()));
            }
            return null;
        }*/
    }
}
