namespace Discord_Bot.Classes
{
    public class MusicRequest
    {
        public ulong? ServerId { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public string Duration { get; set; }
        public string User { get; set; }

        public MusicRequest(ulong serverId, string uRL, string title, string thumbnail, string duration, string user)
        {
            ServerId = serverId;
            URL = uRL;
            Title = title;
            Thumbnail = thumbnail;
            Duration = duration;
            User = user;
        }
    }
}
