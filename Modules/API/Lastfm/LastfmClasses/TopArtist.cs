using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Modules.API.Lastfm.LastfmClasses
{
    public class TopArtistClass
    {

        public class TopArtist
        {
            [JsonProperty("topartists")]
            public Topartists TopArtists { get; set; }
        }

        public class Topartists
        {
            [JsonProperty("artist")]
            public List<Artist> Artist { get; set; }

            [JsonProperty("@attr")]
            public Attr Attr { get; set; }
        }

        public class Artist
        {
            [JsonProperty("streamable")]
            public string Streamable { get; set; }

            [JsonProperty("image")]
            public List<Image> Image { get; set; }

            [JsonProperty("mbid")]
            public string Mbid { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("playcount")]
            public string PlayCount { get; set; }

            [JsonProperty("@attr")]
            public Attr Attr { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class Image
        {
            [JsonProperty("size")]
            public string Size { get; set; }

            [JsonProperty("#text")]
            public string Text { get; set; }
        }

        public class Attr
        {
            [JsonProperty("rank")]
            public string Rank { get; set; }

            [JsonProperty("perPage")]
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
    }
}
