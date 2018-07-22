using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class JamDataGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_GetAllJamDataQuery = "SELECT * FROM Jam_Team_Data_View";
        private const string s_GetJamDataForTeamQuery = "SELECT * FROM Jam_Team_Data_View WHERE TeamID = @TeamID";
        #endregion

        public JamDataGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        private JamTeamData ReadData(SqlDataReader reader)
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

        public IList<JamTeamData> GetAllJamData()
        {
            IList<JamTeamData> jamFouls = new List<JamTeamData>();
            using (var cmd = new SqlCommand(s_GetAllJamDataQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    while (reader.Read())
                    {
                        jamFouls.Add(ReadData(reader));
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
                        jamFouls.Add(ReadData(reader));
                    }
                    reader.Close();
                }
            }
            return jamFouls;
        }
    }
}
