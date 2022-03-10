using Discord_Bot.Classes;
using Discord_Bot.Database;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Discord_Bot
{
    internal class StartupFunctions :DBManagement
    {
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



        //A list of the current servers
        public static void ServerList()
        {
            var table = DBManagement.Read($"SELECT * FROM `serversetting`");

            if (table != null)
            {
                foreach (System.Data.DataRow server in table.Rows)
                {
                    Global.servers.Add(new ServerSetting(server));
                }
            }
        }



        //Check if database exists
        //Currently copies sql files, later, it should copy a backup, and only use database.sql if there are no other options
        public static void DBCheck()
        {
            try
            {
                if (!File.Exists("database.db"))
                {
                    Sqlite_conn.Open();

                    SQLiteCommand sqlite_cmd;

                    sqlite_cmd = Sqlite_conn.CreateCommand();

                    sqlite_cmd.CommandText = File.ReadAllText("database.sql");
                    sqlite_cmd.ExecuteNonQuery();

                    sqlite_cmd.CommandText = File.ReadAllText("database insert.sql");
                    sqlite_cmd.ExecuteNonQuery();
                }

                Console.WriteLine("Database integrity checked!");
                Global.Logs.Add(new Log("LOG", "Database integrity checked!"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Startup", ex.ToString()));
            }

            if (Sqlite_conn.State == ConnectionState.Open) Sqlite_conn.Close();
        }
    }
}
