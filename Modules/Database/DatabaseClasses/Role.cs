using System.Data;

namespace Discord_Bot.Modules.Database.DatabaseClasses
{
    public class Role
    {
        public ulong ServerId { get; set; }
        public string RoleName { get; set; }
        public ulong RoleId { get; set; }

        public Role() { }

        public Role(DataRow row)
        {
            ServerId = ulong.Parse(row[0].ToString());
            RoleName = row[1].ToString();
            RoleId = ulong.Parse(row[2].ToString());
        }
    }
}
