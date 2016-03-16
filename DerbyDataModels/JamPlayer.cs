namespace DerbyDataModels
{
    public class JamPlayer
    {
        public int ID { get; set; }
        public int PlayerID { get; set; }
        public int TeamID { get; set; }
        public int JamID { get; set; }
        public bool IsJammer { get; set; }
        public bool IsPivot { get; set; }
    }
}
