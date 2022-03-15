using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Interfaces
{
    internal interface ILastfmCommands
    {
        [Command("lf conn")]
        public Task LfConnect(string name);


        [Command("lf del")]
        public Task LfDisconnect();


        [Command("lf tt")]
        public Task LfTopTrack(params string[] parameters);


        [Command("lf tal")]
        public Task LfTopAlbum(params string[] parameters);


        [Command("lf tar")]
        public Task LfTopArtist(params string[] parameters);


        [Command("lf np")]
        public Task LfNowPlaying();


        [Command("lf rc")]
        public Task LfRecent(int limit = 10);


        [Command("lf artist")]
        public Task LfArtist([Remainder] string artist);
    }
}
