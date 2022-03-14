using System;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using System.Linq;
using Discord.Commands;
using Discord_Bot.Modules.ListClasses;

namespace Discord_Bot.Modules.API
{
    class SpotifyAPI
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
                Global.Logs.Add(new Log("ERROR", "Spotify_api.cs Spotify_search", ex.ToString()));
            }
            return -1;
            
        }


        //The function running the query
        private static async Task<int> Run(SocketCommandContext context, string query)
        {
            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(Program.Config.Spotify_Client_Id, Program.Config.Spotify_Client_Secret));
            var spotify = new SpotifyClient(config);

            query = query[25..];
            
            if (query.StartsWith("track"))
            {
                query = query.Split('/')[1][..22];

                var track = await spotify.Tracks.Get(query);

                if (track != null)
                {
                    string temp = track.Name + " " + track.Artists[0].Name;

                    Console.WriteLine("Result: " + temp);
                    Global.Logs.Add(new Log("LOG", "Result:  " + temp));

                    if (YoutubeAPI.Searching(context, temp) == 0)
                    {
                        return 0;
                    }
                    else return -2;
                }
            }
            else if (query.StartsWith("playlist") || query.StartsWith("album"))
            {
                string[] list = null;

                string type = query.Split('/')[0];
                query = query.Split('/')[1][..22];

                if (type.Equals("playlist"))
                {
                    var playlist = await spotify.Playlists.GetItems(query, new PlaylistGetItemsRequest { Limit = 25 });
                    list = playlist.Items.Select(n => (n.Track as FullTrack).Name + " " + (n.Track as FullTrack).Artists[0].Name).ToArray();
                }
                else
                {
                    var album = await spotify.Albums.GetTracks(query);
                    list = album.Items.Select(n => n.Name + " " + n.Artists[0].Name).ToArray();
                }

                if(list.Length > 0)
                {
                    foreach (string track in list)
                    {
                        Console.WriteLine("List item: " + track);
                        Global.Logs.Add(new Log("LOG", "List item: " + track));
                        YoutubeAPI.Searching(context, track);
                    }

                    await context.Channel.SendMessageAsync("Playlist added!");

                    return 1;
                }
            }
            return -1;
        }
    }
}
