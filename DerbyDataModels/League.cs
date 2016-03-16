using System;

namespace DerbyDataModels
{
    public interface ILeague
    {
        int ID { get; set; }
        string Name { get; set; }
        DateTime JoinDate { get; set; }
    }

    public class League
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
