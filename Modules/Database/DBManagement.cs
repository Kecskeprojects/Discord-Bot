﻿using Discord_Bot.Modules.ListClasses;
using System;
using System.Data;
using System.Data.SQLite;

namespace Discord_Bot.Modules.Database
{
    class DBManagement
    {
        protected static readonly SQLiteConnection Sqlite_conn = new($"Data Source=database.db; Version = 3; New = True; Compress = True; ");

        public static int Insert(string query)
        {
            int affected_rows =  -1;

            Sqlite_conn.Open();

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;
                affected_rows = sqlite_cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Insert", ex.ToString()));
            }

            Sqlite_conn.Close();

            return affected_rows;
        }

        public static int Update(string query)
        {
            int affected_rows = -1;

            Sqlite_conn.Open();

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;
                affected_rows = sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Update", ex.ToString()));
            }

            Sqlite_conn.Close();

            return affected_rows;
        }

        public static DataTable Read(string query)
        {
            DataTable results = null;

            Sqlite_conn.Open();

            try
            {
                SQLiteCommand sqlite_cmd;
                SQLiteDataReader reader;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;

                results = new();
                results.Load(reader = sqlite_cmd.ExecuteReader());
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

        public static int Delete(string query)
        {
            int affected_rows = -1;

            Sqlite_conn.Open();

            try
            {
                SQLiteCommand sqlite_cmd;

                sqlite_cmd = Sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = query;
                affected_rows = sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!\n" + ex.ToString());
                Global.Logs.Add(new Log("DEV", ex.Message));
                Global.Logs.Add(new Log("ERROR", "Management.cs Delete", ex.ToString()));
            }

            Sqlite_conn.Close();

            return affected_rows;
        }
    }
}