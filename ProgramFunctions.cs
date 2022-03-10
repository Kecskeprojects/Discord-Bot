using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.Classes;
using Discord_Bot.Database;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Timers;

namespace Discord_Bot
{
    public class ProgramFunctions
    {
        //Testing connection by pinging google
        public static bool Connection()
        {
            try
            {
                if (new System.Net.NetworkInformation.Ping()
                    .Send("google.com", 1000, new byte[32], new System.Net.NetworkInformation.PingOptions()).Status ==
                    System.Net.NetworkInformation.IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }



        //Check the list of custom commands on the server
        public static async Task<bool> Custom_Commands(SocketCommandContext context)
        {
            try
            {
                //CustomCommand command = Global.Custom_commands.Find(n => n.server_id == context.Guild.Id && n.command_name == context.Message.Content.Substring(1).ToLower());
                var table = DBManagement.Read($"SELECT `url` FROM `customcommand` WHERE `serverId` = '{context.Guild.Id}' AND `command` = '{context.Message.Content[1..].ToLower()}'");

                if (table != null) 
                {
                    await context.Message.Channel.SendMessageAsync(table.Rows[0][0].ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Custom_Commands", ex.ToString()));
            }
            return false;
        }

        

        //Add/Remove roles from users, and keep the role chat clean
        public static async Task Self_role(SocketCommandContext context)
        {
            try
            {
                RestUserMessage reply = null;

                var table = DBManagement.Read($"SELECT * FROM `role` WHERE `serverId` = '{context.Guild.Id}' AND `roleName` = '{context.Message.Content[1..].ToLower()}'");

                if (table != null)
                {
                    string name = table.Rows[0][1].ToString();
                    ulong id = ulong.Parse(table.Rows[0][2].ToString());

                    IRole get_role = (context.Channel as IGuildChannel).Guild.GetRole(id);

                    switch (context.Message.Content[0])
                    {
                        case '+':
                            {
                                await (context.User as IGuildUser).AddRoleAsync(get_role);
                                reply = await context.Channel.SendMessageAsync("You now have the `" + name + "` role!");
                                break;
                            }
                        case '-':
                            {
                                await (context.User as IGuildUser).RemoveRoleAsync(get_role);
                                reply = await context.Channel.SendMessageAsync("`" + name + "` role has been removed!");
                                break;
                            }
                    }
                }

                await context.Message.DeleteAsync();

                if (reply != null)
                {
                    await Task.Delay(1500);

                    await reply.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't add/delete role!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Self_role", ex.ToString()));
            }
        }



        //Check for messages starting with I think and certain Keywords
        public static async Task<bool> Feature_Check(SocketUserMessage message)
        {
            try
            {
                if (new Random().Next(1, 101) < 10)
                {
                    string mess = message.Content.ToLower();
                    if (mess.StartsWith("i think")) { await message.Channel.SendMessageAsync("I agree wholeheartedly!"); return true; }

                    else if (mess.StartsWith("i am") || mess.StartsWith("i'm"))
                    {
                        await message.Channel.SendMessageAsync(string.Concat("Hey ", message.Content.AsSpan(mess.StartsWith("i am") ? 5 : 4), ", I'm Kim Synthji!"));
                        return true;
                    }
                }
                else
                {
                    var table = DBManagement.Read($"SELECT `response` FROM `keyword` WHERE `trigger` = '{message.Content}'");
                    if (table != null) 
                    {
                        await message.Channel.SendMessageAsync(table.Rows[0][0].ToString()); 
                        return true; 
                    }
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Feature_Check", ex.ToString()));
            }
            return false;
        }



        //What the bot does every minute
        static int minutes_count = 0;
        public static async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            
            if(minutes_count == 1440) minutes_count = 0;

            if (minutes_count == 0)
            {
                try
                {
                    File.Copy("database.db", Path.Combine(Directory.GetCurrentDirectory(), $"Assets\\Data\\database_{"" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day}.db"));
                    Console.WriteLine("Database backup created!");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Something went wrong!\n" + ex.ToString());
                    Global.Logs.Add(new Log("DEV", ex.Message));
                    Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Log OnTimedEvent", ex.ToString()));
                }
            }

            minutes_count++;

            Log_to_file();

            await Task.CompletedTask;
        }



        //For logging messages, errors, and messages to log files
        static StreamWriter LogFile_writer = null;
        public static void Log_to_file()
        {
            try
            {
                if (Global.Logs.Count != 0 && LogFile_writer == null)
                {
                    string file_location = "Logs\\logs" + "[" + DateTime.Now.Year + "-" + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString()) + "-" + (DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString()) + "].txt";

                    using (LogFile_writer = File.AppendText(file_location)) foreach (string log in Global.Logs.Select(n => n.Content)) LogFile_writer.WriteLine(log);

                    LogFile_writer = null;
                    Global.Logs.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Log to File", ex.ToString()));
            }
        }
    }
}
