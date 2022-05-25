using System;
using System.Data;

namespace Discord_Bot.Modules.Database
{
    public class DBFunctions: DBManagement
    {
        //
        //GREETING database commands
        //

        public static DataTable AllGreeting()
        {
            return Read("SELECT * FROM `greeting`;");
        }

        public static int GreetingAdd(int id, string url)
        {
            return Insert($"INSERT INTO `greeting` (`id`, `url`) VALUES ('{id}','{url}')");
        }

        public static int GreetingRemove(int id)
        {
            return Delete($"DELETE FROM `greeting` WHERE `id` = '{id}';");
        }


        //
        //SERVERSETTING database commands
        //

        public static DataTable AllServerSetting()
        {
            return Read("SELECT * FROM `serversetting`;");
        }


        public static int AddNewServer(ulong serverId)
        {
            return Insert($"INSERT INTO `serversetting`(`serverId`, `musicChannel`, `roleChannel`, `tNotifChannel`, `tNotifRole`) VALUES ('{serverId}','0','0','0','0');");
        }


        public static int ServerSettingUpdate(string channelType, ulong channelId, ulong serverId)
        {
            return Update($"UPDATE `serversetting` SET `{channelType}` = '{channelId}' WHERE `serverId` = '{serverId}';");
        }

        public static int ServerSettingTwitchUpdate(string ChannelId, string ChannelURL, ulong serverId)
        {
            return Update($"UPDATE `serversetting` SET `tChannelId` = '{ChannelId}', `tChannelLink` = '{ChannelURL}' WHERE `serverId` = '{serverId}';");
        }


        //
        //CUSTOMCOMMAND database commands
        //

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


        //
        //SELFROLE database commands
        //

        public static DataRow SelfRoleGet(ulong serverId, string roleName)
        {
            var table = Read($"SELECT * FROM `role` WHERE `roleName` = '{roleName}' AND `serverId` = '{serverId}'");

            if (table.Rows.Count > 0) { return table.Rows[0]; }
            else return null;
        }


        public static int SelfRoleAdd(ulong serverId, string roleName, ulong roleId)
        {
            return Insert($"INSERT INTO `role`(`serverId`, `roleName`, `roleId`) VALUES ('{serverId}','{roleName}','{roleId}');");
        }


        public static int SelfRoleRemove(ulong serverId, ulong roleName)
        {
            return Delete($"DELETE FROM `role` WHERE `roleName` = '{roleName}' AND `serverId` = '{serverId}';");
        }


        //
        //KEYWORD database commands
        //

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


        //
        //LASTFM database commands
        //

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


        //
        //BIASLIST database commands
        //

        public static DataTable BiasList(string biasGroup = "")
        {
            //If a group is searched for, add a search for it
            if (biasGroup != "") biasGroup = $"WHERE `biasGroup` = '{biasGroup}'";

            var table = Read($"SELECT * FROM `bias` {biasGroup};");

            if (table.Rows.Count > 0) { return table; }
            else return null;
        }


        public static DataRow BiasByName(string biasName)
        {
            var table = Read($"SELECT `biasId` FROM `bias` WHERE `biasName` = '{biasName}';");

            if (table.Rows.Count > 0) { return table.Rows[0]; }
            else return null;
        }

        public static DataRow BiasByNameForEach(string[] biasNames)
        {
            //Stringing together all the names we are searching for
            string checked_names = "";
            foreach (var item in biasNames)
            {
                if (checked_names != "") checked_names += " OR ";

                checked_names += $"`biasName` = '{item}'";
            }

            var table = Read($"SELECT `biasId` FROM `bias` WHERE {checked_names};");

            if (table.Rows.Count > 0) { return table.Rows[0]; }
            else return null;
        }


        public static int BiasAdd(int biasId, string biasName, string biasGroup)
        {
            return Insert($"INSERT INTO `bias` (`biasId`,`biasName`,`biasGroup`) VALUES ('{biasId}','{biasName}','{biasGroup}');");
        }


        public static int BiasRemove(string biasName)
        {
            return Delete($"DELETE FROM `userbias` WHERE `biasId`=(SELECT `biasId` FROM `bias` WHERE `biasName`='{biasName}'); DELETE FROM `bias` WHERE `biasName` = '{biasName}';");
        }


        //
        //USERBIAS database commands
        //

        public static DataTable UserBiasesList(ulong userId, string biasGroup)
        {
            //If a group is searched for, add a search for it
            if (biasGroup != "") biasGroup = $"AND `biasGroup` = '{biasGroup}'";

            var table = Read($"SELECT `bias`.`biasName`, `bias`.`biasGroup` AS 'biasName' FROM `userbias` INNER JOIN `bias` ON `userbias`.`biasId` = `bias`.`biasId` WHERE `userbias`.`userId` = '{userId}' {biasGroup};");

            if (table.Rows.Count > 0) { return table; }
            else return null;
        }


        public static DataTable UserBiasCheck(ulong userId, string biasName)
        {
            var table = Read($"SELECT `bias`.`biasName` AS 'biasName' FROM `userbias` INNER JOIN `bias` ON `userbias`.`biasId` = `bias`.`biasId` WHERE `userbias`.`userId` = '{userId}' AND `bias`.`biasname` = '{biasName}';");

            if (table.Rows.Count > 0) { return table; }
            else return null;
        }


        public static DataTable UsersWithBiasList(string[] biasNames)
        {
            //Stringing together all the names we are searching for
            string checked_names = "";
            foreach (var item in biasNames)
            {
                if (checked_names != "") checked_names += " OR ";

                checked_names += $"`bias`.`biasName` = '{item}'";
            }

            var table = Read($"SELECT `userId` FROM `userbias` INNER JOIN `bias` ON `userbias`.`biasId` = `bias`.`biasId` WHERE {checked_names};");

            if (table.Rows.Count > 0) { return table; }
            else return null;
        }


        public static int UserBiasAdd(ulong userId, int biasId)
        {
            return Insert($"INSERT INTO `userbias` (`userId`,`biasId`) VALUES ('{userId}','{biasId}');");
        }

        public static int UserBiasRemove(int biasId, ulong userId)
        {
            return Delete($"DELETE FROM `userbias` WHERE `biasId` = '{biasId}' AND `userId` = '{userId}';");
        }


        public static int UserBiasClear(ulong userId)
        {
            return Delete($"DELETE FROM `userbias` WHERE `userId` = '{userId}';");
        }


        //
        //MANUAL DB FUNCTION
        //

        public static Tuple<int, DataTable, string> ManualDBManagement(string query)
        {
            return Manual(query);
        }
    }
}
