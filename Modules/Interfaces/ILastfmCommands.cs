using Discord.Commands;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Interfaces
{
    internal interface ILastfmCommands
    {
        [Command("lf conn")]
        [Alias(new string[] { "lf c", "lf connect" })]
        public Task LfConnect(string name);


        [Command("lf del")]
        [Alias(new string[] { "lf d", "lf delete", "lf disc", "lf disconnect" })]
        public Task LfDisconnect();


        [Command("lf tt")]
        [Alias(new string[] { "lf top tracks", "lf top track", "lf toptracks", "lf toptrack" })]
        public Task LfTopTrack(params string[] parameters);


        [Command("lf tal")]
        [Alias(new string[] { "lf top albums", "lf top album", "lf topalbums", "lf topalbum" })]
        public Task LfTopAlbum(params string[] parameters);


        [Command("lf tar")]
        [Alias(new string[] { "lf top artists", "lf top artist", "lf topartist", "lf topartists" })]
        public Task LfTopArtist(params string[] parameters);


        [Command("lf np")]
        [Alias(new string[] { "lf nowplaying", "lf now playing" })]
        public Task LfNowPlaying();


        [Command("lf rc")]
        [Alias(new string[] { "lf recent", "lf recents" })]
        public Task LfRecent(int limit = 10);


        [Command("lf artist")]
        [Alias(new string[] { "lf a" })]
        public Task LfArtist([Remainder] string artist);

        [Command("lf wk")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "lf whoknows", "lf whoknow" })]
        public Task LfWhoKnows([Remainder] string input = "");
    }
}
