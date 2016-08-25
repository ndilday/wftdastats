using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatbookReader.Models.Rinxter
{
    public class RinxterTournamentModel
    {
        public RinxterBoutModel[] rows;
    }
    public class RinxterBoutModel
    {
        public string away;
        public DateTime date;
        public string descriptionExt;
        public string descriptionFull;
        public string home;
        public int id;
        public string outcome;
        public string santion;
        public string status;
        public string supplemental;
        public string team1;
        public string team1Name;
        public int team1Score;
        public string team2;
        public string team2Name;
        public int team2Score;
        public string tournament;
        public string venue;
        public string venueExt;
        public string venueS;
    }
    public class RinxterBoutData
    {
        public int id;
        public int team1Id;
        public int team2Id;
    }
}
