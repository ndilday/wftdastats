using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatbookReader.Models
{
    public class BoxTimeModel
    {
        public bool? Started { get; set; }
        public bool Exited { get; set; }
        public bool IsJammer { get; set; }
    }

    public class PlayerLineupModel
    {
        public string PlayerNumber { get; set; }
        public bool WasInjured { get; set; }
        public bool IsJammer { get; set; }
        public bool IsPivot { get; set; }
        public IList<BoxTimeModel> BoxTimes {get;set;}
    }

    public class JamLineupModel
    {
        public bool IsFirstHalf { get; set; }
        public int JamNumber { get; set; }
        public IList<PlayerLineupModel> HomeLineup { get; set; }
        public IList<PlayerLineupModel> AwayLineup { get; set; }
    }
}
