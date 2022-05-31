using System.Collections.Generic;

namespace Discord_Bot.Modules.ListClasses
{
    public class Server
    {
        public ulong? ServerId { get; set; }

        public ulong MusicChannel { get; set; }

        public ulong RoleChannel { get; set; }

        public string TChannelId { get; set; }

        public string TChannelLink { get; set; }

        public ulong TNotifChannel { get; set; }

        public ulong TNotifRole { get; set; }

        public bool TwitchOnline { get; set; }

        public AudioVars AudioVars { get; set; }

        public List<MusicRequest> MusicRequests { get; set; }


        public Server() { }


        public Server(ulong serverId)
        {
            ServerId = serverId;
            MusicChannel = 0;
            RoleChannel = 0;
            TChannelId = "";
            TChannelLink = "";
            TNotifChannel = 0;
            TNotifRole = 0;
            TwitchOnline = false;
            AudioVars = new() { Playing = false};
            MusicRequests = new List<MusicRequest>();
        }


        public Server(Database.DatabaseClasses.ServerSetting setting)
        {
            ServerId = setting.ServerId;
            MusicChannel = setting.MusicChannel;
            RoleChannel = setting.RoleChannel;
            TChannelId = setting.TwitchChannelId;
            TChannelLink = setting.TwitchChannelLink;
            TNotifChannel = setting.TwitchNotificationChannel;
            TNotifRole = setting.TwitchNotificationRole;
            TwitchOnline = false;
            AudioVars = new() { Playing = false };
            MusicRequests = new List<MusicRequest>();
        }
    }
}
