using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Interfaces
{
    public interface IOwnerCommands
    {
        [Command("help owner")]
        [RequireOwner]
        public Task Help();


        [Command("say")]
        [RequireOwner]
        [RequireContext(ContextType.Guild)]
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

    }
}
