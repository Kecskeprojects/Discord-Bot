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

        [Command("coin flip")]
        [Alias(new string[] { "flip a coin", "cf", "fc", "cofl", "flco" })]
        public Task CoinFlip([Remainder] string message);

        [Command("remind at")]
        [Alias(new string[] { "reminder at" })]
        public Task RemindAt([Remainder] string message);

        [Command("remind in")]
        [Alias(new string[] { "reminder in" })]
        public Task RemindIn([Remainder] string message);

        [Command("remind list")]
        [Alias(new string[] { "reminder list" })]
        public Task RemindList();

        [Command("remind remove")]
        [Alias(new string[] { "reminder remove" })]
        public Task RemindRemove(int index);
    }
}
