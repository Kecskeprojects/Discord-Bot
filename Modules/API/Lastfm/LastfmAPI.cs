using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord_Bot.Modules.API.Lastfm.LastfmClasses;
using Discord_Bot.Modules.ListClasses;

namespace Discord_Bot.Modules.API.Lastfm
{
    public class LastfmAPI : LastfmFunctions
    {
        public static async Task TopTracks(SocketCommandContext context, string name, int limit, string period)
        {
            try
            {
                //Getting data from api
                var temp = await RequestHandler("user.gettoptracks", name, limit: limit, period: period);
                var response = JsonConvert.DeserializeObject<TopTrackClass.TopTrack>(temp.Content);

                if (response.TopTracks != null)
                {
                    int totalplays = await TotalPlays(name, period);

                    //Building embed
                    EmbedBuilder builder = new();

                    //Get nickname if possible
                    string temp_name = GetNickname(context);

                    builder.WithAuthor(temp_name + "'s Top Tracks...", iconUrl: "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png");

                    string[] list = { "", "", "" }; int i = 1; int index = 0;
                    foreach (TopTrackClass.Track track in response.TopTracks.Track)
                    {
                        list[index] += $"`#{i}`**{track.Name}** by **{track.Artist.Name}** - *{Math.Round(double.Parse(track.PlayCount) / totalplays * 100)}%* (*{track.PlayCount} plays*)\n";

                        //If we went through 10 results, start filling a new list page
                        if (i % 10 == 0) index++;

                        i++;
                    }

                    //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
                    foreach (var item in list)
                    {
                        if(item != "") builder.AddField("\u200b", item, false);
                    }

                    builder.WithFooter("Total plays: " + totalplays);
                    builder.WithCurrentTimestamp();
                    builder.WithColor(Color.Red);

                    await context.Channel.SendMessageAsync("", false, builder.Build());
                }
                else await context.Channel.SendMessageAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs TopTracks", ex.ToString()));
            }
        }



