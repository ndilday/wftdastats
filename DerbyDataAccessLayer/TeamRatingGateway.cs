using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class TeamRatingGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_InsertTeamRatingBase = "INSERT INTO TeamRating VALUES";
        private const string s_InsertTeamRatingParameter = "\n({0}, {1}, {2}, {3}, {4}, {5}, GETDATE()),";

        private const string s_GetAllTeamRatingsQuery = @"
SELECT tr.*, t.Name as TeamName, CASE WHEN l.Name = 'Women''s Flat Track Derby Association' THEN t.Name ELSE l.Name END as LeagueName
FROM TeamRating tr
JOIN Team t ON tr.TeamID = t.ID
JOIN League l ON l.ID = t.LeagueID
";
        private const string s_GetCurrentTeamRatingsQuery = @"
SELECT tr.*, t.Name as TeamName, CASE WHEN l.Name = 'Women''s Flat Track Derby Association' THEN t.Name ELSE l.Name END as LeagueName
FROM TeamRating tr
JOIN Team t ON tr.TeamID = t.ID
JOIN League l ON l.ID = t.LeagueID
JOIN 
(
    SELECT TeamID, MAX(AddedDate) AS AddedDate
    FROM TeamRating
    GROUP BY TeamID
) maxDates ON tr.TeamID = maxDates.TeamID AND tr.AddedDate = maxDates.AddedDate
";
        #endregion
        public TeamRatingGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public IList<TeamRating> GetAllTeamRatings()
        {
            var data = new List<TeamRating>(250);
            using (var cmd = new SqlCommand(s_GetAllTeamRatingsQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(ReadData(reader));
                    }
                }
            }
            return data;
        }

        public IList<TeamRating> GetCurrentTeamRatings()
        {
            var data = new List<TeamRating>(250);
            using (var cmd = new SqlCommand(s_GetCurrentTeamRatingsQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(ReadData(reader));
                    }
                }
            }
            return data;
        }

        public void InsertTeamRatings(IList<TeamRating> teamRatings)
        {
            int counter = 0;
            string query = s_InsertTeamRatingBase;
            foreach (TeamRating teamRating in teamRatings)
            {
                query += String.Format(s_InsertTeamRatingParameter,
                                       teamRating.TeamID,
                                       teamRating.WftdaRank,
                                       teamRating.WftdaScore,
                                       teamRating.WftdaStrength,
                                       teamRating.FtsRank,
                                       teamRating.FtsScore);
                counter++;
                if (counter > 990)
                {
                    // to avoid ever hitting the 1000 limit, run now and start a new query for the rest
                    using (var cmd = new SqlCommand(query.TrimEnd(','), _connection, _transaction))
                    {
                        cmd.Parameters.Clear();
                        cmd.ExecuteNonQuery();
                    }
                    query = s_InsertTeamRatingBase;
                    counter = 0;
                }
            }
            if (counter > 0)
            {
                query = query.TrimEnd(',');
                using (var cmd = new SqlCommand(query, _connection, _transaction))
                {
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal TeamRating ReadData(SqlDataReader reader)
        {
            TeamRating data = new TeamRating();
            data.TeamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
            data.TeamName = reader.GetString(reader.GetOrdinal("TeamName"));
            data.LeagueName = reader.GetString(reader.GetOrdinal("LeagueName"));
            data.WftdaRank = reader.GetInt32(reader.GetOrdinal("WftdaRank"));
            data.WftdaScore = reader.GetDouble(reader.GetOrdinal("WftdaScore"));
            data.WftdaStrength = reader.GetDouble(reader.GetOrdinal("WftdaStrength"));
            data.FtsRank = reader.GetInt32(reader.GetOrdinal("FtsRank"));
            data.FtsScore = reader.GetDouble(reader.GetOrdinal("FtsScore"));
            data.AddedDate = reader.GetDateTime(reader.GetOrdinal("AddedDate"));
            return data;
        }
    }
}
