using Discord;
using Discord.Commands;
using Discord.Rest;
using System;
using System.Threading.Tasks;
using Discord_Bot.Modules;
using Discord_Bot.Modules.ListClasses;
using Discord_Bot.Modules.Database;

namespace Discord_Bot
{
    public class ProgramFunctions
    {
        //Check the list of custom commands on the server
        public static async Task<bool> Custom_Commands(SocketCommandContext context)
        {
            try
            {
                var row = DBFunctions.CustomCommandGet(context.Guild.Id, context.Message.Content[1..].ToLower());

                if (DBFunctions.CustomCommandGet(context.Guild.Id, context.Message.Content[1..].ToLower()) != null) 
                {
                    await context.Message.Channel.SendMessageAsync(row[2].ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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

                var row = DBFunctions.SelfRoleGet(context.Guild.Id, context.Message.Content[1..].ToLower());

                if (row != null)
                {
                    string name = row[1].ToString();
                    ulong id = ulong.Parse(row[2].ToString());

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
                Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Self_role", ex.ToString()));
            }
        }



        //Check for messages starting with I think and certain Keywords
        public static async Task Feature_Check(SocketCommandContext context)
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

                var row = DBFunctions.KeywordGet(context.Guild.Id, context.Message.Content);
                if (row != null)
                {
                    await context.Channel.SendMessageAsync(row[2].ToString());
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Feature_Check", ex.ToString()));
            }
        }
    }
}
