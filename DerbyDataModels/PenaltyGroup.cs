using System.Collections.Generic;

namespace DerbyDataModels
{
    public class PenaltyGroup
    {
        public int PlayerID { get; set; }
        public List<Penalty> Penalties { get; set; }
        public List<BoxTime> BoxTimes { get; set; }
        public int GroupID { get; set; }
    }
}
