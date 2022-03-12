using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Interfaces
{
    internal interface IChatCommands
    {
        [Command("8ball")]
        public Task Eightball([Remainder] string question);

        [Command("custom list")]
        public Task CustomList();

        [Command("help")]
        public Task Help();
    }
}
