namespace Discord_Bot.Modules.ListClasses
{
    public class MusicRequest
    {
        public string URL { get; set; }
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public string Duration { get; set; }
        public string User { get; set; }

        public MusicRequest(string uRL, string title, string thumbnail, string duration, string user)
        {
            URL = uRL;
            Title = title;
            Thumbnail = thumbnail;
            Duration = duration;
            User = user;
        }
    }
}
