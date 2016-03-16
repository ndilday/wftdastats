using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class TeamGateway : DerbyGatewayBase
    {
        #region Queries
        internal const string s_GetRinxterTeamQuery = @"
SELECT t.RinxterID, t.Name, t.LeagueID, t.ID, tt.Name AS TeamType
FROM Team t
Join TeamType tt ON tt.ID = t.TeamTypeID
WHERE t.RinxterID = @ID";

        internal const string s_GetTeamQuery = @"
SELECT t.RinxterID, t.Name, t.LeagueID, t.ID, tt.Name AS TeamType
FROM Team t
Join TeamType tt ON tt.ID = t.TeamTypeID
WHERE 
    t.Name = @Name AND
    t.LeagueID = @LeagueID";

        internal const string s_GetATeamQuery = @"
SELECT t.RinxterID, t.Name, t.LeagueID, t.ID, tt.Name AS TeamType
FROM Team t
Join TeamType tt ON tt.ID = t.TeamTypeID
WHERE 
    t.TeamTypeID = 1 AND
    t.LeagueID = @LeagueID";

        internal const string s_AddTeamQuery = @"
INSERT INTO Team 
SELECT @ID, @Name, @LeagueID, ID
FROM TeamType
WHERE
    Name = @TeamType";

        internal const string s_GetAllTeamsQuery = @"
SELECT t.RinxterID, t.Name, t.LeagueID, t.ID, tt.Name AS TeamType
FROM Team t
JOIN TeamType tt ON tt.ID = t.TeamTypeID";

        internal const string s_GetAllWftdaTeamsQuery = @"
SELECT t.RinxterID, t.Name, t.LeagueID, t.ID, tt.Name AS TeamType
FROM Team t
JOIN TeamType tt ON tt.ID = 1";

        internal const string s_GetTeamsWithBoutQuery = @"
SELECT DISTINCT t.RinxterID, t.Name, t.LeagueID, t.ID, tt.Name AS TeamType
FROM Team t
JOIN TeamType tt ON tt.ID = t.TeamTypeID
JOIN Bout b ON b.AwayTeamID = t.ID OR b.HomeTeamID = t.ID";
        #endregion

        public TeamGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public Team GetRinxterTeam(int id, string teamName, int leagueID, string teamType)
        {
            using (var cmd = new SqlCommand(s_GetRinxterTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read()) 
                    {
                        reader.Close();
                        return AddTeam(id, teamName, leagueID, teamType);
                    }
                    return ReadData(reader);
                }
            }
        }

        public IList<Team> GetAllWftdaTeams()
        {
            var data = new List<Team>(250);
            using (var cmd = new SqlCommand(s_GetAllWftdaTeamsQuery, _connection, _transaction))
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

        public IList<Team> GetAllTeams()
        {
            var data = new List<Team>(250);
            using (var cmd = new SqlCommand(s_GetAllTeamsQuery, _connection, _transaction))
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

        public IList<Team> GetTeamsWithBouts()
        {
            var data = new List<Team>(250);
            using (var cmd = new SqlCommand(s_GetTeamsWithBoutQuery, _connection, _transaction))
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

        public Team GetTeam(string teamName, int leagueID, string teamType, bool allowInsert)
        {
            using (var cmd = new SqlCommand(s_GetTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = teamName;
                cmd.Parameters.Add("@LeagueID", SqlDbType.Int).Value = leagueID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        reader.Close();
                        if(!allowInsert)
                        {
                            throw new InvalidOperationException(string.Format("Team {0} does not exist in league {1}", teamName, leagueID));
                        }
                        return AddTeam(null, teamName, leagueID, teamType);
                    }
                    return ReadData(reader);
                }
            }
        }

        public Team GetATeam(int leagueID)
        {
            using (var cmd = new SqlCommand(s_GetATeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@LeagueID", SqlDbType.Int).Value = leagueID;

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        reader.Close();
                        throw new InvalidOperationException(string.Format("No A Team in league {0}", leagueID));
                    }
                    return ReadData(reader);
                }
            }
        }


        public Team GetTeam(string teamName)
        {
            using (var cmd = new SqlCommand(s_GetTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = teamName;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        reader.Close();
                        return null;
                    }
                    return ReadData(reader);
                }
            }
        }

        internal Team AddTeam(int? rinxterID, string teamName, int leagueID, string teamType)
        {
            using (var cmd = new SqlCommand(s_AddTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                if (rinxterID == null)
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = DBNull.Value;
                }
                else
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = rinxterID;
                }
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = teamName;
                cmd.Parameters.Add("@LeagueID", SqlDbType.Int).Value = leagueID;
                cmd.Parameters.Add("@TeamType", SqlDbType.NVarChar).Value = teamType;
                cmd.ExecuteNonQuery();
                if (rinxterID != null)
                {
                    return GetRinxterTeam((int)rinxterID, teamName, leagueID, teamType);
                }
                return GetTeam(teamName, leagueID, teamType, true);
            }
        }

        internal Team ReadData(SqlDataReader reader)
        {
            Team team = new Team();
            team.ID = reader.GetInt32(reader.GetOrdinal("ID"));
            team.Name = reader.GetString(reader.GetOrdinal("Name"));
            team.LeagueID = reader.GetInt32(reader.GetOrdinal("LeagueID"));
            team.TeamType = reader.GetString(reader.GetOrdinal("TeamType"));
            team.RinxterID = reader.IsDBNull(reader.GetOrdinal("RinxterID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("RinxterID"));
            return team;
        }
    }
}
