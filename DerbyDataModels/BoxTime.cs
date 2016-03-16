namespace DerbyDataModels
{
    public class BoxTime
    {
        public int BoxTimeID { get; set; }
        public int PlayerID { get; set; }
        public int JamID { get; set; }
        public bool IsJammer { get; set; }
        public bool? StartedJamInBox { get; set; }
        public bool EndedJamInBox { get; set; }
        public bool Finished { get; set; }
        //public double BoxSeconds { get; set; }
    }
}