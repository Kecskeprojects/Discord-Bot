using System.Data;

namespace Discord_Bot.Modules.Database.DatabaseClasses
{
    public class Greeting
    {
        public int Id { get; set; }
        public string URL { get; set; }

        public Greeting() { }

        public Greeting(DataRow row)
        {
            Id = int.Parse(row[0].ToString());
            URL = row[1].ToString();
        }
    }
}
