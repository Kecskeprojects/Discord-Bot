using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Modules.Database;
using Discord_Bot.Modules.ListClasses;
using System;
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
                if (DBFunctions.BiasSearch(biasName) == null)
                {
                    //Get all the biases so that we can get the highest id so far, SQLite can't auto increment as far as I have seen things
                    var list = DBFunctions.BiasList();
                    int biasId;
                    if (list.Count == 0) biasId = 1;
                    else biasId = list[^1].Id + 1;

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
                var bias = DBFunctions.BiasSearch(biasName);

                //Check if they are in the database, then check if the user already has them assigned
                if (bias != null && DBFunctions.UserBiasCheck(Context.User.Id, biasName) == null)
                {
                    //Try adding it to the users idol list
                    if (DBFunctions.UserBiasAdd(Context.User.Id, bias.Id) > 0)
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
                var bias = DBFunctions.BiasSearch(biasName);

                //Try removing the idol from their list
                if (DBFunctions.UserBiasRemove(bias.Id, Context.User.Id) > 0)
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
                var list = DBFunctions.UserBiasesList(Context.User.Id, groupName);

                //Check if you have any
                if (list == null)
                {
                    if (groupName != "") await ReplyAsync("No biases from that group are in your list!");
                    else await ReplyAsync("You do not have any biases set yet!");
                }
                else
                {
                    SortedDictionary<string, List<string>> groups = new();
                    foreach (var bias in list)
                    {
                        if (bias.BiasGroup != "")
                        {
                            //Check if key exists for group, if not, make it
                            if (!groups.ContainsKey(bias.BiasGroup)) groups.Add(bias.BiasGroup, new List<string>());

                            //We make the name uppercase when adding
                            groups[bias.BiasGroup].Add(bias.BiasName.ToUpper());
                        }
                        else
                        {
                            //Check if key exists for group, if not, make it
                            if (!groups.ContainsKey("unsorted")) groups.Add("unsorted", new List<string>());

                            //We make the name uppercase when adding
                            groups["unsorted"].Add(bias.BiasName.ToUpper());
                        }
                    }

                    //Make a list out of all the groups and their members
                    string message = "List of biases:\n\n";
                    foreach (var group in groups)
                    {
                        //Add Group name
                        message += $"{group.Key.ToUpper()}:\n";

                        //Add individual members
                        foreach (var member in group.Value)
                        {
                            if (member != group.Value[0]) message += ", ";

                            message += $"`{member}`";
                        }
                        message += "\n";
                    }

                    //Generate a random number, 10% chance for an additional message to appear
                    Random r = new();
                    if (r.Next(0, 100) < 10)
                    {
                        //Pick a random bias
                        string bias = list[r.Next(0, list.Count)].BiasName;
                        //Also make the first letter upper case
                        bias = bias.ToUpper();

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
                //Get the global list of biases
                var list = DBFunctions.BiasList(groupName.ToLower());

                //Check if we have any items on the list
                if (list.Count == 0)
                {
                    if (groupName != "") await ReplyAsync("No biases from that group are in the database!");
                    else await ReplyAsync("No biases have been added yet!");
                }
                else
                {
                    SortedDictionary<string, List<string>> groups = new();
                    foreach (var bias in list)
                    {
                        if (bias.BiasGroup != "")
                        {
                            //Check if key exists for group, if not, make it
                            if (!groups.ContainsKey(bias.BiasGroup)) groups.Add(bias.BiasGroup, new List<string>());

                            //We make the name uppercase when adding
                            groups[bias.BiasGroup].Add(bias.BiasName.ToUpper());
                        }
                        else
                        {
                            //Check if key exists for group, if not, make it
                            if (!groups.ContainsKey("unsorted")) groups.Add("unsorted", new List<string>());

                            //We make the name uppercase when adding
                            groups["unsorted"].Add(bias.BiasName.ToUpper());
                        }
                    }

                    //Make a list out of all the groups and their members
                    string message = "List of biases:\n\n";
                    foreach (var group in groups)
                    {
                        //Add Group name
                        message += $"{group.Key.ToUpper()}:\n";

                        //Add individual members
                        foreach (var member in group.Value)
                        {
                            if (member != group.Value[0]) message += ", ";

                            message += $"`{member}`";
                        }
                        message += "\n";
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
                    var users = DBFunctions.UsersWithBiasList(nameList);

                    await Context.Guild.DownloadUsersAsync();

                    string message = "";

                    //Make a list of mentions out of them
                    foreach (var userbias in users)
                    {
                        ulong id = userbias.UserId;

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
