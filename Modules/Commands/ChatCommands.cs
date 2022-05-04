using System;
using System.Collections.Generic;
using System.Data;
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
            try
            {
                EmbedBuilder builder = new();
                builder.WithTitle("Custom commands:");
                string commands = "";

                foreach (DataRow item in DBFunctions.AllCustomCommand(Context.Guild.Id).Rows)
                {
                    if (commands == "") { commands += "!" + item[1].ToString(); }
                    else commands += " , !" + item[1].ToString();
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
    }
}
