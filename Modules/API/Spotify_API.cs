using System;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using System.Linq;
using Discord.Commands;
using Discord_Bot.Modules.ListClasses;

namespace Discord_Bot.Modules.API
{
    class Spotify_API
    {
        //Main function starting the query and catching errors
        public static int SpotifySearch(SocketCommandContext context, string query)
        {
            Console.WriteLine(Global.Current_Time() + ": Spotify Data API: Search");
            Global.Logs.Add(new Log("LOG", "Spotify Data API: Search"));

            try
            {
                int result = Run(context, query).GetAwaiter().GetResult();

                Console.WriteLine(Global.Current_Time() + ": Spotify query complete!\n");
                Global.Logs.Add(new Log("LOG", "Spotify query complete!"));

                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "SpotifyAPI.cs SpotifySearch", ex.ToString()));
            }
            return -1;
            
        }


        //The function running the query
        private static async Task<int> Run(SocketCommandContext context, string query)
        {
            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(Global.Config.Spotify_Client_Id, Global.Config.Spotify_Client_Secret));
            var spotify = new SpotifyClient(config);

            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                //Spotify link's first 25 characters is https://open.spotify.com/ which we cut down
                query = query[25..];

                //what remains is something like this 'track/0IGUXY4JbK18bu9oD4mPIm?si=7cedde80c6874b9c' that we cut into two parts
                //first, the type of song, second, the 22 character long id
                string type = query.Split('/')[0];
                string id = query.Split('/')[1][..22];


                if (type == "track")
                {
                    var track = await spotify.Tracks.Get(id);

                    if (track != null)
                    {
                        string temp = track.Name + " " + track.Artists[0].Name;

                        Console.WriteLine("Result: " + temp);
                        Global.Logs.Add(new Log("LOG", "Result: " + temp));

                        if (YoutubeAPI.Searching(context, temp) == 0)
                        {
                            return 0;
                        }
                        else return -2;
                    }
                }
                else if (type == "playlist" || type == "album")
                {
                    string[] list = null;

                    if (type == "playlist")
                    {
                        var playlist = await spotify.Playlists.GetItems(id, new PlaylistGetItemsRequest { Limit = 25 });

                        list = playlist.Items.Select(n => (n.Track as FullTrack).Name + " " + (n.Track as FullTrack).Artists[0].Name).ToArray();
                    }
                    else
                    {
                        var album = await spotify.Albums.GetTracks(id);
                        list = album.Items.Select(n => n.Name + " " + n.Artists[0].Name).ToArray();
                    }

                    if (list.Length > 0)
                    {
                        foreach (string track in list)
                        {
                            Console.WriteLine("List item: " + track);
                            Global.Logs.Add(new Log("LOG", "List item: " + track));
                            YoutubeAPI.Searching(context, track);
                        }

                        await context.Channel.SendMessageAsync("Playlist added!");
                        Global.Logs.Add(new Log("LOG", "Playlist added!"));

                        return 1;
                    }
                }
            }
            return -1;
        }

        //Lastfm complimentary function
        public static async Task<string> ImageSearch(string artist, string song = "", string[] tags = null)
        {
            try
            {
                var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(Global.Config.Spotify_Client_Id, Global.Config.Spotify_Client_Secret));
                var spotify = new SpotifyClient(config);

                string spotifyArtist = "", spotifyImage = "";

                Console.WriteLine("Who knows command image search:");
                Global.Logs.Add(new Log("LOG", "Who knows command image search:"));

                if (song == "")
                {
                    SearchRequest request = new(SearchRequest.Types.Artist, artist);
                    var result = await spotify.Search.Item(request);

                    foreach (var item in result.Artists.Items)
                    {
                        var artist_genres = item.Genres;
                        var union = artist_genres.Select(x => x.ToLower()).Intersect(tags.Select(x => x.ToLower()));
                        if (union.Any() && item.Name.ToLower() == artist.ToLower())
                        {
                            spotifyArtist = item.Name;
                            spotifyImage = item.Images[0].Url;
                            break;
                        }
                    }

                    if(spotifyArtist == "")
                    {
                        Console.WriteLine("Genre and name check failed, finding first artist with just the same name");
                        Global.Logs.Add(new Log("LOG", "Genre and name check failed, finding first artist with just the same name"));

                        foreach (var item in result.Artists.Items)
                        {
                            if (item.Name.ToLower() == artist.ToLower())
                            {
                                spotifyArtist = item.Name;
                                spotifyImage = item.Images[0].Url;
                                break;
                            }
                        }

                        if(spotifyArtist == "")
                        {
                            Console.WriteLine("Name only search also failed, returning first item on list");
                            Global.Logs.Add(new Log("LOG", "Name only search also failed, returning first item on list"));

                            spotifyArtist = result.Artists.Items[0].Name;
                            spotifyImage = result.Artists.Items[0].Images[0].Url;
                        }
                    }
                }
                else
                {
                    SearchRequest request = new(SearchRequest.Types.Track, song + " " + artist);
                    var result = await spotify.Search.Item(request);

                    foreach (var item in result.Tracks.Items)
                    {
                        var temp_artist = await spotify.Artists.Get(item.Album.Artists[0].Id);

                        var artist_genres = temp_artist.Genres;
                        var union = artist_genres.Select(x => x.ToLower()).Intersect(tags.Select(x => x.ToLower()));
                        if (union.Any() && item.Artists[0].Name.ToLower() == artist.ToLower())
                        {
                            spotifyArtist = item.Artists[0].Name;
                            spotifyImage = item.Album.Images[0].Url;
                            break;
                        }
                    }

                    if (spotifyArtist == "")
                    {
                        Console.WriteLine("Genre and name check failed, finding first artist with just the same name");
                        Global.Logs.Add(new Log("LOG", "Genre and name check failed, finding first artist with just the same name"));

                        foreach (var item in result.Tracks.Items)
                        {
                            if (item.Artists[0].Name.ToLower() == artist.ToLower())
                            {
                                spotifyArtist = item.Artists[0].Name;
                                spotifyImage = item.Album.Images[0].Url;
                                break;
                            }
                        }

                        if (spotifyArtist == "")
                        {
                            Console.WriteLine("Name only search also failed, returning first item on list");
                            Global.Logs.Add(new Log("LOG", "Name only search also failed, returning first item on list"));

                            spotifyArtist = result.Tracks.Items[0].Name;
                            spotifyImage = result.Tracks.Items[0].Album.Images[0].Url;
                        }
                    }
                }

                Console.WriteLine($"Artist found by Last.fm: {artist}\nArtist found by Spotify: {spotifyArtist}\nWith image link: {spotifyImage}");
                Global.Logs.Add(new Log("LOG", $"Artist found by Last.fm: {artist}\nArtist found by Spotify: {spotifyArtist}\nWith image link: {spotifyImage}"));

                return spotifyImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "SpotifyAPI.cs ImageSearch", ex.ToString()));
            }
            return "";
        }
    }
}
