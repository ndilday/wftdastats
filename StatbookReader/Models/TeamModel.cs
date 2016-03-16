using System;
using System.Collections.Generic;

namespace StatbookReader.Models
{
    public class TeamModel
    {
        public string Name { get; set; }
        public string LeagueName { get; set; }
        public string Color { get; set; }
        public IList<PlayerModel> Players { get; set; }
    }
}
