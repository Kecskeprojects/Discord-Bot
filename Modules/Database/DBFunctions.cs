using System;
using System.Data;

namespace Discord_Bot.Modules.Database
{
    public class DBFunctions: DBManagement
    {
        public static DataTable AllGreeting()
        {
            return Read("SELECT * FROM `greeting`;");
        }

        public static int GreetingAdd(int id, string url)
        {
            return Insert($"INSERT INTO `greeting` (`id`, `url`), ('{id}','{url}')");
        }

        public static int GreetingRemove(int id)
        {
            return Delete($"DELETE FROM `greeting` WHERE `id` = '{id}';");
        }


        public static DataTable AllServerSetting()
        {
            return Read("SELECT * FROM `serversetting`;");
        }


        public static int AddNewServer(ulong serverId)
        {
            return Insert($"INSERT INTO `serversetting`(`serverId`, `musicChannel`, `roleChannel`, `tNotifChannel`, `tNotifRole`) VALUES ('{serverId}','0','0','0','0');");
        }


        public static int ServerSettingUpdate(string channelType, ulong channelId)
        {
            return Update($"UPDATE `serversetting` SET `{channelType}` = '{channelId}';");
        }


        public static DataTable AllCustomCommand(ulong serverId)
        {
            return Read($"SELECT * FROM `customcommand` WHERE `serverId` = '{serverId}';");
        }


        public static DataRow CustomCommandGet(ulong serverId, string name)
        {
            var table = Read($"SELECT * FROM `customcommand` WHERE `command` = '{name}' AND `serverId` = '{serverId}';");

            if (table.Rows.Count > 0) { return table.Rows[0]; }
            else return null;
        }


        public static int CustomCommandAdd(ulong serverId, string name, string link)
        {
            return Insert($"INSERT INTO `customcommand`(`serverId`, `command`, `url`) VALUES ('{serverId}','{name}','{link}');");
        }


        public static int CustomCommandRemove(ulong serverId, string name)
        {
            return Delete($"DELETE FROM `customcommand` WHERE `command` = '{name}' AND `serverId` = '{serverId}';");
        }


        public static DataRow SelfRoleGet(ulong serverId, string name)
        {
            var table = Read($"SELECT * FROM `customcommand` WHERE `command` = '{name}' AND `serverId` = '{serverId}'");

            if (table.Rows.Count > 0) { return table.Rows[0]; }
            else return null;
        }


        public static int SelfRoleAdd(ulong serverId, string roleName, ulong roleId)
        {
            return Insert($"INSERT INTO `role`(`serverId`, `roleName`, `roleId`) VALUES ('{serverId}','{roleName}','{roleId}');");
        }


        public static int SelfRoleRemove(ulong serverId, ulong roleName)
        {
            return Delete($"DELETE FROM `customcommand` WHERE `command` = '{roleName}' AND `serverId` = '{serverId}';");
        }


        public static DataRow KeywordGet(ulong serverId, string trigger)
        {
            var table = Read($"SELECT * FROM `keyword` WHERE `trigger` = '{trigger}' AND `serverId` = '{serverId}';");

            if (table.Rows.Count > 0) { return table.Rows[0]; }
            else return null;
        }


        public static int KeywordAdd(ulong serverId, string trigger, string response)
        {
            return Insert($"INSERT INTO `keyword` (`serverId`,`trigger`, `response`) VALUES ('{serverId}','{trigger}','{response}');");
        }


        public static int KeywordRemove(ulong serverId, string trigger)
        {
            return Delete($"DELETE FROM `keyword` WHERE `serverId` = '{serverId}' AND `trigger` = '{trigger}';");
        }


        public static DataRow LastfmGet(ulong userId)
        {
            var table = Read($"SELECT * FROM `lastfm` WHERE `userId` = '{userId}';");

            if (table.Rows.Count > 0) { return table.Rows[0]; }
            else return null;
        }

        public static int LastfmAdd(ulong userId, string username)
        {
            return Insert($"INSERT INTO `lastfm` (`userId`,`username`) VALUES ('{userId}','{username}');");
        }


        public static int LastfmRemove(ulong userId)
        {
            return Delete($"DELETE FROM `lastfm` WHERE `userId` = '{userId}';");
        }


        public static Tuple<int, DataTable, string> ManualDBManagement(string query)
        {
            return Manual(query);
        }
    }
}
