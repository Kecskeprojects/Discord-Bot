using System;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Collections.Generic;
using Discord_Bot.Modules.ListClasses;
using Discord.WebSocket;
using Google.Apis.YouTube.v3.Data;

namespace Discord_Bot.Modules.API
{

    public class YoutubeAPI
    {
        private static readonly Dictionary<string, int> keys = new();
        private static int youtube_index = 0;

        [STAThread]

        //Main function starting the api request
        public static int Searching(SocketCommandContext context, string query)
        {
            Console.WriteLine("========================================");
            Console.WriteLine(Global.Current_Time() + ": YouTube Data API: Search");
            Global.Logs.Add(new Log("QUERY", "YouTube Data API: Search"));

            try
            {
                int result = new YoutubeAPI().Run(context, query).GetAwaiter().GetResult();
                Console.WriteLine("========================================\n");
                return result;
            }
            catch (Exception ex)
            {
                //switching api keys if quota is exceeded
                if (ex.ToString().Contains("quotaExceeded") && keys.Count != 0)
                {
                    var current_key = Global.Config.Youtube_API_Keys[youtube_index];

                    keys.Remove(current_key);

                    Random r = new();

                    youtube_index = r.Next(0, keys.Count);
                    current_key = Global.Config.Youtube_API_Keys[youtube_index];

                    Console.WriteLine("Key switched out to key in " + youtube_index + " position, value: " + current_key + "!");
                    Global.Logs.Add(new Log("LOG", "Key switched out to key in " + youtube_index + " position, value: " + current_key + "!"));

                    int result = new YoutubeAPI().Run(context, query).GetAwaiter().GetResult();
                    return result;
                }
                else
                {
                    Console.WriteLine(ex.ToString());
                    Global.Logs.Add(new Log("DEV", ex.Message));
                    Global.Logs.Add(new Log("ERROR", "YoutubeAPI.cs Searching", ex.ToString()));
                }
            }
            return -1;
        }



        //The function running the query
        private async Task<int> Run(SocketCommandContext context, string query)
        {
            var current_key = Global.Config.Youtube_API_Keys[youtube_index];

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = current_key,
                ApplicationName = GetType().ToString()
            });

            Console.WriteLine("API key used: " + current_key);
            Global.Logs.Add(new Log("Query", "API key used: " + current_key));

            if (query.Contains("youtu.be") || query.Contains("youtube"))
            {
                //If the link is a short link youtu.be/..., you have to cut off a different length of it
                if (query.Contains("youtu.be"))
                {
                    query = query[(query.IndexOf(".be") + 4)..];
                    return await VideoSearch(context, youtubeService, query);
                }
                //If the link is a regular youtube link, but has "watch", it is a single video's link
                else if (query.Contains("watch"))
                {
                    //If it has a '&' symbol. it potentially has a playlist_id or some other url parts
                    string playlist_id = "";
                    if (query.Contains('&'))
                    {
                        //Check if it is contains a list id
                        if (query.Contains("list="))
                        {
                            //Cut it into parts and go through them
                            string[] parts = query.Split("&");
                            foreach (var item in parts)
                            {
                                //If we found the one with the list id, save it
                                if (item.StartsWith("list="))
                                {
                                    playlist_id = item[5..];
                                }
                            }
                        }
                        //Cut off the extra parts beyond the video link
                        query = query.Split('&')[0];
                    }
                    //Cut off the "v=" part of the video id
                    query = query[(query.IndexOf("v=") + 2)..];

                    //if it did have a playlist id, start an unawaited function that asks the user if they want it imported
                    if (playlist_id != "") _ = AddPlaylist(context, youtubeService, playlist_id);

                    return await VideoSearch(context, youtubeService, query);
                }
                //If the link is a regular youtube link, but has "playlist", it is a playlist link
                else if (query.Contains("playlist"))
                {
                    query = query[(query.IndexOf("=") + 1)..];
                    return await PlaylistSearch(context, youtubeService, query);
                }
            }
            //In any other case, search the result as a keyword
            else return await KeywordSearch(context, youtubeService, query);

