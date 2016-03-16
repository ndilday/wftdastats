using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatbookReader.Models
{
    public class StatbookModel
    {
        public TeamModel HomeTeam { get; set; }
        public TeamModel AwayTeam { get; set; }
        public DateTime Date { get; set; }
        public string VenueName { get; set; }
        public string VenueCity { get; set; }
        public IList<OfficialModel> Officials { get; set; }
        public PenaltiesModel Penalties { get; set; }
        public IList<JamLineupModel> Lineups { get; set; }
        public IList<JamScoreModel> Scores { get; set; }
    }
}
