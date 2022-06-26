using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Interfaces
{
    internal interface IBiasCommands
    {
        [Command("biaslist add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public Task AddBiasList([Remainder] string biasName);

        [Command("biaslist remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        public Task RemoveBiasList([Remainder] string biasName);

        [Command("bias add")]
        public Task AddBias([Remainder] string biasName);

        [Command("bias remove")]
        public Task RemoveBias([Remainder] string biasName);

        [Command("bias clear")]
        public Task ClearBias();

        [Command("my biases")]
        [Alias(new string[] { "mybiases", "biases", "my bias" })]
        public Task MyBiases([Remainder] string groupName = "");

        [Command("bias list")]
        public Task BiasList([Remainder] string groupName = "");

        [Command("ping")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "p" })]
        public Task PingBias([Remainder] string bias);
    }
}
