using System;
using System.Collections.Generic;

namespace DerbyDataModels
{
    public class RolledUpPerformanceData
    {
        public double TotalPointsVersusMedian { get; set; }
        public double TotalJamPortions { get; set; }
        public int TotalPenalties { get; set; }
        public double TotalPenaltyCost { get; set; }
        public double TotalPlayerValue { get; set; }
        public double PlayerValueVersusTeamAverage { get; set; }
        public RolledUpPerformanceData()
        {
            TotalPointsVersusMedian = TotalJamPortions = TotalPenaltyCost = TotalPlayerValue = 0.0;
            TotalPenalties = 0;
        }
    }

    public class BoutPerformance
    {
        public int BoutID { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public DateTime BoutDate { get; set; }
        public List<JamPerformance> Jams { get; set; }
        public RolledUpPerformanceData BlockerPerformance { get; set; }
        public RolledUpPerformanceData JammerPerformance { get; set; }
    }

    public class JamPerformance
    {
        public int JamID { get; set; }
        public bool IsFirstHalf { get; set; }
        public int JamNumber { get; set; }
        public int PointDelta { get; set; }
        public double MedianDelta { get; set; }
        public double DeltaPercentile { get; set; }
        public double BlockerJamPercentage { get; set; }
        public double JammerJamPercentage { get; set; }
        public int JamPenalties { get; set; }
        public double PenaltyCost { get; set; }
        public double DeltaPortionVersusMedian { get; set; }
        public double PlayerValue { get; set; }
    }

    public class PlayerPerformance
    {
        public Player Player { get; set; }
        public List<BoutPerformance> Bouts { get; set; }
        public RolledUpPerformanceData BlockerPerformance { get; set; }
        public RolledUpPerformanceData JammerPerformance { get; set; }
    }
}
