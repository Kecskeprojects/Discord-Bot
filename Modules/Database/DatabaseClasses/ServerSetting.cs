using System.Data;

namespace Discord_Bot.Modules.Database.DatabaseClasses
{
    public class ServerSetting
    {
        public ulong ServerId { get; set; }
        public ulong MusicChannel { get; set; }
        public ulong RoleChannel { get; set; }
        public string TwitchChannelId { get; set; }
        public string TwitchChannelLink { get; set; }
        public ulong TwitchNotificationChannel { get; set; }
        public ulong TwitchNotificationRole { get; set; }

        public ServerSetting() { }

        public ServerSetting(DataRow row)
        {
            ServerId = ulong.Parse(row[0].ToString());
            MusicChannel = ulong.Parse(row[1].ToString());
            RoleChannel = ulong.Parse(row[2].ToString());
            TwitchChannelId = row[3].ToString();
            TwitchChannelLink = row[4].ToString();
            TwitchNotificationChannel = ulong.Parse(row[5].ToString());
            TwitchNotificationRole = ulong.Parse(row[6].ToString());
        }
    }
}
