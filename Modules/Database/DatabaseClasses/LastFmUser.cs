using System.Data;

namespace Discord_Bot.Modules.Database.DatabaseClasses
{
    public class LastFmUser
    {
        public ulong UserId { get; set; }
        public string Username { get; set; }

        public LastFmUser() { }

        public LastFmUser(DataRow row)
        {
            UserId = ulong.Parse(row[0].ToString());
            Username = row[1].ToString();
        }
    }
}
