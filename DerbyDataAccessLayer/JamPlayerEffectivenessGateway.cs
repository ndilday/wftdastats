using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class JamPlayerEffectivenessGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_DeleteJamPlayerEffectiveness = "DELETE FROM Jam_Player_Effectiveness\n";
        private const string s_InsertJamPlayerEffectivenessBase = "INSERT INTO Jam_Player_Effectiveness VALUES";
        private const string s_InsertJamPlayerEffectivenessParameter = "\n({0}, {1}, {2}, {3}, {4}),";
        private const string s_GetAllJamPlayerEffectivenessQuery = @"
SELECT jpe.*, jp.IsJammer, tp.TeamID
FROM Jam_Player_Effectiveness jpe
JOIN Team_Player tp ON tp.PlayerID = jpe.PlayerID
JOIN Jam_Player jp ON jp.Team_PlayerID = tp.ID  AND jp.JamID = jpe.JamID";
        private const string s_GetPlayerJamEffectivenessForTeamQuery = @"
SELECT jpe.*, jp.IsJammer, tp.TeamID
FROM Jam_Player_Effectiveness jpe
JOIN Team_Player tp ON tp.PlayerID = jpe.PlayerID
JOIN Jam_Player jp ON jp.Team_PlayerID = tp.ID  AND jp.JamID = jpe.JamID
WHERE
    tp.TeamID = @TeamID
ORDER BY PlayerID, JamID";
        #endregion

        public JamPlayerEffectivenessGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public void InsertJamPlayerEffectiveness(Dictionary<int, List<JamPlayerEffectiveness>> pjeMap)
        {
            int counter = 0;
            string query = s_DeleteJamPlayerEffectiveness + s_InsertJamPlayerEffectivenessBase;
            foreach (List<JamPlayerEffectiveness> pjeList in pjeMap.Values)
            {
                foreach (JamPlayerEffectiveness pje in pjeList)
                {
                    query += String.Format(s_InsertJamPlayerEffectivenessParameter,
                                           pje.JamID,
                                           pje.PlayerID,
                                           pje.JamPortion,
                                           pje.PenaltyCost,
                                           pje.BaseQuality);
                    counter++;
                    if (counter > 990)
                    {
                        // to avoid ever hitting the 1000 limit, run now and start a new query for the rest
                        using (var cmd = new SqlCommand(query.TrimEnd(','), _connection, _transaction))
                        {
                            cmd.Parameters.Clear();
                            cmd.ExecuteNonQuery();
                        }
                        query = s_InsertJamPlayerEffectivenessBase;
                        counter = 0;
                    }
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
    
        public Dictionary<int, Dictionary<int, JamPlayerEffectiveness>> GetAllJamPlayerEffectiveness()
        {
            var data = new Dictionary<int, Dictionary<int, JamPlayerEffectiveness>>();
            using (var cmd = new SqlCommand(s_GetAllJamPlayerEffectivenessQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int jamID = reader.GetInt32(reader.GetOrdinal("JamID"));
                        int playerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
                        int teamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
                        double jamPortion = reader.GetDouble(reader.GetOrdinal("jamPortion"));
                        double penaltyCost = reader.GetDouble(reader.GetOrdinal("PenaltyCost"));
                        double baseEffectiveness = reader.GetDouble(reader.GetOrdinal("BaseEffectiveness"));
                        bool isJammer = reader.GetBoolean(reader.GetOrdinal("IsJammer"));
                        if (!data.ContainsKey(jamID))
                        {
                            data[jamID] = new Dictionary<int, JamPlayerEffectiveness>();
                        }
                        data[jamID][playerID] = new JamPlayerEffectiveness
                        {
                            PlayerID = playerID,
                            JamID = jamID,
                            TeamID = teamID,
                            BaseQuality = baseEffectiveness,
                            IsJammer = isJammer,
                            JamPortion = jamPortion,
                            PenaltyCost = penaltyCost
                        };
                    }
                }
            }
            return data;
        }

        public Dictionary<int, Dictionary<int, JamPlayerEffectiveness>> GetJamPlayerEffectivenessForTeam(int teamID)
        {
            var data = new Dictionary<int, Dictionary<int, JamPlayerEffectiveness>>();
            using (var cmd = new SqlCommand(s_GetPlayerJamEffectivenessForTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int jamID = reader.GetInt32(reader.GetOrdinal("JamID"));
                        int playerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
                        double jamPortion = reader.GetDouble(reader.GetOrdinal("jamPortion"));
                        double penaltyCost = reader.GetDouble(reader.GetOrdinal("PenaltyCost"));
                        double baseEffectiveness = reader.GetDouble(reader.GetOrdinal("BaseEffectiveness"));
                        bool isJammer = reader.GetBoolean(reader.GetOrdinal("IsJammer"));
                        if (!data.ContainsKey(jamID))
                        {
                            data[jamID] = new Dictionary<int, JamPlayerEffectiveness>();
                        }
                        data[jamID][playerID] = new JamPlayerEffectiveness
                        {
                            PlayerID = playerID,
                            JamID = jamID,
                            TeamID = teamID,
                            BaseQuality = baseEffectiveness,
                            IsJammer = isJammer,
                            JamPortion = jamPortion,
                            PenaltyCost = penaltyCost
                        };
                    }
                }
            }
            return data;
        }
    }
}
