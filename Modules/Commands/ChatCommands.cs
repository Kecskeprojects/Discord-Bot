using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord_Bot.Modules.Database;
using Discord_Bot.Modules.ListClasses;

namespace Discord_Bot.Modules.Commands
{
    public class ChatCommands : ModuleBase<SocketCommandContext>, Interfaces.IChatCommands
    {
        //Array for 8ball answers
        public readonly string[] answers_8ball = { "It is certain", "It is decidedly so", "Without a doubt", "Yes, definitely", "You may rely on it", "As I see it, yes", "Most likely"
                                                 , "Outlook good", "Yes", "Signs point to yes", "Reply hazy try again", "Ask again later", "Better not tell you now", "Cannot predict now"
                                                 , "Concentrate and ask again", "Don't count on it", "My reply is no", "My sources say no", "Outlook not so good", "Very doubtful" };

        //
        //TEXT CHAT COMMANDS
        //


        //8ball game, takes in any number of arguments, returns random 8ball answer from array
        [Command("8ball")]
        public async Task Eightball([Remainder] string question)
        {
            if (question.Length == 0) { await ReplyAsync("Ask me about something!"); return; }

            await ReplyAsync(answers_8ball[new Random().Next(0, answers_8ball.Length)]);
        }



        //Command to list out all the currently available commands
        [Command("custom list")]
        [RequireContext(ContextType.Guild)]
        public async Task CustomList()
        {
            var list = DBFunctions.AllCustomCommand(Context.Guild.Id);
            if (list.Count == 0) await ReplyAsync("There are no custom commands on this server!");

            try
            {
                EmbedBuilder builder = new();
                builder.WithTitle("Custom commands:");
                string commands = "";

                foreach (var command in list)
                {
                    if (commands == "") { commands += "!" + command.Command; }
                    else commands += " , !" + command.Command;
                }
                builder.WithDescription(commands);
                builder.WithColor(Color.Teal);

                await ReplyAsync("", false, builder.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ChatCommands.cs CustomList", ex.ToString()));
            }
        }



        //List out commands everybody has access to
        [Command("help")]
        public async Task Help()
        {
            try
            {
                Dictionary<string, string> commands = new();

                if (!File.Exists("Assets\\Commands\\Commands.txt")) { await ReplyAsync("List of commands can't be found!"); return; }


                using (StreamReader reader = new("Assets\\Commands\\Commands.txt"))
                {
                    string curr = "";
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (line == "") { continue; }

                        if (line.StartsWith("!")) { commands[curr] += "`" + line.Split('\t')[0] + " ` " + line.Split('\t')[1] + "\n"; }

                        else if (line.StartsWith("*")) { commands[curr] += line + "\n"; }

                        else { commands.Add(line, ""); curr = line; }
                    }
                };


                EmbedBuilder builder = new();
                builder.WithTitle("List of Commands");

                foreach (var item in commands) { builder.AddField(item.Key, item.Value, false); }
                builder.WithThumbnailUrl(Global.Config.Img);
                builder.WithColor(Color.Orange);

                await ReplyAsync("", false, builder.Build());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ChatCommands.cs Help", ex.ToString()));
            }
        }

        //Adding reminding messages to database via dates
        [Command("remind at")]
        public async Task RemindAt([Remainder] string message)
        {
            try
            {
                //Take the message apart and clear trailing whitespaces
                string datestring = message.Split(">")[0].Trim();
                string remindMessage = message.Split(">")[1].Trim();

                //Length check, the message row of the database only accepts lengths of up to 150
                if (remindMessage.Length > 150)
                {
                    await ReplyAsync("Reminder message too long!(maximum **150** characters)");
                    return;
                }

                //Add last two digits of current year to beginning in case it was left off as the datetime parse doesn't always assume a year
                if (datestring.Split(".").Length == 2) datestring = datestring.Insert(0, "" + DateTime.Now.Year.ToString()[2..] + ".");

                //Try parsing date into an exact format, in which case one can write timezones
                if (DateTime.TryParseExact(datestring, "yy.MM.dd HH:mm z", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime date))
                {
                    //Convert date to local timezone
                    DateTime ConvertedDate = TimeZoneInfo.ConvertTime(date.ToUniversalTime(), TimeZoneInfo.Local);
                    
                    //Format date to sql compatible form
                    string sqlDateString = ConvertedDate.ToString("yyyy-MM-dd HH:mm");

                    //Add reminder to database
                    DBFunctions.ReminderAdd(Context.User.Id, sqlDateString, remindMessage);

                    await ReplyAsync($"Alright, I will remind you at `{ConvertedDate}`!");
                }
                else
                {
                    //Try parsing the date
                    if (DateTime.TryParse(datestring, out date))
                    {
                        //Check if date is not already in the past
                        if (DateTime.Compare(date, DateTime.Now) > 0)
                        {
                            //Format date to sql compatible form
                            string sqlDateString = date.ToString("yyyy-MM-dd HH:mm");

                            //Add reminder to database
                            DBFunctions.ReminderAdd(Context.User.Id, sqlDateString, remindMessage);

                            await ReplyAsync($"Alright, I will remind you at `{date}`!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ChatCommands.cs RemindAt", ex.ToString()));
            }
        }

        //Adding reminding messages to database via amounts of time from current date
        [Command("remind in")]
        public async Task RemindIn(int amount, string type, [Remainder] string remindMessage)
        {
            try
            {
                //Remove the > symbol in case it is there, clear trailing whitespaces
                if (remindMessage.StartsWith(">")) remindMessage = remindMessage[1..];
                remindMessage = remindMessage.Trim();

                //Length check, the message row of the database only accepts lengths of up to 150
                if (remindMessage.Length > 150)
                {
                    await ReplyAsync("Reminder message too long!(maximum **150** characters)");
                    return;
                }

                //Check what lengths of time we need to deal with and add it to the current date
                DateTime date = DateTime.Now;
                switch (type)
                {
                    case "year":
                    case "years":
                        {
                            date = date.AddYears(amount);
                            break;
                        }
                    case "month":
                    case "months":
                        {
                            date = date.AddMonths(amount);
                            break;
                        }
                    case "day":
                    case "days":
                        {
                            date = date.AddDays(amount);
                            break;
                        }
                    case "hour":
                    case "hours":
                        {
                            date = date.AddHours(amount);
                            break;
                        }
                    case "minute":
                    case "minutes":
                        {
                            date = date.AddMinutes(amount);
                            break;
                        }
                    default:
                        {
                            return;
                        }
                }

                //Format date to sql compatible form
                string sqlDateString = date.ToString("yyyy-MM-dd HH:mm");

                //Add reminder to database
                DBFunctions.ReminderAdd(Context.User.Id, sqlDateString, remindMessage);

                await ReplyAsync($"Alright, I will remind you at `{date}`!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ChatCommands.cs RemindIn", ex.ToString()));
            }

        }
    }
}
