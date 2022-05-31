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
    }
}
