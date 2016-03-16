using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class PlayerTrueSkillGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_DeletePlayerTrueSkillsQuery = "DELETE FROM PlayerTrueSkill\n";
        private const string s_InsertPlayerTrueSkillsBase = "INSERT INTO PlayerTrueSkill VALUES";
        private const string s_InsertPlayerTrueSkillsParameter = "\n({0}, {1}, {2}, {3}, '{4}'),";
        private const string s_GetAllPlayerTrueSkillsQuery = "SELECT * FROM PlayerTrueSkill";
        #endregion

        public PlayerTrueSkillGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public void InsertPlayerTrueSkills(IList<PlayerTrueSkill> ptss)
        {
            int counter = 0;
            string query = s_DeletePlayerTrueSkillsQuery + s_InsertPlayerTrueSkillsBase;
            foreach (PlayerTrueSkill pts in ptss)
            {
                query += String.Format(s_InsertPlayerTrueSkillsParameter,
                                        pts.PlayerID,
                                        pts.IsJammer ? 1 : 0,
                                        pts.Mean,
                                        pts.StdDev,
                                        pts.LastUpdated);
                counter++;
                if (counter > 990)
                {
                    // to avoid ever hitting the 1000 limit, run now and start a new query for the rest
                    using (var cmd = new SqlCommand(query.TrimEnd(','), _connection, _transaction))
                    {
                        cmd.Parameters.Clear();
                        cmd.ExecuteNonQuery();
                    }
                    query = s_InsertPlayerTrueSkillsBase;
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

        public Dictionary<int, PlayerTrueSkill> GetAllPlayerTrueSkills()
        {
            var data = new Dictionary<int, PlayerTrueSkill>();
            using (var cmd = new SqlCommand(s_GetAllPlayerTrueSkillsQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int playerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
                        bool isJammer = reader.GetBoolean(reader.GetOrdinal("IsJammer"));
                        double mean = reader.GetDouble(reader.GetOrdinal("Mean"));
                        double stdDev = reader.GetDouble(reader.GetOrdinal("StdDev"));
                        DateTime lastDate = reader.GetDateTime(reader.GetOrdinal("LastUpdated"));

                        data[playerID] = new PlayerTrueSkill
                        {
                            PlayerID = playerID,
                            IsJammer = isJammer,
                            Mean = mean,
                            StdDev = stdDev,
                            LastUpdated = lastDate
                        };
                    }
                }
            }
            return data;
        }
    }
}
