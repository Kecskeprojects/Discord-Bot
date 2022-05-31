using System.Data;

namespace Discord_Bot.Modules.Database.DatabaseClasses
{
    public class UserBias
    {
        public ulong UserId { get; set; }
        public int BiasId { get; set; }

        public UserBias() { }

        public UserBias(DataRow row)
        {
            UserId = ulong.Parse(row[0].ToString());
            BiasId = int.Parse(row[1].ToString());
        }
    }
}
