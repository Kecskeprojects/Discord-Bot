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
        static readonly Dictionary<string, int> keys = new();
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
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Youtube_api.cs Searching", ex.ToString()));
            }
            return -1;
        }



        //The function running the query
        private async Task<int> Run(SocketCommandContext context, string query)
        {
            var global_keys = Program.Config.Youtube_API_Keys;

            //switching api keys
            if (keys[global_keys[youtube_index]] >= 9900)
            {
                keys.Remove(global_keys[youtube_index]);

                int i = 0; Random r = new();

                youtube_index = r.Next(0, keys.Count);

                Console.WriteLine("Key switched out to key in " + youtube_index + " position, value: " + global_keys[i] + "!");
                Global.Logs.Add(new Log("LOG", "Key switched out to key in " + youtube_index + " position, value: " + global_keys[i] + "!"));
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = global_keys[youtube_index],
                ApplicationName = GetType().ToString()
            });

            Console.WriteLine("API key used: " + global_keys[youtube_index]);
            Global.Logs.Add(new Log("Query", "API key used: " + global_keys[youtube_index]));
            
            if (query.Contains("youtu.be") || query.Contains("youtube"))
            {
                if (query.Contains("youtu.be"))
                {
                    query = query[(query.IndexOf(".be") + 4)..];
                    return await Video_Search(context, youtubeService, query);
                }
                else if (query.Contains("watch"))
                {
                    string playlist_id = "";
                    if (query.Contains('&'))
                    {
                        if (!query.Split('&')[1].StartsWith("t="))
                        {
                            playlist_id = query.Split('&')[1][5..];
                            query = query.Split('&')[0];
                        }
                        else query = query.Split('&')[0];
                    }
                    query = query[(query.IndexOf("v=") + 2)..];

                    if (playlist_id != "") _ = Add_Playlist(context, youtubeService, playlist_id);
                    return await Video_Search(context, youtubeService, query);
                }
                else if (query.Contains("playlist"))
                {
                    query = query[(query.IndexOf("=") + 1)..];
                    return await Playlist_Search(context, youtubeService, query);
                }
            }
            else return await Keyword_Search(context, youtubeService, query);
            return -1;
        }



        //Setting the keys to default values
        public static void KeyReset()
        {
            keys.Clear();
            foreach (string item in Program.Config.Youtube_API_Keys) keys.Add(item, 0);

            youtube_index = new Random().Next(0, keys.Count);

            Console.WriteLine("Youtube keys reset!");
            Global.Logs.Add(new Log("LOG", "Youtube keys reset!"));
        }



        //Searching by video id
        private static async Task<int> Video_Search(SocketCommandContext context, YouTubeService youtubeService, string query)
        {
            var global_keys = Program.Config.Youtube_API_Keys;

            string name = (context.User as SocketGuildUser).Nickname ?? context.User.Username;

            var searchListRequest = youtubeService.Videos.List("snippet,contentDetails");
            searchListRequest.Id = query;
            searchListRequest.MaxResults = 1;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();
            keys[global_keys[youtube_index]] += 1;

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
        private static async Task<int> Keyword_Search(SocketCommandContext context, YouTubeService youtubeService, string query)
        {
            var global_keys = Program.Config.Youtube_API_Keys;

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = 1;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();
            keys[global_keys[youtube_index]] += 100;

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
                return await Playlist_Search(context, youtubeService, searchListResponse.Items[0].Id.PlaylistId);
            }
            else if(searchListResponse.Items[0].Id.VideoId != null)
            {
                return await Video_Search(context, youtubeService, searchListResponse.Items[0].Id.VideoId);
            }
            return -1;
        }



        //Adding playlist
        private static async Task<int> Playlist_Search(SocketCommandContext context, YouTubeService youtubeService, string query)
        {
            var global_keys = Program.Config.Youtube_API_Keys;

            var searchListRequest = youtubeService.PlaylistItems.List("snippet,contentDetails");

            searchListRequest.PlaylistId = query;
            searchListRequest.MaxResults = 50;

            PlaylistItemListResponse searchListResponse = new();
            try
            {
                // Call the search.list method to retrieve results matching the specified query term.
                searchListResponse = await searchListRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Youtube_api.cs Playlist_Search", ex.ToString()));
            }

            keys[global_keys[youtube_index]] += 1;

            if (searchListResponse.Items.Count == 0)
            {
                Console.WriteLine("No playlists found!");
                Global.Logs.Add(new Log("QUERY", "No playlists found!"));
                return -1;
            }

            foreach (var item in searchListResponse.Items)
            {
                await Video_Search(context, youtubeService, item.ContentDetails.VideoId);
            }

            Console.WriteLine("Youtube playlist query complete!\n");
            Global.Logs.Add(new Log("QUERY", "Youtube playlist query Complete!"));

            await context.Channel.SendMessageAsync("Playlist added!");

            return 1;
        }



        //Checking if user wants to add playlist
        private static async Task Add_Playlist(SocketCommandContext context, YouTubeService youtubeService, string playlist_id)
        {
            var message = await context.Channel.SendMessageAsync("You requested a song from a playlist!\n Do you want to me to add the playlist to the queue?");
            await message.AddReactionAsync(new Emoji("\U00002705"));
            int timer = 0;
            while (timer <= 15)
            {
                var result = await message.GetReactionUsersAsync(new Emoji("\U00002705"), 5).FlattenAsync();
                if (result.Count() > 1) 
                {
                    await Playlist_Search(context, youtubeService, playlist_id);
                    break; 
                }
                await Task.Delay(1000);
                timer++;
            }
            await message.DeleteAsync();
        }
    }
}