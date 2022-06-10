using System;
using System.Data;
namespace Discord_Bot.Modules.Database.DatabaseClasses
{
    public class Reminder
    {
        public ulong UserId { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }

        public Reminder() { }

        public Reminder(DataRow row)
        {
            UserId = ulong.Parse(row[0].ToString());
            Date = DateTime.Parse(row[1].ToString());
            Message = row[2].ToString();
        }
    }
}
