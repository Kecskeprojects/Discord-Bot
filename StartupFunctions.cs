using Discord_Bot.Modules;
using Discord_Bot.Modules.ListClasses;
using Discord_Bot.Modules.Database;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Discord_Bot
{
    internal class StartupFunctions : DBFunctions
    {
        //Testing connection by pinging google, it is quite a problem if that's down too
        public static bool Connection()
        {
            try
            {
                if (new System.Net.NetworkInformation.Ping()
                    .Send("google.com", 1000, new byte[32], new System.Net.NetworkInformation.PingOptions()).Status ==
                    System.Net.NetworkInformation.IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }



        //Check if folders for long term storage exist
        public static void Check_Folders()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Assets"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Assets");
                Console.WriteLine("Assets folder created!");
                Global.Logs.Add(new Log("LOG", "Assets folder was created!"));
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Assets\\Commands"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Assets\\Commands");
                Console.WriteLine("Commands folder created!");
                Global.Logs.Add(new Log("LOG", "Commands folder was created!"));
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Assets\\Data"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Assets\\Data");
                Console.WriteLine("Data folder created!");
                Global.Logs.Add(new Log("LOG", "Data folder was created!"));
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Logs"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Logs");
                Console.WriteLine("Logs folder created!");
                Global.Logs.Add(new Log("LOG", "Logs folder was created!"));
            }
        }



        //Setting the list of servers we currently have in the database
        public static void ServerList()
        {
            var list = AllServerSetting();

            if (list.Count > 0)
            {
                foreach (var server in list)
                {
                    Global.servers.Add(server.ServerId, new Server(server));
                }
            }
        }



        //Check if database exists
        //Try copying a backup if any are available, otherwise make a new db file using the provided sql file
        public static void DBCheck()
        {
            try
            {
                if (!File.Exists("database.db"))
                {
                    string[] array = Directory.GetFiles("Assets\\Data");
                    if (array.Length > 0)
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "\\" + array.Last(), "database.db");
                        Console.WriteLine("Newest backup copied!");
                        Global.Logs.Add(new Log("LOG", "Newest backup copied!"));
                    }
                    else
                    {
                        Sqlite_conn.Open();

                        SQLiteCommand sqlite_cmd;

                        sqlite_cmd = Sqlite_conn.CreateCommand();

                        sqlite_cmd.CommandText = File.ReadAllText("database.sql");
                        sqlite_cmd.ExecuteNonQuery();

                        Console.WriteLine("No backups available, new db file made from sql file!");
                        Global.Logs.Add(new Log("LOG", "No backups available, new db file made from sql file!"));

                        Sqlite_conn.Close();
                    }
                }

                Sqlite_conn.Open();

                Sqlite_conn.StateChange += new StateChangeEventHandler(OpenConnection);

                Console.WriteLine("Database integrity checked!");
                Global.Logs.Add(new Log("LOG", "Database integrity checked!"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "StartupFunctions.cs Startup", ex.ToString()));
            }
        }



        //Things to do when app is closing
        //3 second time limit to event by default
        public static void Closing(object sender, EventArgs e)
        {
            Global.Logs.Add(new Log("LOG", "Application closing..."));
            ProgramFunctions.LogToFile();
        }
    }
}
