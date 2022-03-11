using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.IO;
using Discord_Bot.Modules.ListClasses;
using Discord_Bot.Modules.Database;

namespace Discord_Bot.Modules.Commands
{
    public class AdminCommands : ModuleBase<SocketCommandContext>, Interfaces.IAdminCommands
    {
        //
        //ADMIN COMMANDS
        //


        //Embed complete list of commands in a text file
        [Command("help admin")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task Help()
        {
            try
            {
                if (!File.Exists("Assets\\Commands\\Admin_Commands.txt")) { await ReplyAsync("Command file missing!"); return; }

                await Context.Channel.SendFileAsync(Directory.GetCurrentDirectory() + "\\Assets\\Commands\\Admin_Commands.txt");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs Help", ex.ToString()));
            }
        }



        //Custom command adding, gifs and pics mainly
        [Command("command add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task CommandAdd(string name, string link)
        {
            try
            {
                //Check if a command with such a name already exists
                var table = DBManagement.Read($"SELECT `command` FROM `customcommand` WHERE `command` = '{name}' AND `serverId` = '{Context.Guild.Id}'");

                if (table.Rows.Count == 0)
                {
                    //Check if the url is a valid url, not just a string of characters
                    if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
                    {
                        //Add command to database
                        int affected = DBManagement.Insert($"INSERT INTO `customcommand`(`serverId`, `command`, `url`) VALUES ('{Context.Guild.Id}','{name}','{link}')");

                        if (affected > 0) await ReplyAsync($"New command successfully added: {name}");
                        else await ReplyAsync("Command could not be added!");
                    }
                    else await ReplyAsync("That link is invalid!");
                }
                else await ReplyAsync("A command with this name already exists on this server!");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs CommandAdd", ex.ToString()));
            }
        }



        //Custom command removing, gifs and pics mainly
        [Command("command remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task CommandRemove(string name)
        {
            try
            {
                int affected = DBManagement.Delete($"DELETE FROM `customcommand` WHERE `command` = '{name}' AND `serverId` = '{Context.Guild.Id}'");

                if (affected > 0) await ReplyAsync($"The {name} command has been removed.");
                else await ReplyAsync("Command does not exist.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs CommandRemove", ex.ToString()));
            }
        }


        
        //Setting modification
        [Command("setting set")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task SettingSet(string type, string name)
        {
            try
            {
                IMessageChannel channel = Context.Guild.TextChannels.Where(x => x.Name.ToLower() == name).FirstOrDefault();
                IRole role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name).FirstOrDefault();

                if (role != null || channel != null)
                {
                    int affected = -1;
                    switch (type)
                    {
                        case "music":
                            {
                                affected = DBManagement.Insert($"UPDATE `serversetting` SET `musicChannel` = '{channel.Id}'");
                                if (affected > 0) Global.servers[Context.Guild.Id].MusicChannel = channel.Id;
                                break;
                            }
                        case "role":
                            {
                                affected = DBManagement.Insert($"UPDATE `serversetting` SET `roleChannel` = '{channel.Id}'");
                                if (affected > 0) Global.servers[Context.Guild.Id].RoleChannel = channel.Id;
                                break;
                            }
                        case "notif":
                            {
                                affected = DBManagement.Insert($"UPDATE `serversetting` SET `tNotifChannel` = '{channel.Id}'");
                                if (affected > 0) Global.servers[Context.Guild.Id].TNotifChannel = channel.Id;
                                break;
                            }
                        case "notifrole":
                            {
                                affected = DBManagement.Insert($"UPDATE `serversetting` SET `tNotifRole` = '{role.Id}'");
                                if (affected > 0) Global.servers[Context.Guild.Id].TNotifRole = role.Id;
                                break;
                            }
                    }

                    if (affected > 0) await ReplyAsync("Server settings updated!"); 
                    else await ReplyAsync("Server settings could not be updated!");
                }
                else await ReplyAsync("Channel/Role not found!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs SettingSet", ex.ToString()));
            }
        }



        //Setting removal
        [Command("setting unset")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task SettingUnset(string type)
        {
            try
            {
                int affected = -1;
                switch (type)
                {
                    case "music":
                        {
                            affected = DBManagement.Insert($"UPDATE `serversetting` SET `musicChannel` = '0'");
                            if (affected > 0) Global.servers[Context.Guild.Id].MusicChannel = 0;
                            break;
                        }
                    case "role":
                        {
                            affected = DBManagement.Insert($"UPDATE `serversetting` SET `roleChannel` = '0'");
                            if (affected > 0) Global.servers[Context.Guild.Id].RoleChannel = 0;
                            break;
                        }
                    case "notif":
                        {
                            affected = DBManagement.Insert($"UPDATE `serversetting` SET `tNotifChannel` = '0'");
                            if (affected > 0) Global.servers[Context.Guild.Id].TNotifChannel = 0;
                            break;
                        }
                    case "notifrole":
                        {
                            affected = DBManagement.Insert($"UPDATE `serversetting` SET `tNotifRole` = '0'");
                            if (affected > 0) Global.servers[Context.Guild.Id].TNotifRole = 0;
                            break;
                        }
                }

                if (affected > 0) await ReplyAsync("Server settings updated!");
                else await ReplyAsync("Server settings could not be updated!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs SettingUnset", ex.ToString()));
            }
        }


        
        //Adding self role to database
        [Command("self role add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task SelfRoleAdd(string name)
        {
            try
            {
                IRole role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name).FirstOrDefault();
                var table = DBManagement.Read($"SELECT `command` FROM `customcommand` WHERE `command` = '{name}' AND `serverId` = '{Context.Guild.Id}'");

                if (role != null)
                {
                    if (table.Rows.Count == 0)
                    {
                        //Add role to database
                        int affected = DBManagement.Insert($"INSERT INTO `role`(`serverId`, `roleName`, `roleId`) VALUES ('{Context.Guild.Id}','{role.Name}','{role.Id}')");

                        if (affected > 0) await ReplyAsync($"New role successfully added: {role.Name}");
                        else await ReplyAsync("Role could not be added!");
                    }
                    else await ReplyAsync("Role already in database!");
                }
                else await ReplyAsync("Role not found!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs SelfRoleAdd", ex.ToString()));
            }
        }



        //Removing self rome from database
        [Command("self role remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task SelfRoleRemove(string name)
        {
            try
            {
                //Find role with matching name
                IRole role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name).FirstOrDefault();

                if(role != null)
                {
                    //Remove role from database
                    int affected = DBManagement.Delete($"DELETE FROM `customcommand` WHERE `command` = '{role.Name}' AND `serverId` = '{Context.Guild.Id}'");

                    if (affected > 0) await ReplyAsync($"The {role.Name} role has been removed.");
                    else await ReplyAsync("Role does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs SelfRoleRemove", ex.ToString()));
            }
        }


        
        //Keyword list modification
        [Command("keyword add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task KeywordAdd([Remainder] string keyword_response)
        {
            try
            {
                Console.WriteLine(keyword_response);
                string[] array = keyword_response[1..^1].Split("` `");
                var table = DBManagement.Read($"SELECT `trigger` FROM `keyword` WHERE `trigger` = '{array[0]}' AND `serverId` = '{Context.Guild.Id}'");

                if(table.Rows.Count == 0)
                {
                    int affected = DBManagement.Insert($"INSERT INTO `keyword` (`serverId`,`trigger`, `response`) VALUES ('{Context.Guild.Id}','{array[0]}','{array[1]}');");
                    if (affected > 0) await ReplyAsync("Keyword added to database!");
                    else await ReplyAsync("Keyword could not be added to database!");
                }
                else await ReplyAsync("Keyword already in database!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs KeywordAdd", ex.ToString()));
            }
        }



        //Keyword removal
        [Command("keyword remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task KeywordRemove(string keyword)
        {
            try
            {
                int affected = DBManagement.Insert($"DELETE FROM `keyword` WHERE `serverId` = '{Context.Guild.Id}' AND `trigger` = '{keyword}';");
                if (affected > 0) await ReplyAsync("Keyword removed from database!");
                else await ReplyAsync("Keyword could not be removed from database!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs KeywordRemove", ex.ToString()));
            }
        }



        //Lists server settings
        [Command("server settings")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task ServerSettings()
        {
            try
            {
            ServerSetting server = Global.servers[Context.Guild.Id];
            ulong[] ids = { server.MusicChannel, server.RoleChannel, server.TNotifChannel, server.TNotifRole };
            string[] ch_names = { "none", "none", "none", "none" };

            for (int i = 0; i < 3; i++)
            {
                var item = Context.Guild.TextChannels.Where(n => n.Id == ids[i]).FirstOrDefault();
                if (item != null) ch_names[i] = item.Name;
            }

            if(ids[3] != 0) ch_names[3] = (Context.Channel as IGuildChannel).Guild.GetRole(ids[3]).Name;


            EmbedBuilder embed = new();

            embed.WithTitle("The server's settings are the following:");
            embed.AddField("Music channel:", $"`{ ch_names[0]}`");
            embed.AddField("Role channel:", $"`{ ch_names[1]}`");
            embed.AddField("Notification channel:", $"`{ ch_names[2]}`");
            embed.AddField("Notification role:", $"`{ ch_names[3]}`");

            embed.WithThumbnailUrl(Program.Config.Img);
            embed.WithTimestamp(DateTime.Now);
            embed.WithColor(Color.Teal);

            await ReplyAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Admin_Commands.cs ServerSettings", ex.ToString()));
            }
        }



        //Command for owner, the bot says in whatever channel you gave it what you told it to say
        [Command("say")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task Say(IMessageChannel channel, [Remainder] string text)
        {
            if (Context.Guild.TextChannels.Contains(channel))
            {
                await Context.Message.DeleteAsync();

                await channel.SendMessageAsync(text);
            }
        }



        //Test to see if bot is responsive
        [Command("ping")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task Ping() { await ReplyAsync("pong"); }
    }
}
