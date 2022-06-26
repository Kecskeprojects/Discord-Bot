using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        [Alias(new string[] { "reminder at" })]
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

                    //Check if date is not already in the past
                    if (DateTime.Compare(ConvertedDate, DateTime.Now) > 0)
                    {
                        //Format date to sql compatible form
                        string sqlDateString = ConvertedDate.ToString("yyyy-MM-dd HH:mm");

                        //Add reminder to database
                        DBFunctions.ReminderAdd(Context.User.Id, sqlDateString, remindMessage);

                        await ReplyAsync($"Alright, I will remind you at `{ConvertedDate}`!");

                        return;
                    }
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

                            return;
                        }
                    }
                }

                await ReplyAsync("Invalit input format, the order is the following:\n`[year].[month].[day] [hour]:[minute] +-[timezone]`\nYear, hour, minute are optional unless using timezones!");
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
        [Alias(new string[] { "reminder in" })]
        public async Task RemindIn([Remainder] string message)
        {
            try
            {
                //Take the message apart and clear trailing whitespaces
                string amountstring = message.Split(">")[0].Trim();
                string remindMessage = message.Split(">")[1].Trim();

                //Split amounts into a string list, accounting for accidental spaces
                List<string> amounts = amountstring.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                //Go through every element in the list and check
                //if the user missed a space between the number and the corresponding type
                for(int i = 0; i < amounts.Count; i++)
                {
                    //Don't check if the element is a number in itself
                    if (!int.TryParse(amounts[i], out _))
                    {
                        char[] chars = amounts[i].ToCharArray();
                        int lastValid = -1;

                        //Go through the array of chars one by one
                        //until a non number is found, in which case we exit the loop
                        for (int j = 0; j < chars.Length; j++)
                        {
                            if (char.IsDigit(chars[j]))
                            {
                                lastValid = j;
                            }
                            else
                            {
                                break;
                            }
                        }

                        //If any numbers were found in the array,
                        //we rewrite original string and put the number part of it into the list
                        if (lastValid >= 0)
                        {
                            amounts[i] = amounts[i][(lastValid + 1)..];
                            amounts.Insert(i, new string(chars, 0, lastValid + 1));
                        }
                    }
                }

                //Length check, the message row of the database only accepts lengths of up to 150
                if (remindMessage.Length > 150)
                {
                    await ReplyAsync("Reminder message too long!(maximum **150** characters)");
                    return;
                }

                //Check what lengths of time we need to deal with and add it to the current date
                DateTime date = DateTime.Now;
                
                //Check if amounts has an even amount of elements, meaning every number has it's type pair and vice versa
                if(amounts.Count % 2 == 0)
                {
                    //Go through the list 2 elements at a time
                    for (int i = 0; i < amounts.Count; i += 2)
                    {
                        //The first element is the number, the second is the type, meaning day, month, year...
                        int amount = int.Parse(amounts[i]);
                        string type = amounts[i + 1];

                        //Add the appropriate amount of time
                        switch (type)
                        {
                            case "year":
                            case "years":
                            case "yr":
                            case "y":
                                {
                                    date = date.AddYears(amount);
                                    break;
                                }
                            case "month":
                            case "months":
                            case "mon":
                            case "M":
                                {
                                    date = date.AddMonths(amount);
                                    break;
                                }
                            case "week":
                            case "weeks":
                            case "w":
                                {
                                    date = date.AddDays(amount * 7);
                                    break;
                                }
                            case "day":
                            case "days":
                            case "d":
                                {
                                    date = date.AddDays(amount);
                                    break;
                                }
                            case "hour":
                            case "hours":
                            case "hr":
                            case "h":
                                {
                                    date = date.AddHours(amount);
                                    break;
                                }
                            case "minute":
                            case "minutes":
                            case "min":
                            case "m":
                                {
                                    date = date.AddMinutes(amount);
                                    break;
                                }
                            default:
                                {
                                    return;
                                }
                        }
                    }

                    //Format date to sql compatible form
                    string sqlDateString = date.ToString("yyyy-MM-dd HH:mm");

                    //Add reminder to database
                    DBFunctions.ReminderAdd(Context.User.Id, sqlDateString, remindMessage);

                    await ReplyAsync($"Alright, I will remind you at `{date}`!");
                }
                else
                {
                    await ReplyAsync("Incorrect number of inputs!");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ChatCommands.cs RemindIn", ex.ToString()));
            }

        }



        //Remove a reminder from their list of reminders
        [Command("remind list")]
        [Alias(new string[] { "reminder list" })]
        public async Task RemindList()
        {
            try
            {
                var list = DBFunctions.UserReminders(Context.User.Id);

                if(list.Count > 0)
                {
                    EmbedBuilder builder = new();
                    builder.WithTitle("Your reminders:");

                    int i = 1;
                    foreach (var reminder in list)
                    {
                        builder.AddField($"{i}.  {reminder.Date}", reminder.Message);
                        i++;
                    }

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ChatCommands.cs RemindList", ex.ToString()));
            }
        }



        //Remove a reminder from the user's list of reminders
        [Command("remind remove")]
        [Alias(new string[] { "reminder remove" })]
        public async Task RemindRemove(int index)
        {
            try
            {
                var list = DBFunctions.UserReminders(Context.User.Id);

                if (list.Count > 0 && list.Count >= index)
                {
                    var reminder = list[index - 1];

                    if (DBFunctions.ReminderRemove(Context.User.Id, reminder.Date.ToString("yyyy-MM-dd HH:mm")) > 0)
                    {
                        await ReplyAsync($"Reminder with the date {reminder.Date} has been removed!");
                    }
                    else await ReplyAsync("Reminder could not be removed!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "ChatCommands.cs RemindRemove", ex.ToString()));
            }
        }
    }
}
