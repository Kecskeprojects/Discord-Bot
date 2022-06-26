using System;
using System.Collections.Generic;
using System.Data;
using Discord_Bot.Modules.Database.DatabaseClasses;
using Discord_Bot.Modules.ListClasses;

namespace Discord_Bot.Modules.Database
{
    public class DBFunctions: DBManagement
    {
        //
        //GREETING database commands
        //

        public static List<Greeting> AllGreeting()
        {
            DataTable table = Read("SELECT `id`, `url` FROM `greeting`;");

            List<Greeting> greetings = new();

            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    greetings.Add(new Greeting(dr));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
                greetings = new();
            }

            return greetings;
        }


        public static int GreetingAdd(int id, string url)
        {
            return Insert($"INSERT INTO `greeting` (`id`, `url`) VALUES ('{id}','{url}');");
        }


        public static int GreetingRemove(int id)
        {
            return Delete($"DELETE FROM `greeting` WHERE `id` = '{id}';");
        }


        //
        //SERVERSETTING database commands
        //

        public static List<ServerSetting> AllServerSetting()
        {
            var table = Read("SELECT `serverId`, `musicChannel`, `roleChannel`, `tChannelId`, `tChannelLink`, `tNotifChannel`, `tNotifRole` FROM `serversetting`;");

            List<ServerSetting> serverSettings = new();

            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    serverSettings.Add(new ServerSetting(dr));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
                serverSettings = new();
            }

