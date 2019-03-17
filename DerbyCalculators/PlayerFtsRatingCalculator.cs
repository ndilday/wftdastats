using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DerbyCalculators.Models;
using DerbyDataAccessLayer;
using DerbyDataModels;
using FTSReader;

namespace DerbyCalculators
{
    class PlayerFtsRatingCalculator
    {
        static DateTime STATS_START_DATE = new DateTime(2018, 1, 1);
        string _connectionString;
        public PlayerFtsRatingCalculator(string connString)
        {
            _connectionString = connString;
        }

        public void GetPlayerRatingPerformancesForTeam(int teamId)
        {
            Dictionary<int, Dictionary<int, BoutPerformance>> boutPlayerPerformanceMap = new Dictionary<int, Dictionary<int, BoutPerformance>>();
            //pull point data
            IList<PlayerPerformance> playerPerformanceList = new PlayerPerformanceCalculator(_connectionString).GetPlayerPointPerformancesForTeam(teamId);
            // pull fts data
            FTSScraper scraper = new FTSScraper();
            List<TeamGameRatingData> ftsData = scraper.GetTeamRatingHistory(3402);
            // TODO: figure out how to translate between our internal team IDs and FTS IDs
            
            //splice the player performance records to be clustered by bout, rather than player
            foreach(PlayerPerformance playerPerformance in playerPerformanceList)
            {
                foreach(BoutPerformance boutPerformance in playerPerformance.Bouts)
                {
                    if(!boutPlayerPerformanceMap.ContainsKey(boutPerformance.BoutID))
                    {
                        boutPlayerPerformanceMap[boutPerformance.BoutID] = new Dictionary<int, BoutPerformance>();
                    }
                    boutPlayerPerformanceMap[boutPerformance.BoutID][playerPerformance.Player.ID] = boutPerformance;
                }
            }

            // pair games from the fts record with games from the playerPerformance record
        }
    }
}
