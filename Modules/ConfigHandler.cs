using Discord_Bot.Modules.ListClasses;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Modules
{
    public class ConfigHandler
    {
        private struct Config
        {
            public string token;
            public string img;
            public int bitrate;
            public string twitch_Client_Id;
            public string spotify_Client_Id;
            public string spotify_Client_Secret;
            public string lastfm_API_Key;
            public string lastfm_API_Secret;
            public string[] youtube_API_Keys;
        }

        private Config conf = new()
        {
            token = "",
            img = "",
            bitrate = 0,
            twitch_Client_Id = "",
            spotify_Client_Id = "",
            spotify_Client_Secret = "",
            lastfm_API_Key = "",
            lastfm_API_Secret = "",
            youtube_API_Keys = Array.Empty<string>()
        };

        public ConfigHandler()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets\\config.json").Replace(@"\", @"\\");

            if (!File.Exists(configPath))
            {
                using (StreamWriter sw = File.AppendText(configPath)) sw.WriteLine(JsonConvert.SerializeObject(conf));

                Console.WriteLine("New Config initialized! Need to fill in values before running commands!");
                Global.Logs.Add(new Log("LOG", "New Config initialized! Need to fill in values before running commands!"));

                throw new Exception("NO CONFIG AVAILABLE! Go to executable path and fill out newly created file!");
            }

            using StreamReader reader = new(configPath);
            conf = JsonConvert.DeserializeObject<Config>(reader.ReadToEnd());
        }

        public string Token { get { return conf.token; } }

        public string Img { get { return conf.img; } }

        public int Bitrate { get { return conf.bitrate; } }

        public string Twitch_Client_Id { get { return conf.twitch_Client_Id; } }

        public string Spotify_Client_Id { get { return conf.spotify_Client_Id; } }

        public string Spotify_Client_Secret { get { return conf.spotify_Client_Secret; } }

        public string Lastfm_API_Key { get { return conf.lastfm_API_Key; } }

        public string Lastfm_API_Secret { get { return conf.lastfm_API_Secret; } }

        public string[] Youtube_API_Keys { get { return conf.youtube_API_Keys; } }
    }
}
