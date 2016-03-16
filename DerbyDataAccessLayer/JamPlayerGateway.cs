using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class JamPlayerGateway : DerbyGatewayBase
    {
        #region Queries
        internal const string s_GetJamPlayerQuery = @"
SELECT *
FROM Jam_Player jp
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
WHERE
    tp.PlayerID = @PlayerID AND
    JamID = @JamID";

        internal const string s_GetJamPlayerByNameAndNumberQuery = @"
SELECT *
FROM Jam_Player jp
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Player p ON p.ID = tp.PlayerID
WHERE
    p.Name = @PlayerName AND
    p.Number = @PlayerNumber AND
    JamID = @JamID";

        internal const string s_GetJamPlayerByNameQuery = @"
SELECT *
FROM Jam_Player jp
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Player p ON p.ID = tp.PlayerID
WHERE
    p.Name = @PlayerName AND
    JamID = @JamID";

        internal const string s_AddJamPlayerQuery = @"
INSERT INTO Jam_Player 
SELECT @JamID, tp.ID, @IsJammer, @IsPivot
FROM Team_Player tp
WHERE
    tp.PlayerID = @PlayerID AND
    tp.TeamID = @TeamID";

        internal const string s_GetPlayersByJamQuery = @"
SELECT *
FROM Jam_Player jp
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Player p ON p.ID = tp.PlayerID
WHERE
    jp.JamID = @JamID";

        internal const string s_GetPlayersQuery = @"
SELECT jp.ID, tp.TeamID, tp.PlayerID, jp.IsPivot, jp.JamID, jp.IsJammer
FROM Jam_Player jp
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Player p ON p.ID = tp.PlayerID
";
        #endregion

        public JamPlayerGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public JamPlayer GetJamPlayer(int jamID, int playerID)
        {
            using (var cmd = new SqlCommand(s_GetJamPlayerQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = playerID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, we can't fix it here
                    if (!reader.Read())
                    {
                        return null;
                    }
                    return ReadData(reader);
                }
            }
        }

        public JamPlayer GetJamPlayer(int jamID, string playerName, string playerNumber)
        {
            using (var cmd = new SqlCommand(s_GetJamPlayerByNameAndNumberQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;
                cmd.Parameters.Add("@PlayerName", SqlDbType.NVarChar).Value = playerName;
                cmd.Parameters.Add("@PlayerNumber", SqlDbType.NVarChar).Value = playerNumber;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, we can't fix it here
                    if (!reader.Read())
                    {
                        return null;
                    }
                    return ReadData(reader);
                }
            }
        }

        public JamPlayer GetJamPlayerByName(int jamID, string playerName)
        {
            using (var cmd = new SqlCommand(s_GetJamPlayerByNameQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;
                cmd.Parameters.Add("@PlayerName", SqlDbType.NVarChar).Value = playerName;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, we can't fix it here
                    if (!reader.Read())
                    {
                        return null;
                    }
                    return ReadData(reader);
                }
            }
        }

        public JamPlayer AddJamPlayer(int jamID, int playerID, int teamID, bool isJammer, bool isPivot)
        {
            // create the player
            using (var cmd = new SqlCommand(s_AddJamPlayerQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = playerID;
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                cmd.Parameters.Add("@IsJammer", SqlDbType.Bit).Value = isJammer;
                cmd.Parameters.Add("@IsPivot", SqlDbType.Bit).Value = isPivot;
                cmd.ExecuteNonQuery();
            }
            return GetJamPlayer(jamID, playerID);
        }

        public IList<JamPlayer> GetJamPlayersByJam(int jamID)
        {
            List<JamPlayer> list = new List<JamPlayer>();
            using (var cmd = new SqlCommand(s_GetPlayersByJamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        list.Add(ReadData(reader));
                    }
                    return list;
                }
            }
        }

        public IList<JamPlayer> GetJamPlayers()
        {
            List<JamPlayer> list = new List<JamPlayer>();
            using (var cmd = new SqlCommand(s_GetPlayersQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        list.Add(ReadData(reader));
                    }
                    return list;
                }
            }
        }

        internal JamPlayer ReadData(SqlDataReader reader)
        {
            JamPlayer player = new JamPlayer();
            player.ID = reader.GetInt32(reader.GetOrdinal("ID"));
            player.TeamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
            player.PlayerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
            player.JamID = reader.GetInt32(reader.GetOrdinal("JamID"));
            player.IsJammer = reader.GetBoolean(reader.GetOrdinal("IsJammer"));
            player.IsPivot = reader.GetBoolean(reader.GetOrdinal("IsPivot"));
            return player;
        }
    }
}
