using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Interfaces
{
    internal interface IVoiceCommands
    {
        [Command("p")]
        public Task Play([Remainder] string content);


        [Command("join")]
        public Task Join();


        [Command("leave")]
        public Task Leave();


        [Command("queue")]
        public Task Queue();


        [Command("np")]
        public Task Now_Playing();


        [Command("clear")]
        public Task Clear();


        [Command("skip")]
        public Task Skip();


        [Command("remove")]
        public Task Remove(int position);
    }
}
