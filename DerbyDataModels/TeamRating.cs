using System;

namespace DerbyDataModels
{
    public class TeamRating
    {
        public int TeamID { get; set; }
        public string TeamName { get; set; }
        public string LeagueName { get; set; }
        public int WftdaRank { get; set; }
        public double WftdaStrength { get; set; }
        public double WftdaScore { get; set; }
        public int FtsRank { get; set; }
        public double FtsScore { get; set; }
        public DateTime AddedDate { get; set; }
    }
}