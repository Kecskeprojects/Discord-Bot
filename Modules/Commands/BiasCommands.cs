using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Modules.Database;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Discord_Bot.Modules.Commands
{
    public class BiasCommands : ModuleBase<SocketCommandContext>, Interfaces.IBiasCommands
    {
        [Command("biaslist add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public async Task AddBiasList([Remainder] string biasData)
        {
            try
            {
                //Make the name lowercase and clear and accidental spaces
                biasData = biasData.ToLower().Trim();

                string biasName = biasData.Split('-')[0];
                string biasGroup = biasData.Split('-')[1];

                //Check if there is such an idol already in the database
                if (DBFunctions.BiasByName(biasName) == null)
                {
                    //Get all the biases so that we can get the highest id so far, SQLite can't auto increment as far as I have seen things
                    var result = DBFunctions.BiasList();
                    int biasId;
                    if (result == null) biasId = 1;
                    else biasId = int.Parse(result.Rows[^1][0].ToString()) + 1;

                    //Try adding them to the database
                    if (DBFunctions.BiasAdd(biasId, biasName, biasGroup) > 0)
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
        public async Task MyBiases([Remainder] string groupName = "")
        {
            try
            {
                groupName = groupName.ToLower();

                //Get your list of biases
                var result = DBFunctions.UserBiasesList(Context.User.Id, groupName);

                //Check if you have any
                if (result == null)
                {
                    if (groupName != "") await ReplyAsync("No biases from that group are in your list!");
                    else await ReplyAsync("You do not have any biases set yet!");
                }
                else
                {
                    Dictionary<string, string> groups = new();
                    foreach (DataRow item in result.Rows)
                    {
                        if (item[1].ToString() != "")
                        {
                            //Check if key exists for group, if not, make it
                            if (!groups.ContainsKey(item[1].ToString())) groups.Add(item[1].ToString(), $"{item[1].ToString().ToUpper()}:\n");

                            //If dictionary item has been modified from default, add comma
                            if (groups[item[1].ToString()] != $"{item[1].ToString().ToUpper()}:\n") groups[item[1].ToString()] += ", ";

                            //We make the first character of each name capital
                            groups[item[1].ToString()] += $"`{item[0].ToString().ToUpper()}`";
                        }
                        else
                        {
                            if (!groups.ContainsKey("unsorted")) groups.Add("unsorted", $"UNSORTED:\n");

                            //If dictionary item has been modified from default, add comma
                            if (groups["unsorted"] != $"UNSORTED:\n") groups["unsorted"] += ", ";

                            //We make the first character of each name capital
                            groups["unsorted"] += $"`{item[0].ToString().ToUpper()}`";
                        }
                    }

                    //Make a list out of them
                    string message = "Your biases:\n\n";
                    foreach (var item in groups)
                    {
                        if (item.Key != "soloist" && item.Key != "unsorted")
                        {
                            message += $"{item.Value}\n";
                        }
                    }

                    //Add soloists and unsorted ones to the end
                    if (groups.ContainsKey("soloist"))
                    {
                        message += $"{groups["soloist"]}\n";
                    }
                    if (groups.ContainsKey("unsorted"))
                    {
                        message += $"{groups["unsorted"]}";
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
        public async Task BiasList([Remainder] string groupName = "")
        {

            try
            {
                groupName = groupName.ToLower();

                //Get the global list of biases
                var result = DBFunctions.BiasList(groupName);

                //Check if we have any
                if (result == null)
                {
                    if (groupName != "") await ReplyAsync("No biases from that group are in the database!");
                    else await ReplyAsync("No biases have been added yet!");
                }
                else
                {
                    Dictionary<string, string> groups = new();
                    foreach (DataRow item in result.Rows)
                    {
                        if (item[2].ToString() != "")
                        {
                            //Check if key exists for group, if not, make it
                            if (!groups.ContainsKey(item[2].ToString())) groups.Add(item[2].ToString(), $"{item[2].ToString().ToUpper()}:\n");

                            //If dictionary item has been modified from default, add comma
                            if (groups[item[2].ToString()] != $"{item[2].ToString().ToUpper()}:\n") groups[item[2].ToString()] += ", ";

                            //We make the first character of each name capital
                            groups[item[2].ToString()] += $"`{item[1].ToString().ToUpper()}`";
                        }
                        else
                        {
                            if(!groups.ContainsKey("unsorted")) groups.Add("unsorted", $"UNSORTED:\n");

                            //If dictionary item has been modified from default, add comma
                            if (groups["unsorted"] != $"UNSORTED:\n") groups["unsorted"] += ", ";

                            //We make the first character of each name capital
                            groups["unsorted"] += $"`{item[1].ToString().ToUpper()}`";
                        }
                    }
                    
                    //Make a list out of them
                    string message = "List of biases:\n\n";
                    foreach (var item in groups)
                    {
                        if(item.Key != "soloist" && item.Key != "unsorted")
                        {
                            message += $"{item.Value}\n";
                        }
                    }

                    //Add soloists and unsorted ones to the end
                    if (groups.ContainsKey("soloist"))
                    {
                        message += $"{groups["soloist"]}\n";
                    }
                    if (groups.ContainsKey("unsorted"))
                    {
                        message += $"{groups["unsorted"]}";
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
        public async Task PingBias([Remainder] string biasNames)
        {
            try
            {
                //Make the name lowercase and split up names
                biasNames = biasNames.ToLower();
                string[] nameList = biasNames.Split(',');

                //Clear trailing white spaces
                for (int i = 0; i < nameList.Length; i++) nameList[i] = nameList[i].Trim();

                //Check if that idol is in the database
                if (DBFunctions.BiasByNameForEach(nameList) == null)
                {
                    await ReplyAsync("No bias found with that name/those names!");
                }
                else
                {
                    //Get the user ids that have this set as their bias
                    var userIds = DBFunctions.UsersWithBiasList(nameList);

                    await Context.Guild.DownloadUsersAsync();
                    
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