            return serverSettings;
        }


        public static int AddNewServer(ulong serverId)
        {
            return Insert($"INSERT INTO `serversetting`(`serverId`, `musicChannel`, `roleChannel`, `tChannelId`, `tChannelLink`, `tNotifChannel`, `tNotifRole`) VALUES ('{serverId}','0','0','','','0','0');");
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

        public static List<CustomCommand> AllCustomCommand(ulong serverId)
        {
            var table = Read($"SELECT `serverId`, `command`, `url` FROM `customcommand` WHERE `serverId` = '{serverId}';");

            List<CustomCommand> customCommand = new();

            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    customCommand.Add(new CustomCommand(dr));
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message); 
                Global.Logs.Add(new Log("ERROR", ex.ToString())); 
                customCommand = new(); 
            }

            return customCommand;
        }


        public static CustomCommand CustomCommandGet(ulong serverId, string name)
        {
            var table = Read($"SELECT `serverId`, `command`, `url` FROM `customcommand` WHERE `command` = '{name}' AND `serverId` = '{serverId}' LIMIT 1;");

            CustomCommand command = null;

            try
            {
                if (table.Rows.Count > 0)
                {
                    command = new CustomCommand(table.Rows[0]);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
            }

            return command;
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

        public static Role SelfRoleGet(ulong serverId, string roleName)
        {
            var table = Read($"SELECT `serverId`, `roleName`, `roleId` FROM `role` WHERE `roleName` = '{roleName}' AND `serverId` = '{serverId}' LIMIT 1;");

            Role role = null;

            try
            {
                if (table.Rows.Count > 0)
                {
                    role = new Role(table.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
            }

            
            return role;
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

        public static Keyword KeywordGet(ulong serverId, string trigger)
        {
            var table = Read($"SELECT `serverId`, `trigger`, `response` FROM `keyword` WHERE `trigger` = '{trigger}' AND `serverId` = '{serverId}' LIMIT 1;");

            Keyword keyword = null;

            try
            {
                if (table.Rows.Count > 0)
                {
                    keyword = new Keyword(table.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
            }

            return keyword;
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

        public static List<LastFmUser> LastfmUsers()
        {
            var table = Read($"SELECT `userId`, `username` FROM `lastfm`;");

            List<LastFmUser> lastFmUsers = new();

            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    lastFmUsers.Add(new LastFmUser(dr));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
                lastFmUsers = new();
            }

            return lastFmUsers;
        }

        public static LastFmUser LastfmUserGet(ulong userId)
        {
            var table = Read($"SELECT `userId`, `username` FROM `lastfm` WHERE `userId` = '{userId}' LIMIT 1;");

            LastFmUser lastfmuser = null;

            try
            {
                if (table.Rows.Count > 0)
                {
                    lastfmuser = new LastFmUser(table.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
            }

            return lastfmuser;
        }

        public static int LastfmUserAdd(ulong userId, string username)
        {
            return Insert($"INSERT INTO `lastfm` (`userId`,`username`) VALUES ('{userId}','{username}');");
        }


        public static int LastfmUserRemove(ulong userId)
        {
            return Delete($"DELETE FROM `lastfm` WHERE `userId` = '{userId}';");
        }


        //
        //BIASLIST database commands
        //

        public static List<Bias> BiasList(string biasGroup = "")
        {
            //If a group is searched for, add a search for it
            if (biasGroup != "") biasGroup = $"WHERE `biasGroup` = '{biasGroup}'";

            var table = Read($"SELECT `biasId`, `biasName`, `biasGroup` FROM `bias` {biasGroup};");

            List<Bias> biases = new();

            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    biases.Add(new Bias(dr));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
                biases = new();
            }

            return biases;
        }


        public static Bias BiasSearch(string biasName, string biasGroup = "")
        {
            if(biasGroup != "")
            {
                biasGroup = $"AND `biasGroup` = '{biasGroup}'";
            }

            var table = Read($"SELECT `biasId`, `biasName`, `biasGroup` FROM `bias` WHERE `biasName` = '{biasName}' {biasGroup} LIMIT 1;");

            Bias bias = null;

            try
            {
                if (table.Rows.Count > 0)
                {
                    bias = new Bias(table.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
            }

            return bias;
        }

        public static Bias BiasByNameForEach(string[] biasNames)
        {
            //Stringing together all the names we are searching for
            string checked_names = "";
            foreach (var item in biasNames)
            {
                if (checked_names != "") checked_names += " OR ";

                checked_names += $"`biasName` = '{item}'";
            }

            var table = Read($"SELECT `biasId`, `biasName`, `biasGroup` FROM `bias` WHERE {checked_names} LIMIT 1;");

            Bias bias = null;

            try
            {
                if (table.Rows.Count > 0)
                {
                    bias = new Bias(table.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
            }

            return bias;
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

        public static List<Bias> UserBiasesList(ulong userId, string biasGroup = "")
        {
            //If a group is searched for, add a constraint for it in the sql query
            if (biasGroup != "") biasGroup = $"AND `bias`.`biasGroup` = '{biasGroup}'";

            var table = Read("SELECT `bias`.`biasId` AS 'biasId', `bias`.`biasName` AS 'biasName', `bias`.`biasGroup` AS 'biasGroup' FROM `userbias` " +
                "INNER JOIN `bias` ON `userbias`.`biasId` = `bias`.`biasId` " +
                $"WHERE `userbias`.`userId` = '{userId}' {biasGroup};");

            List<Bias> userbiases = new();

            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    userbiases.Add(new Bias(dr));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
                userbiases = new();
            }

            return userbiases;
        }


        public static Bias UserBiasCheck(ulong userId, string biasName, string biasGroup = "")
        {
            if (biasGroup != "")
            {
                biasGroup = $"AND `bias`.`biasGroup` = '{biasGroup}'";
            }

            var table = Read($"SELECT `bias`.`biasId` AS 'biasId', `bias`.`biasName` AS 'biasName', `bias`.`biasGroup` AS 'biasGroup' FROM `userbias` " +
                "INNER JOIN `bias` ON `userbias`.`biasId` = `bias`.`biasId` " +
                $"WHERE `userbias`.`userId` = '{userId}' AND `bias`.`biasname` = '{biasName}' {biasGroup} LIMIT 1;");

            Bias bias = null;

            try
            {
                if (table.Rows.Count > 0)
                {
                    bias = new Bias(table.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
            }

            return bias;
        }


        public static List<ulong> UsersWithBiasList(string[] biasNames)
        {
            //Stringing together all the names we are searching for
            string checked_names = "";
            foreach (var item in biasNames)
            {
                if (checked_names != "") checked_names += " OR ";

                checked_names += $"`bias`.`biasName` = '{item}'";
            }

            var table = Read($"SELECT DISTINCT `userId` FROM `userbias` " +
                "INNER JOIN `bias` ON `userbias`.`biasId` = `bias`.`biasId` " +
                $"WHERE { checked_names};");

            List<ulong> users = new();

            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    users.Add(ulong.Parse(dr[0].ToString()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
                users = new();
            }

            return users;
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
        //REMINDER database commands
        //

        public static List<Reminder> ReminderList(string date)
        {
            var table = Read($"SELECT `userId`, `date`, `message` FROM `reminder` WHERE `date` <= '{date}';");

            List<Reminder> reminders = new();

            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    reminders.Add(new Reminder(dr));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
                reminders = new();
            }

            return reminders;
        }


        public static List<Reminder> UserReminders(ulong userId)
        {
            var table = Read($"SELECT `userId`, `date`, `message` FROM `reminder` WHERE `userId` = '{userId}' ORDER BY `date` ASC;");

            List<Reminder> reminders = new();

            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    reminders.Add(new Reminder(dr));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Global.Logs.Add(new Log("ERROR", ex.ToString()));
                reminders = new();
            }

            return reminders;
        }


        public static int ReminderAdd(ulong userId, string date, string message)
        {
            return Insert($"INSERT INTO `reminder` (`userId`,`date`,`message`) VALUES ('{userId}','{date}','{message}');");
        }

        public static int ReminderRemove(ulong userId, string date)
        {
            return Delete($"DELETE FROM `reminder` WHERE `date` = '{date}' AND `userId` = '{userId}';");
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
