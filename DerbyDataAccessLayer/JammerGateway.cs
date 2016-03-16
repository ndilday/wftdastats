using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class JammerGateway : DerbyGatewayBase
    {
        #region Queries
        const string s_GetJammerQuery = @"
SELECT j.*, tp.TeamID
FROM Jammer j
JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
WHERE
    jp.JamID = @JamID AND
    tp.PlayerID = @PlayerID";

        const string s_AddJammerQuery = @"
INSERT INTO Jammer
SELECT jp.ID, @Points, @Lost, @Lead, @Called, @Injury, @NoPass, @PassedStar, @ReceivedStar
FROM Jam_Player jp
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
WHERE
    jp.JamID = @JamID AND
    tp.PlayerID = @PlayerID";

        const string s_UpdateJammerPointsQuery = @"
UPDATE j
SET Points = @Points, PassedStar = @PassedStar
FROM Jammer j
JOIN Jam_Player jp ON j.Jam_PlayerID = jp.ID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
WHERE 
    jp.JamID = @JamID AND
    tp.PlayerID = @PlayerID
";
        const string s_GetAllJammersQuery = @"
SELECT j.*, jp.JamID, tp.PlayerID, tp.TeamID
FROM Jammer j
JOIN Jam_Player jp ON j.Jam_PlayerID = jp.ID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
";
        #endregion

        public JammerGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public Jammer GetJammer(int jamID, int playerID)
        {
            using (var cmd = new SqlCommand(s_GetJammerQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = playerID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        // throw exception?
                        return null;
                    }
                    return ReadData(reader, jamID, playerID);
                }
            }
        }

        public IList<Jammer> GetAllJammers()
        {
            var dataList = new List<Jammer>();
            using (var cmd = new SqlCommand(s_GetAllJammersQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = ReadData(reader);
                        dataList.Add(data);
                    }
                }
            }
            return dataList;
        }

        public Jammer AddJammer(int jamID, int playerID, int points, bool lostLead, bool lead, bool called, bool injury, bool noPass, bool passedStar, bool receivedStar)
        {
            using (var cmd = new SqlCommand(s_AddJammerQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = playerID;
                cmd.Parameters.Add("@Points", SqlDbType.Int).Value = points;
                cmd.Parameters.Add("@Lost", SqlDbType.Bit).Value = lostLead;
                cmd.Parameters.Add("@Lead", SqlDbType.Bit).Value = lead;
                cmd.Parameters.Add("@Called", SqlDbType.Bit).Value = called;
                cmd.Parameters.Add("@Injury", SqlDbType.Bit).Value = injury;
                cmd.Parameters.Add("@NoPass", SqlDbType.Bit).Value = noPass;
                cmd.Parameters.Add("@PassedStar", SqlDbType.Bit).Value = passedStar;
                cmd.Parameters.Add("@ReceivedStar", SqlDbType.Bit).Value = receivedStar;
                cmd.ExecuteNonQuery();
            }
            return GetJammer(jamID, playerID);
        }

        public Jammer UpdateJammer(int jamID, int playerID, int points, bool passedStar)
        {
            using (var cmd = new SqlCommand(s_UpdateJammerPointsQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = playerID;
                cmd.Parameters.Add("@Points", SqlDbType.Int).Value = points;
                cmd.Parameters.Add("@PassedStar", SqlDbType.Bit).Value = passedStar;
                cmd.ExecuteNonQuery();
            }
            return GetJammer(jamID, playerID);
        }

        private Jammer ReadData(SqlDataReader reader)
        {
            Jammer jammer = new Jammer();
            jammer.JamID = reader.GetInt32(reader.GetOrdinal("JamID"));
            jammer.PlayerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
            jammer.TeamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
            jammer.ID = reader.GetInt32(reader.GetOrdinal("ID"));
            jammer.Lead = reader.GetBoolean(reader.GetOrdinal("Lead"));
            jammer.LostLead = reader.GetBoolean(reader.GetOrdinal("Lost"));
            jammer.Called = reader.GetBoolean(reader.GetOrdinal("Called"));
            jammer.Injury = reader.GetBoolean(reader.GetOrdinal("Injury"));
            jammer.NoPass = reader.GetBoolean(reader.GetOrdinal("NoPass"));
            jammer.PassedStar = reader.GetBoolean(reader.GetOrdinal("PassedStar"));
            jammer.ReceivedStar = reader.GetBoolean(reader.GetOrdinal("ReceivedStar"));
            return jammer;
        }

        private Jammer ReadData(SqlDataReader reader, int jamID, int playerID)
        {
            Jammer jammer = new Jammer();
            jammer.JamID = jamID;
            jammer.PlayerID = playerID;
            jammer.TeamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
            jammer.ID = reader.GetInt32(reader.GetOrdinal("ID"));
            jammer.Lead = reader.GetBoolean(reader.GetOrdinal("Lead"));
            jammer.LostLead = reader.GetBoolean(reader.GetOrdinal("Lost"));
            jammer.Called = reader.GetBoolean(reader.GetOrdinal("Called"));
            jammer.Injury = reader.GetBoolean(reader.GetOrdinal("Injury"));
            jammer.NoPass = reader.GetBoolean(reader.GetOrdinal("NoPass"));
            jammer.PassedStar = reader.GetBoolean(reader.GetOrdinal("PassedStar"));
            jammer.ReceivedStar = reader.GetBoolean(reader.GetOrdinal("ReceivedStar"));
            return jammer;
        }
    }
}
