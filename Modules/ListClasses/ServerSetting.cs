﻿namespace Discord_Bot.Modules.ListClasses
{
    public class ServerSetting
    {
        public ulong? ServerId { get; set; }
        public ulong MusicChannel { get; set; }
        public ulong RoleChannel { get; set; }
        public string TChannelId { get; set; }
        public string TChannelLink { get; set; }
        public ulong TNotifChannel { get; set; }
        public ulong TNotifRole { get; set; }

        public ServerSetting() { }

        public ServerSetting(ulong serverId)
        {
            ServerId = serverId;
            MusicChannel = 0;
            RoleChannel = 0;
            TChannelId = "";
            TChannelLink = "";
            TNotifChannel = 0;
            TNotifRole = 0;
        }

        public ServerSetting(System.Data.DataRow row)
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
            }
        }
    }
}