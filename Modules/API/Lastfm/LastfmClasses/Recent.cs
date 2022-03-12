using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Modules.API.Lastfm.LastfmClasses
{
    public class RecentClass
    {
        public class Recent
        {
            [JsonProperty("recenttracks")]
            public Recenttracks RecentTracks { get; set; }
        }

        public class Recenttracks
        {
            [JsonProperty("track")]
            public List<Track> Track { get; set; }

            [JsonProperty("@attr")]
            public Attr Attr { get; set; }
        }

        public class Track
        {
            [JsonProperty("artist")]
            public Artist Artist { get; set; }

            [JsonProperty("streamable")]
            public string Streamable { get; set; }

            [JsonProperty("image")]
            public List<Image> Image { get; set; }

            [JsonProperty("mbid")]
            public string Mbid { get; set; }

            [JsonProperty("album")]
            public Album Album { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("@attr")]
            public Attr Attr { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("date")]
            public Date Date { get; set; }
        }
        public class Artist
        {
            [JsonProperty("mbid")]
            public string Mbid { get; set; }

            [JsonProperty("#text")]
            public string Text { get; set; }
        }

        public class Image
        {
            [JsonProperty("size")]
            public string Size { get; set; }

            [JsonProperty("#text")]
            public string Text { get; set; }
        }

        public class Album
        {
            [JsonProperty("mbid")]
            public string Mbid { get; set; }

            [JsonProperty("#text")]
            public string Text { get; set; }
        }

        public class Date
        {
            [JsonProperty("uts")]
            public string Uts { get; set; }

            [JsonProperty("#text")]
            public string Text { get; set; }
        }

        public class Attr
        {
            [JsonProperty("nowplaying")]
            public string NowPlaying { get; set; }

            [JsonProperty("user")]
            public string User { get; set; }

            [JsonProperty("totalPages")]
            public string TotalPages { get; set; }

            [JsonProperty("page")]
            public string Page { get; set; }

            [JsonProperty("total")]
            public string Total { get; set; }

            [JsonProperty("perPage")]
            public string PerPage { get; set; }
        }
    }
}
