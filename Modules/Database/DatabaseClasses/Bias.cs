using System.Data;

namespace Discord_Bot.Modules.Database.DatabaseClasses
{
    public class Bias
    {
        public int Id { get; set; }
        public string BiasName { get; set; }
        public string BiasGroup { get; set; }

        public Bias() { }

        public Bias(DataRow row)
        {
            Id = int.Parse(row[0].ToString());
            BiasName = row[1].ToString();
            BiasGroup = row[2].ToString();
        }
    }
}