            return -1;
        }



        //Setting the keys to default values
        public static void KeyReset()
        {
            keys.Clear();
            foreach (string item in Global.Config.Youtube_API_Keys) keys.Add(item, 0);

            youtube_index = new Random().Next(0, keys.Count);

            Console.WriteLine("Youtube keys reset!");
            Global.Logs.Add(new Log("LOG", "Youtube keys reset!"));
        }



        //Searching by video id
        private static async Task<int> VideoSearch(SocketCommandContext context, YouTubeService youtubeService, string query)
        {
            var current_key = Global.Config.Youtube_API_Keys[youtube_index];

            string name = (context.User as SocketGuildUser).Nickname ?? context.User.Username;

            var searchListRequest = youtubeService.Videos.List("snippet,contentDetails");
            searchListRequest.Id = query;
            searchListRequest.MaxResults = 1;

            VideoListResponse searchListResponse = new();
            try
            {
                // Call the SearchListRequest method to retrieve results matching the specified query term
                searchListResponse = await searchListRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "YoutubeAPI.cs KeywordSearch", ex.ToString()));
            }

            keys[current_key] += 1;

            if (searchListResponse.Items.Count < 1)
            {
                Console.WriteLine("No videos found!");
                Global.Logs.Add(new Log("QUERY", "No videos found!"));
                return - 1;
            }

            var video = searchListResponse.Items[0];

            string[] temp = { "https://www.youtube.com/watch?v=" + video.Id, video.Snippet.Title.Replace("&#39;", "'"), video.Snippet.Thumbnails.Default__.Url, video.ContentDetails.Duration };
            Global.servers[context.Guild.Id].MusicRequests.Add(new MusicRequest(temp[0], temp[1], temp[2], temp[3], name));

            Console.WriteLine("Youtube video query complete!\n");
            Global.Logs.Add(new Log("QUERY", "Youtube video query Complete!"));

            Console.WriteLine(temp[0] + "\n" + temp[1]);
            Global.Logs.Add(new Log("QUERY", temp[0] + "\n" + temp[1]));

            return 0;
        }



        //Searching for keywords
        private static async Task<int> KeywordSearch(SocketCommandContext context, YouTubeService youtubeService, string query)
        {
            var current_key = Global.Config.Youtube_API_Keys[youtube_index];

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = 1;

            SearchListResponse searchListResponse = new();
            try
            {
                // Call the SearchListRequest method to retrieve results matching the specified query term
                searchListResponse = await searchListRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "YoutubeAPI.cs KeywordSearch", ex.ToString()));
            }

            keys[current_key] += 100;

            if (searchListResponse.Items.Count < 1) 
            { 
                Console.WriteLine("No videos found!"); 
                Global.Logs.Add(new Log("QUERY", "No videos found!")); 
                return -1; 
            }

            Console.WriteLine("Youtube search query complete!\n");
            Global.Logs.Add(new Log("QUERY", "Youtube search query Complete!"));

            if(searchListResponse.Items[0].Id.PlaylistId != null)
            {
                return await PlaylistSearch(context, youtubeService, searchListResponse.Items[0].Id.PlaylistId);
            }
            else if(searchListResponse.Items[0].Id.VideoId != null)
            {
                return await VideoSearch(context, youtubeService, searchListResponse.Items[0].Id.VideoId);
            }
            return -1;
        }



        //Adding playlist
        private static async Task<int> PlaylistSearch(SocketCommandContext context, YouTubeService youtubeService, string query)
        {
            var current_key = Global.Config.Youtube_API_Keys[youtube_index];

            var searchListRequest = youtubeService.PlaylistItems.List("snippet,contentDetails");

            searchListRequest.PlaylistId = query;
            searchListRequest.MaxResults = 50;

            PlaylistItemListResponse searchListResponse = new();
            try
            {
                // Call the SearchListRequest method to retrieve results matching the specified query term
                searchListResponse = await searchListRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "YoutubeAPI.cs PlaylistSearch", ex.ToString()));
            }

            keys[current_key] += 1;

            if (searchListResponse.Items.Count < 1)
            {
                Console.WriteLine("No playlists found!");
                Global.Logs.Add(new Log("QUERY", "No playlists found!"));
                return -1;
            }

            foreach (var item in searchListResponse.Items)
            {
                await VideoSearch(context, youtubeService, item.ContentDetails.VideoId);
            }

            Console.WriteLine("Youtube playlist query complete!\n");
            Global.Logs.Add(new Log("QUERY", "Youtube playlist query Complete!"));

            await context.Channel.SendMessageAsync("Playlist added!");

            return 1;
        }



        //Checking if user wants to add playlist
        private static async Task AddPlaylist(SocketCommandContext context, YouTubeService youtubeService, string playlist_id)
        {
            var message = await context.Channel.SendMessageAsync("You requested a song from a playlist!\n Do you want to me to add the playlist to the queue?");
            await message.AddReactionAsync(new Emoji("\U00002705"));

            //Wait 15 seconds for user to react to message, and then delete it, also delete it if they react, but add playlist
            int timer = 0;
            while (timer <= 15)
            {
                var result = await message.GetReactionUsersAsync(new Emoji("\U00002705"), 5).FlattenAsync();
                if (result.Count() > 1) 
                {
                    await PlaylistSearch(context, youtubeService, playlist_id);
                    break; 
                }
                await Task.Delay(1000);
                timer++;
            }
            await message.DeleteAsync();
        }
    }
}