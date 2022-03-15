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

        public MusicVariables AudioVars { get; set; }

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
            AudioVars = new() { Playing = false, JoinedVoice = false };
            MusicRequests = new List<MusicRequest>();
        }
        

        public Server(System.Data.DataRow row)
        {
            if (row != null)
            {
                ServerId = ulong.Parse(row[0].ToString());
                MusicChannel = ulong.Parse(row[1].ToString());
                RoleChannel = ulong.Parse(row[2].ToString());
                TChannelId = row[3].ToString();
                TChannelLink = row[4].ToString();
                TNotifChannel = ulong.Parse(row[5].ToString());
                TNotifRole = ulong.Parse(row[6].ToString());
                TwitchOnline = false;
                AudioVars = new() { Playing = false, JoinedVoice = false };
                MusicRequests = new List<MusicRequest>();
            }
        }
    }
}
