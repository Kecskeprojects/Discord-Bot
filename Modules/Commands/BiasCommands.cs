using Discord;
using Discord.Commands;
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
                biasName = biasName.ToLower().Trim();

                var result = DBFunctions.BiasList();

                int biasId;
                if (result == null) biasId = 1;
                else biasId = int.Parse(result.Rows[^1][0].ToString()) + 1;

                if (DBFunctions.BiasByName(biasName) == null)
                {
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
                biasName = biasName.ToLower().Trim();

                if (DBFunctions.BiasByName(biasName) != null)
                {
                    if (DBFunctions.BiasRemove(biasName) > 0)
                    {
                        await ReplyAsync("Bias removed from list!");
                    }
                    else await ReplyAsync("Bias could not be removed!");
                }
                else await ReplyAsync("Bias not in database!");
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
                biasName = biasName.ToLower().Trim();

                var result = DBFunctions.BiasByName(biasName);

                if (result != null && DBFunctions.UserBiasCheck(Context.User.Id, biasName) == null)
                {
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
                biasName = biasName.ToLower().Trim();

                var result = DBFunctions.BiasByName(biasName);

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
                var result = DBFunctions.UserBiasesList(Context.User.Id);

                if (result == null)
                {
                    await ReplyAsync("You do not have any biases set yet!");
                }
                else
                {
                    string message = "Your biases:\n";
                    foreach (DataRow item in result.Rows)
                    {
                        if (message != "Your biases:\n") message += ", ";

                        message += $"`{char.ToUpper(item[0].ToString()[0]) + item[0].ToString()[1..]}`";
                    }

                    Random r = new();

                    if (r.Next(0, 100) < 10)
                    {
                        string bias = result.Rows[r.Next(0, result.Rows.Count)][0].ToString();
                        bias = char.ToUpper(bias[0]) + bias[1..];

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
                var result = DBFunctions.BiasList();

                if (result == null)
                {
                    await ReplyAsync("No biases have been added yet!");
                }
                else
                {
                    string message = "List of biases:\n";
                    foreach (DataRow item in result.Rows)
                    {
                        if (message != "List of biases:\n") message += ", ";

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
                biasName = biasName.ToLower().Trim();

                if (DBFunctions.BiasByName(biasName) == null)
                {
                    await ReplyAsync("No bias found with that name!");
                }
                else
                {
                    var userIds = DBFunctions.UsersWithBiasList(biasName);

                    string message = "";

                    foreach (DataRow item in userIds.Rows)
                    {
                        ulong id = ulong.Parse(item[0].ToString());

                        var user = Context.Guild.GetUser(id);

                        if (user != null) message += user.Mention + " ";
                        else continue;
                    }

                    await ReplyAsync(message);
                    await Context.Message.DeleteAsync();
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
