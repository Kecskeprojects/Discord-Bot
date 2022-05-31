using System.Data;

namespace Discord_Bot.Modules.Database.DatabaseClasses
{
    public class CustomCommand
    {
        public ulong ServerId { get; set; }
        public string Command { get; set; }
        public string URL { get; set; }

        public CustomCommand() { }

        public CustomCommand(DataRow row)
        {
            ServerId = ulong.Parse(row[0].ToString());
            Command = row[1].ToString();
            URL = row[2].ToString();
        }
    }
}
