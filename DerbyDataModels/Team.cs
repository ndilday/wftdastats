namespace DerbyDataModels
{
    public class Team
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int LeagueID { get; set; }
        public string TeamType { get; set; }
        public int? RinxterID { get; set; }
    }
}
