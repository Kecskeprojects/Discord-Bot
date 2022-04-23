﻿using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Interfaces
{
    public interface IAdminCommands
    {
        [Command("help admin")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task Help();


        [Command("command add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task CommandAdd(string name, string link);


        [Command("command remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task CommandRemove(string name);


        [Command("setting set")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task SettingSet(string type, string name, string twitchurl = "");


        [Command("setting unset")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task SettingUnset(string type);


        [Command("self role add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task SelfRoleAdd(string name);


        [Command("self role remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task SelfRoleRemove(string name);


        [Command("keyword add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task KeywordAdd([Remainder] string keyword_response);


        [Command("keyword remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task KeywordRemove(string keyword);


        [Command("server settings")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task ServerSettings();


        [Command("say")]
        [RequireOwner]
        public Task Say(IMessageChannel channel, [Remainder] string text);


        [Command("dbmanagement")]
        [RequireOwner]
        public Task DBManagement([Remainder] string query);


        [Command("greeting add")]
        [RequireOwner]
        public Task GreetingAdd(string url);


        [Command("greeting remove")]
        [RequireOwner]
        public Task GreetingRemove(int id);


        [Command("ping")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task Ping();
    }
}
