using Discord;
using Discord.Commands;
using Discord.Rest;
using System;
using System.Threading.Tasks;
using Discord_Bot.Modules;
using Discord_Bot.Modules.ListClasses;
using Discord_Bot.Modules.Database;
using System.IO;
using Discord.WebSocket;
using System.Linq;

namespace Discord_Bot
{
    public class ProgramFunctions
    {
        //Check the list of custom commands on the server
        public static async Task CustomCommands(SocketCommandContext context)
        {
            try
            {
                var command = DBFunctions.CustomCommandGet(context.Guild.Id, context.Message.Content[1..].ToLower());

                if (command != null) 
                {
                    await context.Message.Channel.SendMessageAsync(command.URL);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ProgramFunctions.cs CustomCommands", ex.ToString()));
            }
        }

        

        //Add/Remove roles from users, and keep the role chat clean
        public static async Task SelfRole(SocketCommandContext context)
        {
            try
            {
                RestUserMessage reply = null;

                var role = DBFunctions.SelfRoleGet(context.Guild.Id, context.Message.Content[1..].ToLower());

                if (role != null)
                {
                    IRole get_role = (context.Channel as IGuildChannel).Guild.GetRole(role.RoleId);

                    switch (context.Message.Content[0])
                    {
                        case '+':
                            {
                                await (context.User as IGuildUser).AddRoleAsync(get_role);
                                reply = await context.Channel.SendMessageAsync("You now have the `" + role.RoleName + "` role!");
                                break;
                            }
                        case '-':
                            {
                                await (context.User as IGuildUser).RemoveRoleAsync(get_role);
                                reply = await context.Channel.SendMessageAsync("`" + role.RoleName+ "` role has been removed!");
                                break;
                            }
                    }
                }

                if (reply != null)
                {
                    await Task.Delay(2000);

                    await reply.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ProgramFunctions.cs SelfRole", ex.ToString()));
            }
        }



        //Check for messages starting with I think and certain Keywords
        public static async Task FeatureCheck(SocketCommandContext context)
        {
            try
            {
                if (new Random().Next(1, 101) < 10)
                {
                    string mess = context.Message.Content.ToLower();
                    if (mess.StartsWith("i think")) { await context.Channel.SendMessageAsync("I agree wholeheartedly!"); return; }

                    else if (mess.StartsWith("i am") || mess.StartsWith("i'm"))
                    {
                        await context.Channel.SendMessageAsync(string.Concat("Hey ", context.Message.Content.AsSpan(mess.StartsWith("i am") ? 5 : 4), ", I'm Kim Synthji!"));
                        return;
                    }
                }

                if(context.Message.Content.Length <= 100)
                {
                    var keyword = DBFunctions.KeywordGet(context.Guild.Id, context.Message.Content.Replace("\'" , "").Replace("\"", "").Replace("`", "").Replace(";", ""));
                    if (keyword != null)
                    {
                        await context.Channel.SendMessageAsync(keyword.Response);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ProgramFunctions.cs FeatureCheck", ex.ToString()));
            }
        }



        //For logging messages, errors, and messages to log files
        static StreamWriter LogFile_writer = null;
        public static void LogToFile()
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
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ProgramFunctions.cs LogtoFile", ex.ToString()));
            }
        }



        //Copy database to Assets\Data folder, done once a day and a minute after starting the bot
        public static void DatabaseBackup()
        {
            try
            {
                File.Copy("database.db", Path.Combine(Directory.GetCurrentDirectory(),
                    $"Assets\\Data\\database_"
                    + (DateTime.Now.Year < 10 ? "0" + DateTime.Now.Year : DateTime.Now.Year)
                    + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month)
                    + (DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day : DateTime.Now.Day)
                    + (DateTime.Now.Hour < 10 ? "0" + DateTime.Now.Hour : DateTime.Now.Hour)
                    + (DateTime.Now.Minute < 10 ? "0" + DateTime.Now.Minute : DateTime.Now.Minute)
                    + ".db"));
                Console.WriteLine("Database backup created!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ProgramFunctions.cs Log DatabaseBackup", ex.ToString()));
            }
        }



        //Checking and sending out reminders
        public static async Task ReminderCheck(DiscordSocketClient Client)
        {
            try
            {
                //Get the list of reminders that are before or exactly set to this minute
                //Also format date to sql compatible format
                var result = DBFunctions.ReminderList(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                if (result.Count > 0)
                {
                    foreach (var reminder in result)
                    {
                        //Modify message
                        reminder.Message = reminder.Message.Insert(0, $"You told me to remind you at `{reminder.Date}` with the following message:\n\n");

                        //Try getting user
                        var user = await Client.GetUserAsync(reminder.UserId);

                        //If user exists send a direct message to the user
                        if(user != null)
                        {
                            await UserExtensions.SendMessageAsync(user, reminder.Message);
                        }

                        //Delete the user regardless of the outcome, unless an error occurs of course, keep it in that case
                        //Also format date to sql compatible format
                        DBFunctions.ReminderRemove(reminder.UserId, reminder.Date.ToString("yyyy-MM-dd HH:mm"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ProgramFunctions.cs Log ReminderCheck", ex.ToString()));
            }
        }
    }
}
