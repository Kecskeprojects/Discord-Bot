using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Interfaces
{
    internal interface IChatCommands
    {
        [Command("8ball")]
        public Task Eightball([Remainder] string question);

        [Command("custom list")]
        [RequireContext(ContextType.Guild)]
        public Task CustomList();

        [Command("help")]
        public Task Help();

        [Command("remind at")]
        public Task RemindAt([Remainder] string message);

        [Command("remind in")]
        public Task RemindIn([Remainder] string message);

        [Command("remind list")]
        public Task RemindList();

        [Command("remind remove")]
        public Task RemindRemove(int index);
    }
}
