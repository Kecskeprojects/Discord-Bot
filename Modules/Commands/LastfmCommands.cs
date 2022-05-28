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
                if (DBFunctions.LastfmGet(Context.User.Id) != null)
                {
                    await ReplyAsync("you have a lastfm account connected to your discord account already!");
                    return;
                }
                else
                {
                    if (DBFunctions.LastfmAdd(Context.User.Id, name) > 0) await ReplyAsync("Username " + name + " connected to account!");
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
                if (DBFunctions.LastfmRemove(Context.User.Id) > 0)
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
                var row = DBFunctions.LastfmGet(Context.User.Id);

                if (row != null)
                {
                    string username = row[1].ToString();
                    if (parameters.Length == 0) await LastfmAPI.TopTracks(Context, username, 10, "overall");
                    else
                    {
                        try { parameters = LastfmFunctions.TopLastfmCheck(parameters); }
                        catch (Exception ex) { await ReplyAsync(ex.Message); return; }

                        int limit = int.Parse(parameters[0]);
                        string period = parameters[1];

                        await LastfmAPI.TopTracks(Context, username, limit, period);
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
                var row = DBFunctions.LastfmGet(Context.User.Id);

                if (row != null)
                {
                    string username = row[1].ToString();
                    if (parameters.Length == 0) await LastfmAPI.TopAlbums(Context, username, 10, "overall");
                    else
                    {
                        try { parameters = LastfmFunctions.TopLastfmCheck(parameters); }
                        catch (Exception ex) { await ReplyAsync(ex.Message); return; }

                        int limit = int.Parse(parameters[0]);
                        string period = parameters[1];

                        await LastfmAPI.TopAlbums(Context, username, limit, period);
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
                var row = DBFunctions.LastfmGet(Context.User.Id);

                if (row != null)
                {
                    string username = row[1].ToString();
                    if (parameters.Length == 0) await LastfmAPI.TopArtists(Context, username, 10, "overall");
                    else
                    {
                        try { parameters = LastfmFunctions.TopLastfmCheck(parameters); }
                        catch (Exception ex) { await ReplyAsync(ex.Message); return; }

                        int limit = int.Parse(parameters[0]);
                        string period = parameters[1];

                        await LastfmAPI.TopArtists(Context, username, limit, period);
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
                var row = DBFunctions.LastfmGet(Context.User.Id);

                if (row != null)
                {
                    string username = row[1].ToString();

                    await LastfmAPI.NowPlaying(Context, username);

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
                var row = DBFunctions.LastfmGet(Context.User.Id);

                if (row != null)
                {
                    string username = row[1].ToString();

                    await LastfmAPI.Recents(Context, username, limit);
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
                var row = DBFunctions.LastfmGet(Context.User.Id);

                if (row != null)
                {
                    string username = row[1].ToString();

                    await LastfmAPI.Artist(Context, username, artist);
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
