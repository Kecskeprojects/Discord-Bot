using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Modules.Database;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Commands
{
    public class BiasCommands : ModuleBase<SocketCommandContext>, Interfaces.IBiasCommands
    {
        [Command("biaslist add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public async Task AddBiasList([Remainder] string biasName)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                biasName = biasName.ToLower().Trim();

                //Check if there is such an idol already in the database
                if (DBFunctions.BiasByName(biasName) == null)
                {
                    //Get all the biases so that we can get the highest id so far, SQLite can't auto increment as far as I have seen things
                    var result = DBFunctions.BiasList();
                    int biasId;
                    if (result == null) biasId = 1;
                    else biasId = int.Parse(result.Rows[^1][0].ToString()) + 1;

                    //Try adding them to the database
                    if (DBFunctions.BiasAdd(biasId, biasName) > 0)
                    {
                        await ReplyAsync("Bias added to list!");
                    }
                    else await ReplyAsync("Bias could not be added!");
                }
                else await ReplyAsync("Bias already in database!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "DirectCommands.cs AddBiasList", ex.ToString()));
            }

        }

        [Command("biaslist remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public async Task RemoveBiasList([Remainder] string biasName)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                biasName = biasName.ToLower().Trim();

                //Try removing them from the database
                if (DBFunctions.BiasRemove(biasName) > 0)
                {
                    await ReplyAsync("Bias removed from list!");
                }
                else await ReplyAsync("Bias could not be removed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "DirectCommands.cs RemoveBiasList", ex.ToString()));
            }
        }

        [Command("bias add")]
        public async Task AddBias([Remainder] string biasName)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                biasName = biasName.ToLower().Trim();

                //Get id of given idol
                var result = DBFunctions.BiasByName(biasName);

                //Check if they are in the database, then check if the user already has them assigned
                if (result != null && DBFunctions.UserBiasCheck(Context.User.Id, biasName) == null)
                {
                    //Try adding it to the users idol list
                    if (DBFunctions.UserBiasAdd(Context.User.Id, int.Parse(result[0].ToString())) > 0)
                    {
                        await ReplyAsync("Bias added to your list of biases!");
                    }
                    else await ReplyAsync("Bias could not be added!");
                }
                else await ReplyAsync("Bias not in database or you already have it on your list!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "DirectCommands.cs AddBias", ex.ToString()));
            }
        }

        [Command("bias remove")]
        public async Task RemoveBias([Remainder] string biasName)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                biasName = biasName.ToLower().Trim();

                //Get id of given idol
                var result = DBFunctions.BiasByName(biasName);

                //Try removing the idol from their list
                if (DBFunctions.UserBiasRemove(int.Parse(result[0].ToString()), Context.User.Id) > 0)
                {
                    await ReplyAsync("Bias removed from your list of biases!");
                }
                else await ReplyAsync("Bias could not be removed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "DirectCommands.cs RemoveBias", ex.ToString()));
            }
        }

        [Command("bias clear")]
        public async Task ClearBias()
        {
            try
            {
                //Try clearing all the biases the user had
                if (DBFunctions.UserBiasClear(Context.User.Id) > 0)
                {
                    await ReplyAsync("Your biases have been cleared!");
                }
                else await ReplyAsync("You did not have any biases to clear!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "DirectCommands.cs ClearBias", ex.ToString()));
            }
        }


        [Command("my biases")]
        public async Task MyBiases()
        {
            try
            {
                //Get your list of biases
                var result = DBFunctions.UserBiasesList(Context.User.Id);

                //Check if you have any
                if (result == null)
                {
                    await ReplyAsync("You do not have any biases set yet!");
                }
                else
                {
                    //Make a list out of them
                    string message = "Your biases:\n";
                    foreach (DataRow item in result.Rows)
                    {
                        if (message != "Your biases:\n") message += ", ";

                        //We make the first character of each name capital
                        message += $"`{char.ToUpper(item[0].ToString()[0]) + item[0].ToString()[1..]}`";
                    }

                    //Generate a random number, 10% chance for an additional message to appear
                    Random r = new();
                    if (r.Next(0, 100) < 10)
                    {
                        //Pick a random bias
                        string bias = result.Rows[r.Next(0, result.Rows.Count)][0].ToString();
                        //Also make the first letter upper case
                        bias = char.ToUpper(bias[0]) + bias[1..];

                        //Choose 1 out of 4 responses
                        switch (r.Next(0, 4))
                        {
                            case 0: { message += $"\n\nBetween you and me, I quite like {bias} too."; break; }
                            case 1: { message += $"\n\n{bias}? Good choice!"; break; }
                            case 2: { message += $"\n\nHmm, this list is quite short, someone with a life over here..."; break; }
                            case 3: { message += $"\n\nOh, {bias} is honestly just great."; break; }
                        }
                    }
                    await ReplyAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "DirectCommands.cs MyBiases", ex.ToString()));
            }
        }

        [Command("bias list")]
        public async Task BiasList()
        {

            try
            {
                //Get the global list of biases
                var result = DBFunctions.BiasList();

                //Check if we have any
                if (result == null)
                {
                    await ReplyAsync("No biases have been added yet!");
                }
                else
                {
                    //Make a list out of them
                    string message = "List of biases:\n";
                    foreach (DataRow item in result.Rows)
                    {
                        if (message != "List of biases:\n") message += ", ";

                        //We make the first character of each name capital
                        message += $"`{char.ToUpper(item[1].ToString()[0]) + item[1].ToString()[1..]}`";
                    }

                    await ReplyAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "DirectCommands.cs BiasList", ex.ToString()));
            }
        }

        [Command("ping")]
        [RequireContext(ContextType.Guild)]
        public async Task PingBias([Remainder] string biasName)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                biasName = biasName.ToLower().Trim();

                //Check if that idol is in the database
                if (DBFunctions.BiasByName(biasName) == null)
                {
                    await ReplyAsync("No bias found with that name!");
                }
                else
                {
                    //Get the user ids that have this set as their bias
                    var userIds = DBFunctions.UsersWithBiasList(biasName);

                    string message = "";

                    //Make a list of mentions out of them
                    foreach (DataRow item in userIds.Rows)
                    {
                        ulong id = ulong.Parse(item[0].ToString());

                        //Find user on server
                        var user = Context.Guild.GetUser(id);

                        //If user is not found or the user is the one sending the command, do not add their mention to the list
                        if (user != null && user != (Context.User as SocketGuildUser)) message += user.Mention + " ";
                        else continue;
                    }

                    //If only one person has the bias and it's the command sender, send unique message, otherwise delete command and send mentions
                    if(message == "")
                    {
                        await ReplyAsync("Only you have that bias for now! Time to convert people.");
                    }
                    else
                    {
                        await Context.Message.DeleteAsync();
                        await ReplyAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "DirectCommands.cs PingBias", ex.ToString()));
            }
        }
    }
}
