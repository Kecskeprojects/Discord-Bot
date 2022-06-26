using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Discord_Bot.Modules.API.Lastfm.LastfmClasses
{
    public class TrackInfo
    {
        public class Album
        {
            [JsonPropertyName("artist")]
            public string Artist { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("mbid")]
            public string Mbid { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("image")]
            public List<Image> Image { get; set; }

            [JsonPropertyName("@attr")]
            public Attr Attr { get; set; }
        }

        public class Artist
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("mbid")]
            public string Mbid { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }
        }

        public class Attr
        {
            [JsonPropertyName("position")]
            public string Position { get; set; }
        }

        public class Image
        {
            [JsonPropertyName("#text")]
            public string Text { get; set; }

            [JsonPropertyName("size")]
            public string Size { get; set; }
        }

        public class Root
        {
            [JsonPropertyName("track")]
            public Track Track { get; set; }
        }

        public class Streamable
        {
            [JsonPropertyName("#text")]
            public string Text { get; set; }

            [JsonPropertyName("fulltrack")]
            public string Fulltrack { get; set; }
        }

        public class Tag
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }
        }

        public class Toptags
        {
            [JsonPropertyName("tag")]
            public List<Tag> Tag { get; set; }
        }

        public class Track
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("mbid")]
            public string Mbid { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("duration")]
            public string Duration { get; set; }

            [JsonPropertyName("streamable")]
            public Streamable Streamable { get; set; }

            [JsonPropertyName("listeners")]
            public string Listeners { get; set; }

            [JsonPropertyName("playcount")]
            public string Playcount { get; set; }

            [JsonPropertyName("artist")]
            public Artist Artist { get; set; }

            [JsonPropertyName("album")]
            public Album Album { get; set; }

            [JsonPropertyName("userplaycount")]
            public string Userplaycount { get; set; }

            [JsonPropertyName("userloved")]
            public string Userloved { get; set; }

            [JsonPropertyName("toptags")]
            public Toptags Toptags { get; set; }

            [JsonPropertyName("wiki")]
            public Wiki Wiki { get; set; }
        }

        public class Wiki
        {
            [JsonPropertyName("published")]
            public string Published { get; set; }

            [JsonPropertyName("summary")]
            public string Summary { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }
        }
    }
}
