using System.Data;

namespace Discord_Bot.Modules.Database.DatabaseClasses
{
    public class Keyword
    {
        public ulong ServerId { get; set; }
        public string Trigger { get; set; }
        public string Response { get; set; }

        public Keyword() { }

        public Keyword(DataRow row)
        {
            ServerId = ulong.Parse(row[0].ToString());
            Trigger = row[1].ToString();
            Response = row[2].ToString();
        }
    }
}
