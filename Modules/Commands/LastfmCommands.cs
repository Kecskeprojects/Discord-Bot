using Discord;
using Discord.Commands;
using Discord_Bot.Modules.API.Lastfm;
using Discord_Bot.Modules.API.Lastfm.LastfmClasses;
using Discord_Bot.Modules.Database;
using Discord_Bot.Modules.ListClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Modules.Commands
{
    public class LastfmCommands : ModuleBase<SocketCommandContext>, Interfaces.ILastfmCommands
    {
        [Command("lf conn")]
        [Alias(new string[] { "lf c", "lf connect" })]
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
        [Alias(new string[] { "lf d", "lf delete", "lf disc", "lf disconnect" })]
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
        [Alias(new string[] { "lf top tracks", "lf top track", "lf toptracks", "lf toptrack"})]
        public async Task LfTopTrack(params string[] parameters)
        {
            //Checking inputs before anything else
            int limit; string period;
            try
            {
                parameters = LastfmFunctions.LastfmParameterCheck(parameters);

                limit = int.Parse(parameters[0]);
                period = parameters[1];
            }
            catch (Exception ex) { await ReplyAsync(ex.Message); return; }

            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    var response = await LastfmAPI.TopTracks(user.Username, limit, period);

                    if (response != null)
                    {
                        int totalplays = await LastfmFunctions.TotalPlays(user.Username, period);

                        //Getting base of lastfm embed
                        EmbedBuilder builder = LastfmFunctions.BaseEmbed($"{LastfmFunctions.GetNickName(Context)}'s Top Tracks...");
                        builder.WithFooter("Total plays: " + totalplays);

                        string[] list = { "", "", "" }; int i = 1; int index = 0;
                        foreach (TopTrackClass.Track track in response.Track)
                        {
                            //One line in the embed
                            list[index] += $"`#{i}`**{track.Name}** by **{track.Artist.Name}** - *{Math.Round(double.Parse(track.PlayCount) / totalplays * 100)}%* (*{track.PlayCount} plays*)\n";

                            //If we went through 10 results, start filling a new list page
                            if (i % 10 == 0) index++;

                            i++;
                        }

                        //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                        foreach (var item in list)
                        {
                            if (item != "") builder.AddField("\u200b", item, false);
                        }

                        await ReplyAsync("", false, builder.Build());
                    }
                    else await ReplyAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
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
        [Alias(new string[] { "lf top albums", "lf top album", "lf topalbums", "lf topalbum"})]
        public async Task LfTopAlbum(params string[] parameters)
        {
            //Checking inputs before anything else
            int limit; string period;
            try
            {
                parameters = LastfmFunctions.LastfmParameterCheck(parameters);

                limit = int.Parse(parameters[0]);
                period = parameters[1];
            }
            catch (Exception ex) { await ReplyAsync(ex.Message); return; }

            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    var response = await LastfmAPI.TopAlbums(user.Username, limit, period);

                    if (response != null)
                    {
                        int totalplays = await LastfmFunctions.TotalPlays(user.Username, period);

                        //Getting base of lastfm embed
                        EmbedBuilder builder = LastfmFunctions.BaseEmbed($"{LastfmFunctions.GetNickName(Context)}'s Top Albums...");
                        builder.WithFooter("Total plays: " + totalplays);
                        builder.WithThumbnailUrl(response.Album[0].Image[1].Text);

                        string[] list = { "", "", "" }; int i = 1; int index = 0;
                        foreach (TopAlbumClass.Album album in response.Album)
                        {
                            //One line in the embed
                            list[index] += $"`#{i}`**{album.Name}** by **{album.Artist.Name}** - *{Math.Round(double.Parse(album.PlayCount) / totalplays * 100)}%* (*{album.PlayCount} plays*)\n";

                            //If we went through 10 results, start filling a new list page
                            if (i % 10 == 0) index++;

                            i++;
                        }

                        //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                        foreach (var item in list)
                        {
                            if (item != "") builder.AddField("\u200b", item, false);
                        }

                        await ReplyAsync("", false, builder.Build());
                    }
                    else await ReplyAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
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
        [Alias(new string[] { "lf top artists", "lf top artist", "lf topartist", "lf topartists"})]
        public async Task LfTopArtist(params string[] parameters)
        {
            //Checking inputs before anything else
            int limit; string period;
            try
            {
                parameters = LastfmFunctions.LastfmParameterCheck(parameters);

                limit = int.Parse(parameters[0]);
                period = parameters[1];
            }
            catch (Exception ex) { await ReplyAsync(ex.Message); return; }


            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    var response = await LastfmAPI.TopArtists(user.Username, limit, period);

                    if (response != null)
                    {
                        int totalplays = await LastfmFunctions.TotalPlays(user.Username, period);

                        //Getting base of lastfm embed
                        EmbedBuilder builder = LastfmFunctions.BaseEmbed($"{LastfmFunctions.GetNickName(Context)}'s Top Artists...");
                        builder.WithFooter("Total plays: " + totalplays);

                        string[] list = { "", "", "" }; int i = 1; int index = 0;
                        foreach (TopArtistClass.Artist artist in response.Artist)
                        {
                            //One line of the embed
                            list[index] += $"`#{i}`**{artist.Name}** - *{Math.Round(double.Parse(artist.PlayCount) / totalplays * 100)}%* (*{artist.PlayCount} plays*)\n";

                            //If we went through 10 results, start filling a new list page
                            if (i % 10 == 0) index++;

                            i++;
                        }

                        //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                        foreach (var item in list)
                        {
                            if (item != "") builder.AddField("\u200b", item, false);
                        }

                        await ReplyAsync("", false, builder.Build());
                    }
                    else await ReplyAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
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
        [Alias(new string[] { "lf nowplaying", "lf now playing"})]
        public async Task LfNowPlaying()
        {
            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    var nowPlaying = await LastfmAPI.NowPlaying(user.Username);

                    if (nowPlaying != null)
                    {
                        if (nowPlaying.Artist != null)
                        {
                            //Getting base of lastfm embed
                            EmbedBuilder builder = LastfmFunctions.BaseEmbed($"{LastfmFunctions.GetNickName(Context)} is currently listening to...");
                            builder.WithThumbnailUrl(nowPlaying.Image[1].Text);

                            builder.WithTitle(nowPlaying.Name);
                            builder.WithUrl(nowPlaying.Url);
                            builder.AddField($"By *{nowPlaying.Artist.Text}*", $"**On *{nowPlaying.Album.Text}***");

                            await ReplyAsync("", false, builder.Build());
                        }
                        else await ReplyAsync("You are not listening to anything currently!");
                    }
                    else await ReplyAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
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
        [Alias(new string[] {"lf recent", "lf recents"})]
        public async Task LfRecent(int limit = 10)
        {
            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    var response = await LastfmAPI.Recents(user.Username, limit);

                    if (response != null)
                    {
                        //Getting base of lastfm embed
                        EmbedBuilder builder = LastfmFunctions.BaseEmbed($"{LastfmFunctions.GetNickName(Context)} recently listened to...");

                        string[] list = { "", "", "" }; int i = 1; int index = 0;
                        foreach (RecentClass.Track track in response.Track)
                        {
                            if (i > limit) break;
                            list[index] += $"`#{i}` **{track.Name}** by **{track.Artist.Text}** - *";
                            list[index] += track.Attr != null ? "Now playing*" : track.Date.Text.Replace(DateTime.Now.Year.ToString(), "") + "*";
                            list[index] += "\n";

                            //If we went through 10 results, start filling a new list page
                            if (i % 10 == 0) index++;

                            i++;
                        }

                        //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                        foreach (var item in list)
                        {
                            if (item != "") builder.AddField("\u200b", item, false);
                        }

                        await ReplyAsync("", false, builder.Build());
                    }
                    else await ReplyAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
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
        [Alias(new string[] { "lf a" })]
        public async Task LfArtist([Remainder]string artist)
        {
            try
            {
                var user = DBFunctions.LastfmUserGet(Context.User.Id);

                if (user != null)
                {
                    var response = await LastfmAPI.Artist(user.Username, artist);

                    if(response != null)
                    {
                        if(response.Item1.Count != 0)
                        {
                            //We give the tuple response to named variables
                            var albums = response.Item1; var tracks = response.Item2;

                            //Total plays of artist
                            int playcount = 0;
                            foreach (TopTrackClass.Track track in tracks)
                            {
                                playcount += int.Parse(track.PlayCount);
                            }

                            //Getting base of lastfm embed
                            EmbedBuilder builder = LastfmFunctions.BaseEmbed($"{LastfmFunctions.GetNickName(Context)}'s stats for {albums[0].Artist.Name}");
                            builder.WithThumbnailUrl(albums[0].Image[1].Text);

                            builder.WithDescription($"You have listened to this artist **{playcount}** times.\nYou listened to **{albums.Count}** of their albums and **{tracks.Count}** of their tracks.");

                            //Assemblying list of top albums
                            string list = ""; int i = 1;
                            foreach (TopAlbumClass.Album album in albums)
                            {
                                if (i > 5) break;
                                list += $"`#{i}` **{album.Name}**  (*{album.PlayCount} plays*)\n";
                                i++;
                            }
                            builder.AddField("Top Albums", list, false);

                            //Assemblying list of top tracks
                            list = ""; i = 1;
                            foreach (TopTrackClass.Track track in tracks)
                            {
                                if (i > 8) break;
                                list += $"`#{i}` **{track.Name}**  (*{track.PlayCount} plays*)\n";
                                i++;
                            }
                            builder.AddField("Top Tracks", list, false);

                            await ReplyAsync("", false, builder.Build());
                        }
                        else await ReplyAsync("You haven't listened to this artist!");
                    }
                    else await ReplyAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
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

        [Command("lf wk")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] {"lf whoknows", "lf whoknow"})]
        public async Task LfWhoKnows([Remainder] string input = "")
        {
            try
            {
                //Variable declarations
                var users = DBFunctions.LastfmUsers();
                Dictionary<string, int> plays = new();
                string searched = "";

                //Download inactive users
                await Context.Guild.DownloadUsersAsync();

                //In case user doesn't give a song, we check if they are playing something
                if (input == "")
                {
                    //Check if they are in the database
                    var user = DBFunctions.LastfmUserGet(Context.User.Id);
                    if(user != null)
                    {
                        //Check if they are playing something
                        var nowPlaying = await LastfmAPI.NowPlaying(user.Username);
                        if (nowPlaying != null)
                        {
                            if (nowPlaying.Artist != null)
                            {
                                //Get artist's name and the track for search
                                string artist_name = nowPlaying.Artist.Text;
                                string track_name = nowPlaying.Name;

                                foreach (var item in users)
                                {
                                    //Check if user is in given server
                                    var temp_user = Context.Guild.GetUser(item.UserId);
                                    if (temp_user != null)
                                    {
                                        //Get their nickname if they have one
                                        string temp_name = temp_user.Nickname ?? temp_user.Username;

                                        //Get their number of plays on given song
                                        var request = await LastfmAPI.TrackPlays(item.Username, artist_name, track_name);

                                        if (request != null)
                                        {
                                            if (item == users[0])
                                            {
                                                //Save the names for use in embed
                                                searched = $"{request.Name} by {request.Artist.Name}";
                                            }

                                            if (request.Userplaycount != "0")
                                            {
                                                //Add user to dictionary
                                                plays.Add(temp_name, int.Parse(request.Userplaycount));
                                            }
                                        }
                                        else { await ReplyAsync("Track not found!"); return; }
                                    }
                                }
                            }
                            else { await ReplyAsync("You are not listening to anything currently!"); return; }
                        }
                        else { await ReplyAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!"); return; }
                    }
                    else { await ReplyAsync("You have yet to connect a username to your discord account. Use the !lf conn [username] command to do so!"); return; }
                }
                else if (input.Contains('>'))
                {
                    //Get artist's name and the track for search
                    string artist_name = input.Split('>')[0].ToLower();
                    string track_name = input.Split('>')[1].ToLower();

                    foreach (var item in users)
                    {
                        //Check if user is in given server
                        var temp_user = Context.Guild.GetUser(item.UserId);
                        if (temp_user != null)
                        {
                            //Get their nickname if they have one
                            string temp_name = temp_user.Nickname ?? temp_user.Username;

                            //Get their number of plays on given song
                            var request = await LastfmAPI.TrackPlays(item.Username, artist_name, track_name);

                            if (request != null)
                            {
                                if (item == users[0])
                                {
                                    //Save the names for use in embed
                                    searched = $"{request.Name} by {request.Artist.Name}";
                                }

                                if(request.Userplaycount != "0")
                                {
                                    //Add user to dictionary
                                    plays.Add(temp_name, int.Parse(request.Userplaycount));
                                }
                            }
                            else { await ReplyAsync("Track not found!"); return; }
                        }
                    }
                }
                else
                {
                    //Get artist's name for search
                    string artist_name = input.ToLower();

                    foreach (var item in users)
                    {
                        //Check if user is in given server
                        var temp_user = Context.Guild.GetUser(item.UserId);
                        if (temp_user != null)
                        {
                            //Get their nickname if they have one
                            string temp_name = temp_user.Nickname ?? temp_user.Username;

                            //Get their number of plays on given artists
                            var request = await LastfmAPI.ArtistPlays(item.Username, artist_name);

                            if (request != null)
                            {
                                if (item == users[0])
                                {
                                    //Save the name for use in embed
                                    searched = request.Name;
                                }

                                if(request.Stats.Userplaycount != "0")
                                {
                                    //Add user to dictionary
                                    plays.Add(temp_name, int.Parse(request.Stats.Userplaycount));
                                }
                            }
                            else { await ReplyAsync("artist not found!"); return; }
                        }
                    }
                }

                if(plays.Count > 0)
                {
                    plays = plays.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                    //Getting base of lastfm embed
                    EmbedBuilder builder = LastfmFunctions.BaseEmbed($"Server ranking for:\n{searched}");

                    string[] list = { "", "", "" }; int i = 1; int index = 0;
                    foreach (var userplays in plays)
                    {
                        //One line in embed
                        list[index] += $"`#{i}` **{userplays.Key}** with *{userplays.Value} plays*";
                        list[index] += "\n";

                        //If we went through 15 results, start filling a new list page
                        if (i % 15 == 0) index++;

                        i++;
                    }

                    //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                    foreach (var item in list)
                    {
                        if (item != "") builder.AddField("\u200b", item, false);
                    }

                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    await ReplyAsync("No one has played this song according to last.fm!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmCommands.cs LfWhoKnows", ex.ToString()));
            }
        }
    }
}
