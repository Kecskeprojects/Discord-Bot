using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Interfaces
{
    internal interface IBiasCommands
    {
        [Command("biaslist add")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task AddBiasList([Remainder] string biasName);

        [Command("biaslist remove")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public Task RemoveBiasList([Remainder] string biasName);

        [Command("bias add")]
        public Task AddBias([Remainder] string biasName);

        [Command("bias remove")]
        public Task RemoveBias([Remainder] string biasName);

        [Command("bias clear")]
        public Task ClearBias();

        [Command("my biases")]
        public Task MyBiases();

        [Command("bias list")]
        public Task BiasList();

        [Command("ping")]
        [RequireContext(ContextType.Guild)]
        public Task PingBias([Remainder] string bias);
    }
}
