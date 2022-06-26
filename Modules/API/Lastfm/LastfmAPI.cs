using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord_Bot.Modules.API.Lastfm.LastfmClasses;
using Discord_Bot.Modules.ListClasses;
using RestSharp;

namespace Discord_Bot.Modules.API.Lastfm
{
    public class LastfmAPI
    {
        //The main request handling function
        static readonly RestClient _client = new("http://ws.audioscrobbler.com/2.0/");
        protected static async Task<RestResponse> StandardRequestHandler(string type, string name, int limit = 0, int page = 0, string period = "")
        {
            string request_string = $"?method={type}&user={name}&api_key={Global.Config.Lastfm_API_Key}";

            if (limit > 0)
            {
                request_string += $"&limit={limit}";
            }

            if (page != 0) request_string += $"&page={page}";

            if (period != "") request_string += $"&period={period}";

            request_string += "&format=json";

            var request = new RestRequest(request_string);
            var temp = await _client.GetAsync(request);
            return temp;
        }



        protected static async Task<RestResponse> UserPlayRequests(string type, string username, string artist, string track = "")
        {
            string request_string = $"?method={type}&api_key={Global.Config.Lastfm_API_Key}&artist={artist}&username={username}";

            if (track != "")
            {
                request_string += $"&track={track}";
            }

            request_string += "&format=json";

            var request = new RestRequest(request_string);
            var temp = await _client.GetAsync(request);
            return temp;
        }



        public static async Task<TopTrackClass.Toptracks> TopTracks(string name, int limit, string period)
        {
            try
            {
                //Getting data from api
                var temp = await StandardRequestHandler("user.gettoptracks", name, limit: limit > 31 ? 10 : limit, period: period);
                var response = JsonConvert.DeserializeObject<TopTrackClass.TopTrack>(temp.Content);

                if (response.TopTracks != null)
                {
                    return response.TopTracks;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs TopTracks", ex.ToString()));
            }
            return null;
        }



        public static async Task<TopAlbumClass.Topalbums> TopAlbums(string name, int limit, string period)
        {
            try
            {
                //Getting data from api
                var temp = await StandardRequestHandler("user.gettopalbums", name, limit: limit > 31 ? 10 : limit, period: period);
                var response = JsonConvert.DeserializeObject<TopAlbumClass.TopAlbum>(temp.Content);

                if (response.TopAlbums != null)
                {
                    return response.TopAlbums;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs TopArtists", ex.ToString()));
            }
            return null;
        }



        public static async Task<TopArtistClass.Topartists> TopArtists(string name, int limit, string period)
        {
            try
            {
                //Getting data from api
                var temp = await StandardRequestHandler("user.gettopartists", name, limit: limit > 31 ? 10 : limit, period: period);
                var response = JsonConvert.DeserializeObject<TopArtistClass.TopArtist>(temp.Content);

                if (response.TopArtists != null)
                {
                    return response.TopArtists;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs TopArtists", ex.ToString()));
            }
            return null;
        }



        public static async Task<RecentClass.Track> NowPlaying(string name)
        {
            try
            {
                //Cyclically retry checking for song
                int tryCount = 0;
                while(tryCount < 4)
                {
                    //Getting data from api
                    var temp = await StandardRequestHandler("user.getrecenttracks", name, limit: 1);
                    var response = JsonConvert.DeserializeObject<RecentClass.Recent>(temp.Content);

                    if (response.RecentTracks != null)
                    {
                        //If the Attr is not empty in the first index, it means the first song is a song that is currently playing
                        if (response.RecentTracks.Track[0].Attr != null)
                        {
                            return response.RecentTracks.Track[0];
                        }
                        else
                        {
                            //If no currently playing song is found, wait and try once more
                            //This is to try and go around lastfm not always recognizing when user is playing a song on spotify for example
                            if(tryCount < 4)
                            {
                                tryCount++;

                                Console.WriteLine("Now playing try " + tryCount);
                                Global.Logs.Add(new Log("LOG", "Now playing try " + tryCount));

                                await Task.Delay(200);
                                continue;
                            }
                        }
                    }
                    else break;
                }

                //In case we tried and it was not the request that failed, we send an empty class so we can differentiate issues
                if (tryCount >= 4)
                {
                    return new RecentClass.Track();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs NowPlaying", ex.ToString()));
            }

            return null;
        }



        public static async Task<RecentClass.Recenttracks> Recents(string name, int limit)
        {
            try
            {
                //Getting data from api
                var temp = await StandardRequestHandler("user.getrecenttracks", name, limit: limit > 31 ? 10 : limit);
                var response = JsonConvert.DeserializeObject<RecentClass.Recent>(temp.Content);

                if(response.RecentTracks != null)
                {
                    return response.RecentTracks;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs Recent", ex.ToString()));
            }
            return null;
        }



        public static async Task<ArtistInfo.Artist> ArtistPlays(string name, string artist_name)
        {
            try
            {
                //Getting data from api
                var temp = await UserPlayRequests("artist.getInfo", name, artist_name);
                var response = JsonConvert.DeserializeObject<ArtistInfo.Root>(temp.Content);

                if (response.Artist != null)
                {
                    return response.Artist;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs Recent", ex.ToString()));
            }
            return null;
        }



        public static async Task<TrackInfo.Track> TrackPlays(string name, string artist_name, string track_name)
        {
            try
            {
                //Getting data from api
                var temp = await UserPlayRequests("track.getInfo", name, artist_name, track_name);
                var response = JsonConvert.DeserializeObject<TrackInfo.Root>(temp.Content);

                if (response.Track != null)
                {
                    return response.Track;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs Recent", ex.ToString()));
            }
            return null;
        }



        public static async Task<Tuple<List<TopAlbumClass.Album>, List<TopTrackClass.Track>>> Artist(string name, string artist_name)
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
                    var temp = await StandardRequestHandler("user.gettopalbums", name, limit: 1000, page: page);
                    var album_res = JsonConvert.DeserializeObject<TopAlbumClass.TopAlbum>(temp.Content);

                    //We send null in case request completely failed
                    if(album_res.TopAlbums == null)
                    {
                        return null;
                    }

                    foreach (var album in album_res.TopAlbums.Album)
                    {
                        if (album.Artist.Name.ToLower() == artist_name.ToLower()) albums.Add(album);
                    }

                    totalpage = int.Parse(album_res.TopAlbums.Attr.TotalPages);
                    page++;
                } while (page <= totalpage);
                
                //if there are no albums found, they have not listened to the artist
                //We send a tuple with empty lists
                if(albums.Count == 0)
                {
                    return new Tuple<List<TopAlbumClass.Album>, List<TopTrackClass.Track>>(albums, tracks);
                }


                page = 1; totalpage = 0;
                do
                {
                    //Getting data from api
                    var temp = await StandardRequestHandler("user.gettoptracks", name, limit: 1000, page: page);
                    var track_res = JsonConvert.DeserializeObject<TopTrackClass.TopTrack>(temp.Content);

                    //We send null in case request completely failed
                    if (track_res.TopTracks == null)
                    {
                        return null;
                    }

                    foreach (var track in track_res.TopTracks.Track)
                    {
                        if (track.Artist.Name.ToLower() == artist_name.ToLower()) tracks.Add(track);
                    }

                    totalpage = int.Parse(track_res.TopTracks.Attr.TotalPages);
                    page++;
                } while (page <= totalpage);

                return new Tuple<List<TopAlbumClass.Album>, List<TopTrackClass.Track>>(albums, tracks);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "LastfmAPI.cs Artist", ex.ToString()));
            }

            return null;
        }
    }
}
