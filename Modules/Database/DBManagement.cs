using Discord_Bot.Modules.ListClasses;
using System;
using System.Data;
using System.Data.SQLite;

namespace Discord_Bot.Modules.Database
{
    public class DBManagement
    {
        protected static readonly SQLiteConnection Sqlite_conn = new($"Data Source=database.db; Version = 3; New = True; Compress = True; ");

        protected static int Insert(string query)
        {
            int affected_rows =  -1;

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;
                affected_rows = sqlite_cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                Console.WriteLine(query);
                Global.Logs.Add(new Log("LOG", query));

                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Insert", ex.ToString()));
            }

            return affected_rows;
        }

        protected static int Update(string query)
        {
            int affected_rows = -1;

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;
                affected_rows = sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(query);
                Global.Logs.Add(new Log("LOG", query));

                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Update", ex.ToString()));
            }

            return affected_rows;
        }

        protected static DataTable Read(string query)
        {
            DataTable results = new();

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;

                results.Load(sqlite_cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                Console.WriteLine(query);
                Global.Logs.Add(new Log("LOG", query));

                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Read", ex.ToString()));
            }

            return results;
        }

        protected static int Delete(string query)
        {
            int affected_rows = -1;

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;
                affected_rows = sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(query);
                Global.Logs.Add(new Log("LOG", query));

                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Delete", ex.ToString()));
            }

            return affected_rows;
        }

        protected static  Tuple<int, DataTable, string> Manual(string query)
        {
            int affected_rows = -1;
            DataTable table = new();
            string Error = "";

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;

                SQLiteDataReader reader = sqlite_cmd.ExecuteReader();

                affected_rows = reader.RecordsAffected;
                table.Load(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine(query);
                Global.Logs.Add(new Log("LOG", query));

                Error = ex.ToString();
                Console.WriteLine(ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Delete", ex.ToString()));
            }

            return new Tuple<int, DataTable, string>(affected_rows, table, Error);
        }

        protected static void OpenConnection(object sender, StateChangeEventArgs e)
        {
            if (Sqlite_conn.State == ConnectionState.Closed)
            {
                Sqlite_conn.Open();

                Console.WriteLine("Database connection had to be reopened!");
                Global.Logs.Add(new Log("LOG", "Database connection had to be reopened!"));
            }
        }
    }
}
