using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Modules.API.Lastfm.LastfmClasses
{
    public class TopAlbumClass
    {
        public class TopAlbum
        {
            [JsonProperty("topalbums")]
            public Topalbums TopAlbums { get; set; }
        }

        public class Topalbums
        {
            [JsonProperty("album")]
            public List<Album> Album { get; set; }

            [JsonProperty("@attr")]
            public Attr Attr { get; set; }
        }

        public class Album
        {
            [JsonProperty("artist")]
            public Artist Artist { get; set; }

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
        public class Artist
        {
            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("mbid")]
            public string Mbid { get; set; }
        }

        public class Image
        {
            [JsonProperty("size")]
            public string Size { get; set; }

            [JsonProperty("#text")]
            public string Text { get; set; }
        }
    }
}
