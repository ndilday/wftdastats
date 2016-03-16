using System;
using System.Collections.Generic;

namespace StatbookReader.Models
{
    public class PenaltyModel
    {
        public bool IsFirstHalf { get; set; }
        public int JamNumber { get; set; }
        public string PenaltyCode { get; set; }
    }

    public class PlayerPenaltiesModel
    {
        public string PlayerNumber { get; set; }
        public IList<PenaltyModel> Penalties { get; set; }
    }

    public class PenaltiesModel
    {
        public IList<PlayerPenaltiesModel> HomePlayerPenalties { get; set; }
        public IList<PlayerPenaltiesModel> AwayPlayerPenalties { get; set; }
    }
}
