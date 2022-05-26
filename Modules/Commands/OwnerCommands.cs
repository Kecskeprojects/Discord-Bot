using Discord;
using Discord.Commands;
using Discord_Bot.Modules.Database;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Commands
{
    internal class OwnerCommands : ModuleBase<SocketCommandContext>, Interfaces.IOwnerCommands
    {
        //
        //OWNER COMMANDS
        //


        //Embed complete list of commands in a text file
        [Command("help owner")]
        [RequireOwner]
        public async Task Help()
        {
            try
            {
                if (!File.Exists("\\Assets\\Commands\\List of Commands.txt")) { await ReplyAsync("Command file missing!"); return; }

                await Context.Channel.SendFileAsync(Directory.GetCurrentDirectory() + "\\Assets\\Commands\\List of Commands.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "AdminCommands.cs Help", ex.ToString()));
            }
        }



        //Command for owner to add global greeting gifs
        [Command("greeting add")]
        [RequireOwner]
        public async Task GreetingAdd(string url)
        {
            try
            {
                var result = DBFunctions.AllGreeting();

                int id;
                if (result == null) id = 1;
                else id = int.Parse(result.Rows[^1][0].ToString()) + 1;

                if (DBFunctions.GreetingAdd(id, url) > 0)
                {
                    await ReplyAsync("Greeting added!");
                }
                else await ReplyAsync("Greeting could not be added!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "AdminCommands.cs GreetingAdd", ex.ToString()));
            }
        }



        //Command for owner to remove global greeting gifs
        [Command("greeting remove")]
        [RequireOwner]
        public async Task GreetingRemove(int id)
        {
            try
            {
                if (DBFunctions.GreetingRemove(id) > 0)
                {
                    await ReplyAsync("Greeting removed!");
                }
                else await ReplyAsync("Greeting could not be removed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "AdminCommands.cs GreetingRemove", ex.ToString()));
            }
        }



        //Command for owner, the bot says in whatever channel you gave it what you told it to say
        [Command("say")]
        [RequireOwner]
        [RequireContext(ContextType.Guild)]
        public async Task Say(IMessageChannel channel, [Remainder] string text)
        {
            if (Context.Guild.TextChannels.Contains(channel))
            {
                await Context.Message.DeleteAsync();

                await channel.SendMessageAsync(text);
            }
        }



        //Manual SQL queries, sends file to text channel if it is a SELECT command
        [Command("dbmanagement")]
        [RequireOwner]
        public async Task DBManagement([Remainder] string query)
        {
            Tuple<int, DataTable, string> result = DBFunctions.ManualDBManagement(query);

            int affected_rows = result.Item1;
            DataTable table = result.Item2;
            string Error = result.Item3;

            if (Error != "")
            {
                await ReplyAsync(result.Item3);
            }
            else if (table.Rows.Count < 1)
            {
                await ReplyAsync("Number of affected rows: " + affected_rows);
            }
            else
            {
                int maxwidth = table.Columns.Count;
                string text = "";

                foreach (DataColumn item in table.Columns)
                {
                    if (text != "") text += ";";
                    text += item.ColumnName;
                }
                text += "\n";

                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < maxwidth; i++)
                    {
                        if (!text.EndsWith("\n")) text += ";";
                        text += row[i].ToString();
                    }
                    text += "\n";
                }

                try
                {
                    StreamWriter writer = new($"Assets\\{table.TableName}.txt", false, Encoding.UTF8);
                    writer.WriteLine(text);
                    writer.Close();
                    await Context.Channel.SendFileAsync($"Assets\\{table.TableName}.txt");
                    File.Delete($"Assets\\{table.TableName}.txt");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Global.Logs.Add(new Log("DEV", ex.Message));
                    Global.Logs.Add(new Log("ERROR", "AdminCommands.cs DBManagement", ex.ToString()));
                }
            }
        }
    }
}
