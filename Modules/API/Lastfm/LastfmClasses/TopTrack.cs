using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Modules.API.Lastfm.LastfmClasses
{
    public class TopTrackClass
    {
        public class TopTrack
        {
            [JsonProperty("toptracks")]
            public Toptracks TopTracks { get; set; }
        }
        public class Toptracks
        {
            [JsonProperty("track")]
            public List<Track> Track { get; set; }

            [JsonProperty("@attr")]
            public Attr Attr { get; set; }
        }

        public class Track
        {
            [JsonProperty("streamable")]
            public Streamable Streamable { get; set; }

            [JsonProperty("mbid")]
            public string Mbid { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("image")]
            public List<Image> Image { get; set; }

            [JsonProperty("artist")]
            public Artist Artist { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("duration")]
            public string Duration { get; set; }


            [JsonProperty("@attr")]
            public Attr Attr { get; set; }

            [JsonProperty("playcount")]
            public string PlayCount { get; set; }
        }

        public class Attr
        {
            [JsonProperty("rank")]
            public string Rank { get; set; }

            [JsonProperty("perPages")]
            public string PerPage { get; set; }

            [JsonProperty("totalPages")]
            public string TotalPages { get; set; }

            [JsonProperty("page")]
            public string Page { get; set; }

            [JsonProperty("total")]
            public string Total { get; set; }

            [JsonProperty("user")]
            public string User { get; set; }
        }
        public class Streamable
        {
            [JsonProperty("fulltrack")]
            public string FullTrack { get; set; }

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

        public class Artist
        {
            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("mbid")]
            public string Mbid { get; set; }
        }
    }
}
