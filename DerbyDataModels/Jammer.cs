namespace DerbyDataModels
{
    public class Jammer
    {
        public int ID { get; set; }
        public int JamID { get; set; }
        public int PlayerID { get; set; }
        public int TeamID { get; set; }
        public bool LostLead { get; set; }
        public bool Lead { get; set; }
        public bool Called { get; set; }
        public bool Injury { get; set; }
        public bool NoPass { get; set; }
        public int Score { get; set; }
        public bool PassedStar { get; set; }
        public bool ReceivedStar { get; set; }
    }
}
