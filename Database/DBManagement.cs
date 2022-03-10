using Discord_Bot.Classes;
using System;
using System.Data;
using System.Data.SQLite;

namespace Discord_Bot.Database
{
    class DBManagement
    {
        protected static readonly SQLiteConnection Sqlite_conn = new($"Data Source=database.db; Version = 3; New = True; Compress = True; ");

        public static void Insert(string query)
        {
            Sqlite_conn.Open();

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;
                sqlite_cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Insert", ex.ToString()));
            }

            Sqlite_conn.Close();
        }

        public static void Update(string query)
        {
            Sqlite_conn.Open();

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Update", ex.ToString()));
            }

            Sqlite_conn.Close();
        }

        public static DataTable Read(string query)
        {
            DataTable results = null;

            Sqlite_conn.Open();

            try
            {
                SQLiteCommand sqlite_cmd;
                SQLiteDataReader sqlite_datareader;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;

                sqlite_datareader = sqlite_cmd.ExecuteReader();

                if (sqlite_datareader.HasRows) 
                {
                    results = new DataTable();
                    results.Load(sqlite_datareader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Read", ex.ToString()));
            }
            
            Sqlite_conn.Close();

            return results;
        }

        public static void Delete(string query)
        {
            SQLiteCommand sqlite_cmd;

            Sqlite_conn.Open();

            try
            {
                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Delete", ex.ToString()));
            }

            Sqlite_conn.Close();
        }
    }
}