        public static async Task TopAlbums(SocketCommandContext context, string name, int limit, string period)
        {
            try
            {
                //Getting data from api
                var temp = await RequestHandler("user.gettopalbums", name, limit: limit, period: period);
                var response = JsonConvert.DeserializeObject<TopAlbumClass.TopAlbum>(temp.Content);

                if (response.TopAlbums != null)
                {
                    int totalplays = await TotalPlays(name, period);

                    //Building embed
                    EmbedBuilder builder = new();

                    //Get nickname if possible
                    string temp_name = GetNickname(context);

                    builder.WithAuthor(temp_name + "'s Top Albums...", iconUrl: "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png");
                    builder.WithThumbnailUrl(response.TopAlbums.Album[0].Image[1].Text);

                    string[] list = { "", "", "" }; int i = 1; int index = 0;
                    foreach (TopAlbumClass.Album album in response.TopAlbums.Album)
                    {
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

                    builder.WithFooter("Total plays: " + totalplays);
                    builder.WithCurrentTimestamp();
                    builder.WithColor(Color.Red);

                    await context.Channel.SendMessageAsync("", false, builder.Build());
                }
                else await context.Channel.SendMessageAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs TopArtists", ex.ToString()));
            }
        }



        public static async Task TopArtists(SocketCommandContext context, string name, int limit, string period)
        {
            try
            {
                //Getting data from api
                var temp = await RequestHandler("user.gettopartists", name, limit: limit, period: period);
                var response = JsonConvert.DeserializeObject<TopArtistClass.TopArtist>(temp.Content);

                if (response.TopArtists != null)
                {
                    int totalplays = await TotalPlays(name, period);

                    //Building embed
                    EmbedBuilder builder = new();

                    //Get nickname if possible
                    string temp_name = GetNickname(context);

                    builder.WithAuthor(temp_name + "'s Top Artists...", iconUrl: "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png");

                    string[] list = { "", "", "" }; int i = 1; int index = 0;
                    foreach (TopArtistClass.Artist artist in response.TopArtists.Artist)
                    {
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

                    builder.WithFooter("Total plays: " + totalplays);
                    builder.WithCurrentTimestamp();
                    builder.WithColor(Color.Red);

                    await context.Channel.SendMessageAsync("", false, builder.Build());
                }
                else await context.Channel.SendMessageAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs TopArtists", ex.ToString()));
            }
        }



        public static async Task NowPlaying(SocketCommandContext context, string name)
        {
            try
            {
                //Cyclically retry checking for song
                int tryCount = 0;
                while(tryCount < 4)
                {
                    //Getting data from api
                    var temp = await RequestHandler("user.getrecenttracks", name, limit: 1);
                    var response = JsonConvert.DeserializeObject<RecentClass.Recent>(temp.Content);

                    if (response.RecentTracks != null)
                    {
                        //If the Attr is not empty in the first index, it means the first song is a song that is currently playing
                        if (response.RecentTracks.Track[0].Attr != null)
                        {
                            RecentClass.Track nowplaying = response.RecentTracks.Track[0];

                            //Building embed
                            EmbedBuilder builder = new();

                            //Get nickname if possible
                            string temp_name = GetNickname(context);

                            builder.WithAuthor(temp_name + " is currently listening to...", iconUrl: "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png");

                            builder.WithTitle(nowplaying.Name);
                            builder.WithUrl(nowplaying.Url);

                            builder.AddField($"By *{nowplaying.Artist.Text}*", $"**On *{nowplaying.Album.Text}***");

                            builder.WithThumbnailUrl(nowplaying.Image[1].Text);

                            builder.WithCurrentTimestamp();
                            builder.WithColor(Color.Red);

                            await context.Channel.SendMessageAsync("", false, builder.Build());
                            break;
                        }
                        else
                        {
                            //If no currently playing song is found, wait and try once more
                            //This is to try and go around lastfm not always recognizing when user is playing a song on spotify for example
                            if(tryCount < 4)
                            {
                                tryCount++;
                                await Task.Delay(200);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
                        break;
                    }
                }

                //If in all 4 tries, no current song was found, user is most likely not listening to anything
                if(tryCount == 4)
                {
                    await context.Channel.SendMessageAsync("You are not listening to anything currently!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs NowPlaying", ex.ToString()));
            }
        }



        public static async Task Recents(SocketCommandContext context, string name, int limit)
        {
            try
            {
                //Getting data from api
                var temp = await RequestHandler("user.getrecenttracks", name, limit: limit);
                var response = JsonConvert.DeserializeObject<RecentClass.Recent>(temp.Content);

                if(response.RecentTracks != null)
                {
                    //Building embed
                    EmbedBuilder builder = new();

                    //Get nickname if possible
                    string temp_name = GetNickname(context);

                    builder.WithAuthor(temp_name + " recently listened to...", iconUrl: "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png");

                    string[] list = { "", "", "" }; int i = 1; int index = 0;
                    foreach (RecentClass.Track track in response.RecentTracks.Track)
                    {
                        if (i > limit) break;
                        list[index] += $"`#{ i}` **{track.Name}** by **{track.Artist.Text}** - *";
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

                    builder.WithCurrentTimestamp();
                    builder.WithColor(Color.Red);

                    await context.Channel.SendMessageAsync("", false, builder.Build());
                }
                else await context.Channel.SendMessageAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs Recent", ex.ToString()));
            }
        }

        public static async Task Artist(SocketCommandContext context, string name, string artist_name)
        {
            try
            {
                //Declarations
                List<TopAlbumClass.Album> albums = new();
                List<TopTrackClass.Track> tracks = new();
                int page = 1; int totalpage = 0;

                page = 1;
                do
                {
                    //Getting data from api
                    var temp = await RequestHandler("user.gettopalbums", name, limit: 1000, page: page);
                    var album_res = JsonConvert.DeserializeObject<TopAlbumClass.TopAlbum>(temp.Content);

                    if(album_res.TopAlbums == null)
                    {
                        await context.Channel.SendMessageAsync("Error during request!\nCheck to make sure if you set your lastfm username correctly!");
                        return;
                    }

                    foreach (var album in album_res.TopAlbums.Album)
                    {
                        if (album.Artist.Name.ToLower() == artist_name.ToLower()) albums.Add(album);
                    }

                    totalpage = int.Parse(album_res.TopAlbums.Attr.TotalPages);
                    page++;
                } while (page <= totalpage);
                
                //if there are no albums found, they have not listened to the artist
                if(albums.Count == 0)
                {
                    await context.Channel.SendMessageAsync("You haven't listened to this artist!"); return;
                }


                page = 1; totalpage = 0;
                do
                {
                    //Getting data from api
                    var temp = await RequestHandler("user.gettoptracks", name, limit: 1000, page: page);
                    var track_res = JsonConvert.DeserializeObject<TopTrackClass.TopTrack>(temp.Content);

                    foreach (var track in track_res.TopTracks.Track)
                    {
                        if (track.Artist.Name.ToLower() == artist_name.ToLower()) tracks.Add(track);
                    }

                    totalpage = int.Parse(track_res.TopTracks.Attr.TotalPages);
                    page++;
                } while (page <= totalpage);

                int playcount = 0;
                foreach (TopTrackClass.Track track in tracks)
                {
                    playcount += int.Parse(track.PlayCount);
                }


                //Building embed
                EmbedBuilder builder = new();

                //Get nickname if possible
                string temp_name = GetNickname(context);

                builder.WithAuthor(temp_name + "'s stats for " + albums[0].Artist.Name, iconUrl: "https://cdn.discordapp.com/attachments/891418209843044354/923401581704118314/last_fm.png");
                builder.WithDescription($"You have listened to this artist **{playcount}** times.\nYou listened to **{albums.Count}** of their albums and **{tracks.Count}** of their tracks.");
                builder.WithThumbnailUrl(albums[0].Image[1].Text);


                string list = ""; int i = 1;
                foreach (TopAlbumClass.Album album in albums)
                {
                    if (i > 5) break;
                    list += $"`#{ i}` **{album.Name}**  (*{album.PlayCount} plays*)\n";
                    i++;
                }
                builder.AddField("Top Albums", list, false);

                list = ""; i = 1;
                foreach (TopTrackClass.Track track in tracks)
                {
                    if (i > 8) break;
                    list += $"`#{ i}` **{track.Name}**  (*{track.PlayCount} plays*)\n";
                    i++;
                }
                builder.AddField("Top Tracks", list, false);

                builder.WithCurrentTimestamp();
                builder.WithColor(Color.Red);

                await context.Channel.SendMessageAsync("", false, builder.Build());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs Artist", ex.ToString()));
            }
        }
    }
}
