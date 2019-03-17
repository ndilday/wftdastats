using System;

namespace DerbyDataModels
{
    public class FoulComparison
    {
        public int Year { get; set; }
        public double BlockerBoxComparison { get; set; }
        public double JammerBoxComparison { get; set; }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            FoulComparison foulComparison = obj as FoulComparison;
            if ((System.Object)foulComparison == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (BlockerBoxComparison == foulComparison.BlockerBoxComparison) &&
                   (JammerBoxComparison == foulComparison.JammerBoxComparison);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Year.GetHashCode();
            hash = hash * 23 + JammerBoxComparison.GetHashCode();
            hash = hash * 23 + BlockerBoxComparison.GetHashCode();
            return hash;
        }
    }

    public class JamData
    {
        public int JamID { get; set; }
        public DateTime PlayDate { get; set; }
        public int HomeTeamType { get; set; }
        public int AwayTeamType { get; set; }
    }

    public class JamTeamData
    {
        public int Year { get; set; }
        public int JamID { get; set; }
        public int TeamID { get; set; }
        public int PointDelta { get; set; }
        public int JammerBoxTime { get; set; }
        public int BlockerBoxTime { get; set; }
        public int OppJammerBoxTime { get; set; }
        public int OppBlockerBoxTime { get; set; }

        public FoulComparison FoulComparison
        {
            get
            {
                int jammerPenaltyDiff = JammerBoxTime - OppJammerBoxTime;
                int blockerPenaltyDiff = BlockerBoxTime - OppBlockerBoxTime;
                double jammerBoxComp = Math.Round(jammerPenaltyDiff / 15.0, MidpointRounding.AwayFromZero) / 2.0;
                double blockerBoxComp = Math.Round(blockerPenaltyDiff / 15.0, MidpointRounding.AwayFromZero) / 2.0;
                
                return new FoulComparison
                {
                    JammerBoxComparison = jammerBoxComp,
                    BlockerBoxComparison = blockerBoxComp,
                    Year = this.Year
                };
            }
        }
    }
}
