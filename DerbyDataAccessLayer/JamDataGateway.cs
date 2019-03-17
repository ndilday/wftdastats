using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class JamDataGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_GetAllJamTeamDataQuery = "SELECT * FROM Jam_Team_Data_View";
        private const string s_GetJamDataForTeamQuery = "SELECT * FROM Jam_Team_Data_View WHERE TeamID = @TeamID";
        private const string s_GetAllJamDataQuery = 
@"SELECT j.ID as JamID, b.PlayDate, t1.TeamTypeId AS HomeTeamType, t2.TeamTypeId AS AwayTeamType 
FROM Jam j 
JOIN Bout b ON b.ID = j.BoutId 
JOIN Team t1 ON b.HomeTeamID = t1.ID 
JOIN Team t2 ON b.AwayTeamID = t2.ID";
        #endregion

        public JamDataGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        private JamTeamData ReadJamTeamData(SqlDataReader reader)
        {
            JamTeamData jamFoul = new JamTeamData();
            jamFoul.JamID = reader.GetInt32(reader.GetOrdinal("JamID"));
            jamFoul.TeamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
            jamFoul.BlockerBoxTime = reader.GetInt32(reader.GetOrdinal("BlockerBoxTime"));
            jamFoul.JammerBoxTime = reader.GetInt32(reader.GetOrdinal("JammerBoxTime"));
            jamFoul.OppBlockerBoxTime = reader.GetInt32(reader.GetOrdinal("OppBlockerBoxTime"));
            jamFoul.OppJammerBoxTime = reader.GetInt32(reader.GetOrdinal("OppJammerBoxTime"));
            jamFoul.PointDelta = reader.GetInt32(reader.GetOrdinal("PointDelta"));
            return jamFoul;
        }

        private JamData ReadJamData(SqlDataReader reader)
        {
            JamData jamData = new JamData();
            jamData.JamID = reader.GetInt32(reader.GetOrdinal("JamID"));
            jamData.PlayDate = reader.GetDateTime(reader.GetOrdinal("PlayDate"));
            jamData.HomeTeamType = reader.GetInt32(reader.GetOrdinal("HomeTeamType"));
            jamData.AwayTeamType = reader.GetInt32(reader.GetOrdinal("AwayTeamType"));
            return jamData;
        }

        public IList<JamData> GetAllJamData()
        {
            IList<JamData> jamData = new List<JamData>();
            using (var cmd = new SqlCommand(s_GetAllJamDataQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    while (reader.Read())
                    {
                        jamData.Add(ReadJamData(reader));
                    }
                    reader.Close();
                }
            }
            return jamData;
        }

        public IList<JamTeamData> GetAllJamTeamData()
        {
            IList<JamTeamData> jamFouls = new List<JamTeamData>();
            using (var cmd = new SqlCommand(s_GetAllJamTeamDataQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    while (reader.Read())
                    {
                        jamFouls.Add(ReadJamTeamData(reader));
                    }
                    reader.Close();
                }
            }
            return jamFouls;
        }

        public IList<JamTeamData> GetJamDataForTeam(int teamID)
        {
            IList<JamTeamData> jamFouls = new List<JamTeamData>();
            using (var cmd = new SqlCommand(s_GetJamDataForTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    while (reader.Read())
                    {
                        jamFouls.Add(ReadJamTeamData(reader));
                    }
                    reader.Close();
                }
            }
            return jamFouls;
        }
    }
}
