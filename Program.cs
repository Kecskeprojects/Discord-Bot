using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Modules;
using Discord_Bot.Modules.API;
using Discord_Bot.Modules.Database;
using Discord_Bot.Modules.ListClasses;
using Microsoft.Extensions.DependencyInjection;

namespace Discord_Bot
{
    class Program
    {
        //The main program, runs even if the bot crashes, and restarts it
        static void Main()
        {
            //Event handler for the closing of the app
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(StartupFunctions.Closing);

            //Constant timer, only stopping when the Bot's process stops
            System.Timers.Timer aTimer = new(60000) { AutoReset = true }; //1 minute

            aTimer.Elapsed += new ElapsedEventHandler(StartupFunctions.OnTimedEvent);

            while (true)
            {
                if (StartupFunctions.Connection())
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

                //Waiting 1 minute before checking connection again
                Thread.Sleep(60000);
            }
        }



        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;



        //Main Service
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(
                new DiscordSocketConfig() { LargeThreshold = 250,
                                            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildBans | 
                                            GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations | GatewayIntents.GuildWebhooks | 
                                            GatewayIntents.GuildVoiceStates | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | 
                                            GatewayIntents.GuildMessageTyping | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageReactions | 
                                            GatewayIntents.DirectMessageTyping });
            _commands = new CommandService(new CommandServiceConfig() { DefaultRunMode = RunMode.Async });
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            StartupFunctions.DBCheck();
            StartupFunctions.Check_Folders();
            StartupFunctions.ServerList();

            YoutubeAPI.KeyReset();

            _client.Log += ClientLog;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, Global.Config.Token);

            await _client.StartAsync();

            new Thread(() =>
            {
                TwitchAPI.Twitch(_client);
            }).Start();

            await Task.Delay(-1);
        }
        


        //Client Messages
        private Task ClientLog(LogMessage arg)
        {
            if (arg.Exception != null)
            {
                switch (arg.Exception.Message)
                {
                    case "Server requested a reconnect":
                    {
                            Console.WriteLine(arg.Exception.Message + "!");
                            Global.Logs.Add(new Log("CLIENT", arg.Exception.Message + "!"));
                            break;
                    }
                    case "WebSocket connection was closed":
                    case "WebSocket session expired":
                        {
                            Console.WriteLine(arg.Exception.Message + "!");
                            Global.Logs.Add(new Log("CLIENT", arg.Exception.Message + "!"));
                            Global.Logs.Add(new Log("DEV", arg.Exception.ToString()));
                            Global.Logs.Add(new Log("DEV", arg.ToString()));
                            break;
                    }
                    default:
                    {
                            Console.WriteLine(arg.Exception);
                            Global.Logs.Add(new Log("DEV", arg.Exception.Message));
                            Global.Logs.Add(new Log("ERROR", "Program.cs ClientLog", arg.ToString()));
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine(arg.ToString());
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
            if (message.Content.Length < 1) return;
            else Global.Logs.Add(new Log("MES_USER", message.Content));


            //If message is not private message, and the server is not on the list, add it to the database and the list
            if (message.Channel.GetChannelType() != ChannelType.DM && !Global.servers.ContainsKey(context.Guild.Id))
            {
                int affected = DBFunctions.AddNewServer(context.Guild.Id);

                if (affected > 0)
                {
                    Global.servers.Add(context.Guild.Id, new Server(context.Guild.Id));

                    Console.WriteLine($"{context.Guild.Name} added to the server list!");
                    Global.Logs.Add(new Log("LOG", $"{context.Guild.Name} added to the server list!"));
                }
                else
                {
                    Console.WriteLine($"{context.Guild.Name} could not be added to list!");
                    Global.Logs.Add(new Log("ERROR", $"{context.Guild.Name} could not be added to list!"));
                }
            }


            if (message.HasCharPrefix('!', ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                //In case there is no such hard coded command, check the list of custom commands
                if (!result.IsSuccess)
                {
                    if (result.ErrorReason == "Unknown command.")
                    {
                        await ProgramFunctions.CustomCommands(context);
                        return;
                    }
                    else
                    {
                        Console.WriteLine(result.ErrorReason);
                        if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);

                        Global.Logs.Add(new Log("ERROR", "Program.cs HandleCommandAsync", result.Error.ToString()));
                    }
                }
            }
            else if (message.Channel.GetChannelType() == ChannelType.Text && Global.servers[context.Guild.Id].RoleChannel == context.Channel.Id)
            {
                if (message.HasCharPrefix('+', ref argPos) || message.HasCharPrefix('-', ref argPos))
                {
                    //self roles
                    _ = ProgramFunctions.SelfRole(context);
                }

                await context.Message.DeleteAsync();
            }
            else
            {
                //Response to mention
                if (message.Content.Contains(_client.CurrentUser.Mention) || message.Content.Contains(_client.CurrentUser.Mention.Remove(2, 1)))
                {
                    var list = DBFunctions.AllGreeting();
                    if (list.Count > 0)
                    {
                        await message.Channel.SendMessageAsync(list[new Random().Next(0, list.Count)].URL);
                    }
                }
                //Responses to triggers
                else _ = ProgramFunctions.FeatureCheck(context);

            }

            await Task.CompletedTask;
        }
    }
}
