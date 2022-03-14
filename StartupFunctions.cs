using Discord_Bot.Modules;
using Discord_Bot.Modules.ListClasses;
using Discord_Bot.Modules.Database;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Timers;
using System.Threading.Tasks;
using System.Linq;
using Discord_Bot.Modules.API;

namespace Discord_Bot
{
    internal class StartupFunctions : DBFunctions
    {
        //Testing connection by pinging google
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



        //A list of the current servers
        public static void ServerList()
        {
            var table = AllServerSetting();

            if (table.Rows.Count > 0)
            {
                foreach (DataRow server in table.Rows)
                {
                    Global.servers.Add(ulong.Parse(server[0].ToString()), new ServerSetting(server));
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

                    Sqlite_conn.Close();
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
                Global.Logs.Add(new Log("ERROR", "Management.cs Startup", ex.ToString()));
            }
        }



        //What the bot does every minute
        static int minutes_count = 0;
        public static async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (minutes_count == 1440) minutes_count = 0;

            if (minutes_count == 0) DatabaseBackup();

            if (DateTime.Now.Hour == 10 && DateTime.Now.Minute == 0) YoutubeAPI.KeyReset();

            Log_to_file();

            minutes_count++;

            await Task.CompletedTask;
        }



        //Things to do when app is closing
        //3 second time limit to event by default
        public static void Closing(object sender, EventArgs e)
        {
            Global.Logs.Add(new Log("LOG", "Application closing..."));
            Log_to_file();
        }



        //For logging messages, errors, and messages to log files
        static StreamWriter LogFile_writer = null;
        public static void Log_to_file()
        {
            try
            {
                if (Global.Logs.Count != 0 && LogFile_writer == null)
                {
                    string file_location = "Logs\\logs" + "[" + DateTime.Now.Year + "-" + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString()) + "-" + (DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString()) + "].txt";

                    using (LogFile_writer = File.AppendText(file_location)) foreach (string log in Global.Logs.Select(n => n.Content)) LogFile_writer.WriteLine(log);

                    LogFile_writer = null;
                    Global.Logs.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Log_to_File", ex.ToString()));
            }
        }



        //Copy database to Assets\Data folder, done once a day
        public static void DatabaseBackup()
        {
            try
            {
                File.Copy("database.db", Path.Combine(Directory.GetCurrentDirectory(),
                    $"Assets\\Data\\database_{"" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute}.db"));
                Console.WriteLine("Database backup created!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Program_Functions.cs Log OnTimedEvent", ex.ToString()));
            }
        }
    }
}
