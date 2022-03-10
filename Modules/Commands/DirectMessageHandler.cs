using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Commands
{
    class DirectMessageHandler
    {
        public static async Task Handle(SocketCommandContext context)
        {
            if (context.Message.Content != "help") await context.Channel.SendMessageAsync("I'm still working on this!");
            else await context.Channel.SendMessageAsync("You found a key word, but I am still working on this");
        }
    }
}
