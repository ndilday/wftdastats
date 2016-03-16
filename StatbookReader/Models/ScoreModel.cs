using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatbookReader.Models
{
    public class ScoreModel
    {
        public string PlayerNumber { get; set; }
        public bool Lost { get; set; }
        public bool Lead { get; set; }
        public bool Called { get; set; }
        public bool Injury { get; set; }
        public bool NoPass { get; set; }
        public int JamTotal { get; set; }
    }

    public class JamScoreModel
    {
        public bool IsFirstHalf { get; set; }
        public int JamNumber { get; set; }
        public ScoreModel HomeJammer { get; set; }
        public ScoreModel AwayJammer { get; set; }
        public ScoreModel HomeStarPass { get; set; }
        public ScoreModel AwayStarPass { get; set; }
    }
}
