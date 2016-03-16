using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    public class TeamPlayerPerformanceCalculator
    {
        string _connectionString;

        public TeamPlayerPerformanceCalculator(string connString)
        {
            _connectionString = connString;
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            // get bouts
            var bouts = new BoutGateway(connection, transaction).GetBouts().OrderBy(b => b.BoutDate);
            IList<Jam> jams = new JamGateway(connection, transaction).GetAllJams();
            IList<JamPlayer> jamPlayers = new JamPlayerGateway(connection, transaction).GetJamPlayers();

            var jamPlayerMap = jamPlayers.GroupBy(jp => jp.JamID).ToDictionary(g => g.Key, g => g.ToList());

            foreach(Bout bout in bouts)
            {
                var boutJams = jams.Where(j => j.BoutID == bout.ID);

                // get teamplayerperformance values
                // scrape FTS?
                // get FTS data for this bout (input ratings, differential, ratings change)

                // Calculate an estimated Team Str based on play rate of players
                double homeTeamStrength, awayTeamStrength;
                CalculateEstimatedTeamStrength(boutJams, bout, jamPlayerMap, out homeTeamStrength, out awayTeamStrength);

                // Calculate Expected Ratio from Team Str comparison
                CalculateExpectedHomeTeamShare(bout, homeTeamStrength, awayTeamStrength);

                // Calculate Expected Delta (Modify both sides scores equally?)
                // Convert expected delta to PerfAgainstAverage
                // ??? Compare Sum of PerfAgainstAverage to Actual Delta
                // ??? Adjust player Ratings to align Team Str with results
            }
        }

        private void CalculateEstimatedTeamStrength(IEnumerable<Jam> boutJams, Bout bout, Dictionary<int, List<JamPlayer>> jamPlayerMap, 
                                                    out double homeTeamStrength, out double awayTeamStrength)
        {
            // we're going to keep this estimate at a per-jam mentality, for now
            int totalJams = boutJams.Count();
            double jammerPortion = 0.5 / totalJams;
            double blockerPortion = 0.125 / totalJams;
            
            homeTeamStrength = 0;
            awayTeamStrength = 0;
            double totalRating = 0;
            foreach(Jam jam in boutJams)
            {
                var players = jamPlayerMap[jam.ID];
                foreach (JamPlayer jamPlayer in players)
                {
                    //totalRating = jp.IsJammer ? jammerPortion * jp.JammerRating : blockerPortion * jp.BlockerRating;
                    if(jamPlayer.TeamID == bout.HomeTeamID)
                    {
                        homeTeamStrength += totalRating;
                    }
                    else if(jamPlayer.TeamID == bout.AwayTeamID)
                    {
                        awayTeamStrength += totalRating;
                    }
                    else
                    {
                        throw new InvalidOperationException("Bad player");
                    }
                }
            }
        }

        private void CalculateExpectedHomeTeamShare(Bout bout, double homeTeamStrength, double awayTeamStrength)
        {
            throw new NotImplementedException();
        }
    }
}
