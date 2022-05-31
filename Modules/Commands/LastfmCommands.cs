using Discord.Commands;
using Discord_Bot.Modules.API.Lastfm;
using Discord_Bot.Modules.Database;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Commands
{
    public class LastfmCommands : ModuleBase<SocketCommandContext>, Interfaces.ILastfmCommands
    {
        [Command("lf conn")]
        public async Task LfConnect(string name)
        {
            try
            {
                if (DBFunctions.LastfmUserGet(Context.User.Id) != null)
                {
                    await ReplyAsync("you have a lastfm account connected to your discord account already!");
                    return;
                }
                else
                {
                    if (DBFunctions.LastfmUserAdd(Context.User.Id, name) > 0) await ReplyAsync("Username " + name + " connected to account!");
                    else await ReplyAsync("Lastfm account could not be connected to account!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmCommands.cs LfConnect", ex.ToString()));
            }
        }



        [Command("lf del")]
        public async Task LfDisconnect()
        {
            try
            {
                if (DBFunctions.LastfmUserRemove(Context.User.Id) > 0)
                {
                    await ReplyAsync("Last.fm has been disconnected from your account!");
                }
                else await ReplyAsync("Last.fm could not be disconnected from your account!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmCommands.cs LfDisconnect", ex.ToString()));
            }
        }



        [Command("lf tt")]
        public async Task LfTopTrack(params string[] parameters)
        {
            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    if (parameters.Length == 0) await LastfmAPI.TopTracks(Context, user.Username, 10, "overall");
                    else
                    {
                        try { parameters = LastfmFunctions.TopLastfmCheck(parameters); }
                        catch (Exception ex) { await ReplyAsync(ex.Message); return; }

                        int limit = int.Parse(parameters[0]);
                        string period = parameters[1];

                        await LastfmAPI.TopTracks(Context, user.Username, limit, period);
                    }
                }
                else await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn command to do so!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmCommands.cs LfTopTrack", ex.ToString()));
            }
        }



        [Command("lf tal")]
        public async Task LfTopAlbum(params string[] parameters)
        {
            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    if (parameters.Length == 0) await LastfmAPI.TopAlbums(Context, user.Username, 10, "overall");
                    else
                    {
                        try { parameters = LastfmFunctions.TopLastfmCheck(parameters); }
                        catch (Exception ex) { await ReplyAsync(ex.Message); return; }

                        int limit = int.Parse(parameters[0]);
                        string period = parameters[1];

                        await LastfmAPI.TopAlbums(Context, user.Username, limit, period);
                    }
                }
                else await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmCommands.cs LfArtist", ex.ToString()));
            }
        }



        [Command("lf tar")]
        public async Task LfTopArtist(params string[] parameters)
        {
            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    if (parameters.Length == 0) await LastfmAPI.TopArtists(Context, user.Username, 10, "overall");
                    else
                    {
                        try { parameters = LastfmFunctions.TopLastfmCheck(parameters); }
                        catch (Exception ex) { await ReplyAsync(ex.Message); return; }

                        int limit = int.Parse(parameters[0]);
                        string period = parameters[1];

                        await LastfmAPI.TopArtists(Context, user.Username, limit, period);
                    }

                }
                else await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmCommands.cs LfTopArtist", ex.ToString()));
            }
        }



        [Command("lf np")]
        public async Task LfNowPlaying()
        {
            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    await LastfmAPI.NowPlaying(Context, user.Username);
                }
                else await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmCommands.cs LfNowPlaying", ex.ToString()));
            }
        }



        [Command("lf rc")]
        public async Task LfRecent(int limit = 10)
        {
            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    await LastfmAPI.Recents(Context, user.Username, limit);
                }
                else await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmCommands.cs LfRecent", ex.ToString()));
            }
        }



        [Command("lf artist")]
        public async Task LfArtist([Remainder]string artist)
        {
            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    await LastfmAPI.Artist(Context, user.Username, artist);
                }
                else await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmCommands.cs LfArtist", ex.ToString()));
            }
        }
    }
}
