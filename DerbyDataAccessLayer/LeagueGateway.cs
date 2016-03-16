using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class LeagueGateway : DerbyGatewayBase
    {
        #region Queries
        internal const string s_GetLeagueQuery = @"
SELECT * 
FROM League
WHERE 
    ID = @LeagueID";

        internal const string s_GetAllLeaguesQuery = "SELECT * FROM League";

        internal const string s_AddRinxterLeagueQuery = "INSERT INTO League Values (@LeagueID, @Name, @JoinDate)";
        #endregion

        public LeagueGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public IList<League> GetAllLeagues()
        {
            var data = new List<League>(250);
            using (var cmd = new SqlCommand(s_GetAllLeaguesQuery, _connection, _transaction))
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

        public League GetLeague(int leagueID, string name, DateTime joinDate, bool allowInsert)
        {
            using (var cmd = new SqlCommand(s_GetLeagueQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@LeagueID", SqlDbType.Int).Value = leagueID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        reader.Close();
                        if (allowInsert)
                        {
                            return AddAndGetLeague(leagueID, name, joinDate);
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("League {0} does not exist.", name));
                        }
                    }
                    return ReadData(reader);
                }
            }
        }

        internal League AddAndGetLeague(int leagueID, string name, DateTime joinDate)
        {
            using (var cmd = new SqlCommand(s_AddRinxterLeagueQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@LeagueID", SqlDbType.Int).Value = leagueID;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@JoinDate", SqlDbType.DateTime).Value = joinDate;
                cmd.ExecuteNonQuery();
                return GetLeague(leagueID, name, joinDate, false);
            }
        }

        internal League ReadData(SqlDataReader reader)
        {
            League league = new League();
            league.ID = reader.GetInt32(reader.GetOrdinal("ID"));
            league.Name = reader.GetString(reader.GetOrdinal("Name"));
            league.JoinDate = reader.GetDateTime(reader.GetOrdinal("JoinDate"));
            return league;
        }
    }
}
