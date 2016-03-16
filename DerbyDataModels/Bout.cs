using System;

namespace DerbyDataModels
{
    public class Bout
    {
        public int ID { get; set; }
        public int HomeTeamID { get; set; }
        public int AwayTeamID { get; set; }
        public DateTime BoutDate { get; set; }
        public int? RinxterID { get; set; }
    }
}
