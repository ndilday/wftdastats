using System.Collections.Generic;

namespace DerbyDataModels
{
    public class PenaltyService
    {
        public List<Penalty> Penalties { get; private set; }
        public List<BoxTime> BoxTimes { get; private set; }
        public PenaltyService()
        {
            Penalties = new List<Penalty>();
            BoxTimes = new List<BoxTime>();
        }
    }
}
